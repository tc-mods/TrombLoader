using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using Microsoft.FSharp.Core;
using Newtonsoft.Json;
using TrombLoader.Helpers;

namespace TrombLoader.CustomTracks;

public class TrackLoader : TrackRegistrationEvent.Listener, CustomTrackLoader
{
    private JsonSerializer _serializer = new();

    public IEnumerable<TromboneTrack> OnRegisterTracks()
    {
        CreateMissingDirectories();

        var songs = GetSearchPaths()
            .SelectMany(searchPath =>
                Directory.EnumerateFiles(searchPath, Globals.defaultChartName, SearchOption.AllDirectories))
            .Select(Path.GetDirectoryName);

        var sw = Stopwatch.StartNew();
        foreach (var songFolder in songs)
        {
            var track = LoadCustomTrack(songFolder, TrackSource.TrombLoader);
            if (track != null) yield return track;
        }

        sw.Stop();
        Plugin.LogInfo($"Loaded tracks in {sw.Elapsed.TotalSeconds} seconds");
    }

    private TromboneTrack LoadCustomTrack(string songFolder, TrackSource source)
    {
        var chartPath = Path.Combine(songFolder, Globals.defaultChartName);
        var chartName = Path.GetFileName(songFolder.TrimEnd('/'));
        if (!File.Exists(chartPath)) return null;

        using var stream = File.OpenText(chartPath);
        using var reader = new JsonTextReader(stream);

        CustomTrackData customLevel;
        try
        {
            _serializer.Context = new StreamingContext(StreamingContextStates.File, chartName);
            customLevel = _serializer.Deserialize<CustomTrackData>(reader);
        }
        catch (Exception exc)
        {
            Plugin.LogWarning($"Unable to deserialize JSON of custom chart: {chartPath}");
            Plugin.LogWarning(exc.Message);
            return null;
        }

        return new CustomTrack(songFolder, customLevel, this, source);
    }

    public SavedLevel LoadChartData(string folderPath, CustomTrackData data)
    {
        var chartPath = Path.Combine(folderPath, Globals.defaultChartName);
        using var stream = File.OpenText(chartPath);
        using var reader = new JsonTextReader(stream);

        _serializer.Context = new StreamingContext(StreamingContextStates.File, data.trackRef);
        var track = _serializer.Deserialize<ChartData>(reader);
        return track?.ToSavedLevel(data);
    }

    /// <summary>
    /// Get all paths to search for `song.tmb` files
    /// </summary>
    /// <returns>A list of folders to recursively search for song.tmb files</returns>
    private string[] GetSearchPaths() =>
    [
        Globals.GetCustomSongsPath(),
        Paths.PluginPath,
    ];

    private static void CreateMissingDirectories()
    {
        //If the custom folder doesnt exist, create it
        if (!Directory.Exists(Globals.GetCustomSongsPath()))
        {
            Directory.CreateDirectory(Globals.GetCustomSongsPath());
        }
    }

    public LoadingPriority Priority => LoadingPriority.Modded;

    public FSharpOption<TromboneTrack> LoadTrack(string folderPath)
    {
        return OptionModule.OfObj(LoadCustomTrack(folderPath, TrackSource.Other));
    }
}

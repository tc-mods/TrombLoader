using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using BaboonAPI.Hooks.Tracks;
using Newtonsoft.Json;
using TrombLoader.Helpers;

namespace TrombLoader.CustomTracks;

public class TrackLoader: TrackRegistrationEvent.Listener
{
    private JsonSerializer _serializer = new();

    public IEnumerable<TromboneTrack> OnRegisterTracks()
    {
        CreateMissingDirectories();

        var songs = Directory.GetFiles(Globals.GetCustomSongsPath(), "song.tmb", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(BepInEx.Paths.PluginPath, "song.tmb", SearchOption.AllDirectories))
            .Select(Path.GetDirectoryName);

        var seen = new HashSet<string>();
        var sw = Stopwatch.StartNew();
        foreach (var songFolder in songs)
        {
            var chartPath = Path.Combine(songFolder, Globals.defaultChartName);
            var chartName = Path.GetFileName(songFolder.TrimEnd('/'));
            if (!File.Exists(chartPath)) continue;

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
                continue;
            }

            if (customLevel == null) continue;

            if (seen.Add(customLevel.trackRef))
            {
                yield return new CustomTrack(songFolder, customLevel, this);
            }
            else
            {
                Plugin.LogWarning(
                    $"Skipping folder {chartPath} as its trackref '{customLevel.trackRef}' was already loaded!");
            }
        }

        sw.Stop();
        Plugin.LogInfo($"Loaded tracks in {sw.Elapsed.TotalSeconds} seconds");
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

    private static void CreateMissingDirectories()
    {
        //If the custom folder doesnt exist, create it
        if (!Directory.Exists(Globals.GetCustomSongsPath()))
        {
            Directory.CreateDirectory(Globals.GetCustomSongsPath());
        }
    }
}

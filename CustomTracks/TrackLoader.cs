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

public class TrackLoader : TrackRegistrationEvent.Listener
{
    public IEnumerable<TromboneTrack> OnRegisterTracks()
    {
        Stopwatch sw = Stopwatch.StartNew();
        CreateMissingDirectories();

        List<TromboneTrack> tracks = new List<TromboneTrack>();

        //Non recursive method is faster but requires stricter custom songs folder structure
        //var songDirectories = Directory.GetDirectories(Globals.GetCustomSongsPath());

        //Only recursively check inside of the custom songs folder
        var tmbDirectories = Directory.GetFiles(Globals.GetCustomSongsPath(), "song.tmb", SearchOption.AllDirectories);
        var seen = new HashSet<string>();

        //For some reasons more thread ends up being much slower even than single threading, but using a low thread count makes it much faster
        for(int i = 0; i < tmbDirectories.Length; i++)
        {
            CustomTrackData customLevel;
            string dirName;
            try
            {
                dirName = Path.GetDirectoryName(tmbDirectories[i]);
                var chartName = dirName.TrimEnd('/');
                customLevel = JsonConvert.DeserializeObject<CustomTrackData>(File.ReadAllText(tmbDirectories[i]), new JsonSerializerSettings()
                {
                    Context = new StreamingContext(StreamingContextStates.File, chartName)
                }) ?? throw new Exception("Deserializer returned unexpected null value.");
            }
            catch (Exception exc)
            {
                Plugin.LogWarning($"Unable to deserialize JSON of custom chart: {tmbDirectories[i]}");
                Plugin.LogWarning(exc.Message);
                continue;
            }

            if (seen.Add(customLevel.trackRef))
            {
                Plugin.LogDebug($"Found custom chart: {customLevel.trackRef}");

                tracks.Add(new CustomTrack(dirName, customLevel, this));
            }
            else
            {
                Plugin.LogWarning(
                    $"Skipping folder {dirName} as its trackref '{customLevel.trackRef}' was already loaded!");
            }
        };
        sw.Stop();
        Plugin.LogInfo($"{tracks.Count} charts were loaded in {sw.Elapsed.TotalMilliseconds:0.00}ms");
        return tracks.Where(x => x != null);
    }

    public SavedLevel ReloadTrack(CustomTrack existing)
    {
        var chartPath = Path.Combine(existing.folderPath, Globals.defaultChartName);

        var track = JsonConvert.DeserializeObject<CustomTrackData>(File.ReadAllText(chartPath));
        return track?.ToSavedLevel();
    }

    public bool ShouldReloadChart()
    {
        return Plugin.Instance.DeveloperMode.Value;
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BaboonAPI.Hooks.Tracks;
using Newtonsoft.Json;
using TrombLoader.Helpers;

namespace TrombLoader.CustomTracks;

public class TrackLoader : TrackRegistrationEvent.Listener
{
    public IEnumerable<TromboneTrack> OnRegisterTracks()
    {
        CreateMissingDirectories();

        List<TromboneTrack> tracks = new List<TromboneTrack>();

        //Non recursive method is faster but requires stricter custom songs folder structure
        //var songDirectories = Directory.GetDirectories(Globals.GetCustomSongsPath());

        //Only recursively check inside of the custom songs folder
        var songDirectories = Directory.GetFiles(Globals.GetCustomSongsPath(), "song.tmb", SearchOption.AllDirectories);
        var seen = new HashSet<string>();

        //Using more thread isn't faster, tested on 12 logical processor, best result using 3
        Parallel.For(0, songDirectories.Length, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount / 4 }, i =>
        {
            CustomTrackData customLevel;
            try
            {
                var chartName = Path.GetDirectoryName(songDirectories[i]).TrimEnd('/');
                customLevel = JsonConvert.DeserializeObject<CustomTrackData>(File.ReadAllText(songDirectories[i]), new JsonSerializerSettings()
                {
                    Context = new StreamingContext(StreamingContextStates.File, chartName)
                });
            }
            catch (Exception exc)
            {
                Plugin.LogWarning($"Unable to deserialize JSON of custom chart: {songDirectories[i]}");
                Plugin.LogWarning(exc.Message);
                return;
            }

            if (seen.Add(customLevel.trackRef))
            {
                Plugin.LogDebug($"Found custom chart: {customLevel.trackRef}");

                tracks.Add(new CustomTrack(songDirectories[i], customLevel, this));
            }
            else
            {
                Plugin.LogWarning(
                    $"Skipping folder {songDirectories[i]} as its trackref '{customLevel.trackRef}' was already loaded!");
            }
        });
        return tracks;
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

﻿using System.Collections.Generic;
using System.IO;
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
        var songs = Directory.GetDirectories(Globals.GetCustomSongsPath());
        foreach (var songFolder in songs)
        {
            var chartPath = songFolder + "/" + Globals.defaultChartName;
            if (!File.Exists(chartPath)) continue;

            using var stream = File.OpenText(chartPath);
            using var reader = new JsonTextReader(stream);

            var customLevel = _serializer.Deserialize<CustomTrack>(reader);
            if (customLevel == null) continue;

            Plugin.LogDebug($"Found custom chart: {customLevel.trackref}");

            customLevel.folderPath = songFolder;
            yield return customLevel;
        }
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
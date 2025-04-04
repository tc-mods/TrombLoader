﻿using System.IO;
using BaboonAPI.Hooks.Tracks;
using BaboonAPI.Utility;
using JetBrains.Annotations;
using Microsoft.FSharp.Core;
using TrombLoader.CustomTracks.Backgrounds;
using TrombLoader.Helpers;
using UnityEngine;

namespace TrombLoader.CustomTracks;

public class CustomTrack : TromboneTrack, Previewable, FilesystemTrack
{
    /// <summary>
    ///  Folder path that this track can be found at
    /// </summary>
    public string folderPath { get; }

    private readonly CustomTrackData _data;
    private readonly TrackLoader _loader;

    public string trackref => _data.trackRef;
    public string trackname_long => _data.name;
    public string trackname_short => _data.shortName;
    public string year => _data.year.ToString();
    public string artist => _data.author;
    public string desc => _data.description;
    public string genre => _data.genre;
    public int difficulty => _data.difficulty;
    public int tempo => (int) _data.tempo;
    public int length => Mathf.FloorToInt(_data.endpoint / (_data.tempo / 60f));

    public CustomTrack(string folderPath, CustomTrackData data, TrackLoader loader)
    {
        this.folderPath = folderPath;
        _data = data;
        _loader = loader;
    }

    /// <summary>
    /// Get custom data for an identifier
    /// </summary>
    /// <param name="identifier">Key for the data property</param>
    /// <returns>Custom data for the specified identifier, if present on the track, otherwise null</returns>
    [CanBeNull]
    public ExtraData GetCustomData(Identifier identifier)
    {
        return _data.custom_data.TryGetValue(identifier, out var data) ? new ExtraData(identifier, data) : null;
    }

    public SavedLevel LoadChart()
    {
        return _loader.LoadChartData(folderPath, _data);
    }

    public LoadedTromboneTrack LoadTrack()
    {
        return new LoadedCustomTrack(this, LoadBackground());
    }

    // Previewable callback
    public Coroutines.YieldTask<FSharpResult<TrackAudio, string>> LoadClip()
    {
        var previewPath = Path.Combine(folderPath, Globals.defaultPreviewName);
        if (!File.Exists(previewPath))
        {
            previewPath = Path.Combine(folderPath, Globals.defaultAudioName);
        }

        var task = BaboonAPI.Utility.Unity.loadAudioClip(previewPath, AudioType.OGGVORBIS);

        // uh oh here comes the F#
        return Coroutines.map(FuncConvert.FromFunc((FSharpResult<AudioClip, string> res) =>
            ResultModule.Map(FuncConvert.FromFunc((AudioClip clip) =>
                new TrackAudio(clip, 0.9f)), res)), task);
    }

    private AbstractBackground LoadBackground()
    {
        if (File.Exists(Path.Combine(folderPath, "bg.trombackground")))
        {
            var bundle = AssetBundle.LoadFromFile(Path.Combine(folderPath, "bg.trombackground"));
            return new CustomBackground(bundle, folderPath);
        }

        var possibleVideoPath = Path.Combine(folderPath, "bg.mp4");
        if (File.Exists(possibleVideoPath) && !(GlobalVariables.turbomode && Plugin.Instance.turboBackgroundFallback.Value))
        {
            return new VideoBackground(possibleVideoPath);
        }

        var spritePath = Path.Combine(folderPath, "bg.png");
        if (File.Exists(spritePath))
        {
            return new ImageBackground(spritePath);
        }

        Plugin.LogWarning($"No background for track {trackref}");
        return new EmptyBackground();
    }

    public bool IsVisible()
    {
        return true;
    }

    public class LoadedCustomTrack : LoadedTromboneTrack, AsyncAudioAware, PauseAware
    {
        private readonly CustomTrack _parent;
        private readonly AbstractBackground _background;

        public LoadedCustomTrack(CustomTrack parent, AbstractBackground background)
        {
            _parent = parent;
            _background = background;
        }

        public TrackAudio LoadAudio()
        {
            var songPath = Path.Combine(_parent.folderPath, Globals.defaultAudioName);
            var e = Plugin.Instance.GetAudioClipSync(songPath);

            // TODO: is there a sync way of getting audio clips off disk
            while (e.MoveNext())
            {
                switch (e.Current)
                {
                    case AudioClip clip:
                        return new TrackAudio(clip, 1.0f);
                    case string err:
                        Plugin.LogError(err);
                        return null;
                }
            }

            Plugin.LogError("Failed to load audio");
            return null;
        }


        Coroutines.YieldTask<FSharpResult<TrackAudio, string>> AsyncAudioAware.LoadAudio()
        {
            var songPath = Path.Combine(_parent.folderPath, Globals.defaultAudioName);

            var task = BaboonAPI.Utility.Unity.loadAudioClip(songPath, AudioType.OGGVORBIS);
            return Coroutines.map(FuncConvert.FromFunc((FSharpResult<AudioClip, string> res) =>
                ResultModule.Map(FuncConvert.FromFunc((AudioClip clip) =>
                    new TrackAudio(clip, 1f)), res)), task);
        }

        public GameObject LoadBackground(BackgroundContext ctx)
        {
            return _background.Load(ctx);
        }

        public void SetUpBackgroundDelayed(BGController controller, GameObject bg)
        {
            // Fix layering
            // Without this hack, video backgrounds render wrong
            var modelCam = GameObject.Find("3dModelCamera")?.GetComponent<Camera>();
            if (modelCam != null)
            {
                modelCam.clearFlags = CameraClearFlags.Depth;
            }

            _background.SetUpBackground(controller, bg);

            controller.tickontempo = false;

            // Apply background effect
            controller.doBGEffect(_parent._data.backgroundMovement);
        }

        public bool CanResume => _background.CanResume;
        public void OnPause(PauseContext ctx) => _background.OnPause(ctx);
        public void OnResume(PauseContext ctx) => _background.OnResume(ctx);

        public void Dispose()
        {
            _background.Dispose();
        }

        public string trackref => _parent.trackref;
    }
}

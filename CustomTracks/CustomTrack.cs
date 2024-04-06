using System.IO;
using BaboonAPI.Hooks.Tracks;
using BaboonAPI.Utility;
using Microsoft.FSharp.Core;
using TrombLoader.CustomTracks.Backgrounds;
using TrombLoader.Helpers;
using UnityEngine;

namespace TrombLoader.CustomTracks;

public class CustomTrack : TromboneTrack, Previewable
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

    public SavedLevel LoadChart()
    {
        return (bool)_loader?.ShouldReloadChart() ? _loader.ReloadTrack(this) : _data.ToSavedLevel();
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
        var possibleBackgroundPath = Path.Combine(folderPath, "bg.trombackground");
        if (File.Exists(possibleBackgroundPath))
        {
            var bundle = AssetBundle.LoadFromFile(possibleBackgroundPath);
            return new CustomBackground(bundle, folderPath);
        }

        var possibleVideoPath = Path.Combine(folderPath, "bg.mp4");
        if (File.Exists(possibleVideoPath))
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

    public class LoadedCustomTrack : LoadedTromboneTrack, PauseAware
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

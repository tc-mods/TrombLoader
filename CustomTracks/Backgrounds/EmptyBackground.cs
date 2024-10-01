using BaboonAPI.Hooks.Tracks;
using UnityEngine;

namespace TrombLoader.CustomTracks.Backgrounds;

/// <summary>
///  Background used when a custom track doesn't specify one. Configured via TrombLoader.cfg.
/// </summary>
public class EmptyBackground : HijackedBackground
{
    public override void SetUpBackground(BGController controller, GameObject bg)
    {
        switch(Plugin.Instance.DefaultBackground.Value)
        {
            case "grey":
                DisableParts(bg);
                break;
            case "black":
                var camera = bg.GetComponent<Camera>();
                camera.backgroundColor = Color.black;
                DisableParts(bg);
                break;
            case "freeplay-static":
                break;
            default:
                // this is just to enable the background animations
                controller.songname = "freeplay";
                break;
        }
    }

    public override bool CanResume => true;

    public override void OnPause(PauseContext ctx)
    {
        // muffin
    }

    public override void OnResume(PauseContext ctx)
    {
        // muffin
    }
}
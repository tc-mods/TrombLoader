using BaboonAPI.Hooks.Tracks;
using UnityEngine;

namespace TrombLoader.CustomTracks.Backgrounds;

/// <summary>
///  Background used when a custom track doesn't specify one. Ends up being a grey slab.
/// </summary>
public class EmptyBackground : HijackedBackground
{
    public override void SetUpBackground(BGController controller, GameObject bg)
    {
        DisableParts(bg);
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
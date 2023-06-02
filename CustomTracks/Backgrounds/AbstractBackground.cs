using System;
using BaboonAPI.Hooks.Tracks;
using UnityEngine;

namespace TrombLoader.CustomTracks.Backgrounds;

public abstract class AbstractBackground : IDisposable, PauseAware
{
    protected AssetBundle Bundle;

    protected AbstractBackground(AssetBundle bundle)
    {
        Bundle = bundle;
    }

    public abstract GameObject Load(BackgroundContext ctx);

    public abstract void SetUpBackground(BGController controller, GameObject bg);

    public abstract void OnPause();
    public abstract void OnResume();
    public abstract bool CanResume { get; }

    public virtual void Dispose()
    {
        if (Bundle != null)
        {
            Bundle.Unload(false);
        }
    }
}
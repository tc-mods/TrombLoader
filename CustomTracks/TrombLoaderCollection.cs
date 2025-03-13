using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using BaboonAPI.Hooks.Tracks.Collections;
using BaboonAPI.Utility;
using Microsoft.FSharp.Core;
using TrombLoader.Helpers;
using UnityEngine;

namespace TrombLoader.CustomTracks;

public class TrombLoaderCollection : BaseTromboneCollection
{
    private readonly Plugin _plugin;
    public override string folder => "BepInEx/CustomSongs";

    public TrombLoaderCollection(Plugin plugin) : base(PluginInfo.PLUGIN_GUID, "TrombLoader Tracks",
        "Custom tracks loaded by TrombLoader")
    {
        _plugin = plugin;
    }

    public override IEnumerable<TromboneTrack> BuildTrackList()
    {
        return TrackLookup.allTracks().Where(track => track is CustomTrack);
    }

    public override Coroutines.YieldTask<FSharpResult<Sprite, string>> LoadSprite()
    {
        var path = Path.Combine(Path.GetDirectoryName(_plugin.Info.Location), "Assets", "collection.png");
        return BaboonAPI.Utility.Unity.loadTexture(path).Select(result =>
            ResultModule.Map(
                FuncConvert.FromFunc((Texture2D tex) =>
                    Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero)), result));
    }

    internal class CollectionLoader(Plugin plugin) : TrackCollectionRegistrationEvent.Listener
    {
        public IEnumerable<TromboneCollection> OnRegisterCollections()
        {
            yield return new TrombLoaderCollection(plugin);
        }
    }
}

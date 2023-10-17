using BaboonAPI.Hooks.Tracks;
using HarmonyLib;
using TrombLoader.CustomTracks;

namespace TrombLoader.Patch;

[HarmonyPatch]
public class BeatlessPatch
{
    [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
    [HarmonyPostfix]
    static void PatchBeatless(GameController __instance)
    {
        var track = TrackLookup.lookup(GlobalVariables.chosen_track);
        if (track is CustomTrack ct)
        {
            if (ct.beatless)
            {
                __instance.goBeatless();
            }
        }
    }
}
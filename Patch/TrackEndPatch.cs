using HarmonyLib;

namespace TrombLoader.Patch;

[HarmonyPatch(typeof(GameController))]
public class TrackEndPatch
{
    [HarmonyPatch(nameof(GameController.Start))]
    [HarmonyPostfix]
    public static void FixTrackEnd(GameController __instance)
    {
        var trackLength = __instance.musictrack.clip.length;
        if (__instance.levelendtime >= trackLength)
        {
            __instance.levelendtime = trackLength - 0.001f;
        }
    }
}

﻿using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TrombLoader.Data;

namespace TrombLoader.Patch
{
    [HarmonyPatch(typeof(GameController))]
    [HarmonyPatch("startSong")]
    public class GameControllerStartSongPatch
    {
        static void DoStartSong(GameController controller, float delay)
        {
            var bg = controller.bgcontroller.fullbgobject;
            var puppetController = bg.GetComponent<BackgroundPuppetController>();
            if (puppetController != null) puppetController.StartSong(delay);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .End() // position = last ret, insert before
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    CodeInstruction.Call(typeof(GameControllerStartSongPatch), nameof(DoStartSong))
                )
                .InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(GameController))]
    [HarmonyPatch("startDance")]
    public class GameControllerStartDancePatch
    {
        static void DoStartDance(GameController controller, float num)
        {
            var puppetController = controller.bgcontroller.fullbgobject.GetComponent<BackgroundPuppetController>();
            if (puppetController != null) puppetController.StartPuppetBob(num);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .End()
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    CodeInstruction.Call(typeof(GameControllerStartDancePatch), nameof(DoStartDance))
                )
                .InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(GameController))]
    [HarmonyPatch("Update")]
    public class GameControllerPuppetUpdatePatch
    {
        private static MethodInfo doPuppetControl_m =
            AccessTools.Method(typeof(HumanPuppetController), nameof(HumanPuppetController.doPuppetControl));

        static void DoPuppetControl(GameController controller, float vp, float vibratoAmount)
        {
            var puppetController = controller.bgcontroller.fullbgobject.GetComponent<BackgroundPuppetController>();
            if (puppetController != null) puppetController.DoPuppetControl(vp, vibratoAmount);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .SearchForward(instruction => instruction.Calls(doPuppetControl_m))
                .ThrowIfInvalid("Failed to find injection point in GameController#Update")
                .Advance(1) // Insert() inserts before, so bump 1 ahead
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_S, (byte) 14),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    CodeInstruction.LoadField(typeof(GameController), nameof(GameController.vibratoamt)),
                    CodeInstruction.Call(typeof(GameControllerPuppetUpdatePatch), nameof(DoPuppetControl))
                )
                .InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(GameController))]
    [HarmonyPatch("setPuppetBreath")]
    public class GameControllerPuppetBreathPatch
    {
        static void Postfix(GameController __instance, bool hasbreath)
        {
            var puppetController = __instance.bgcontroller.fullbgobject.GetComponent<BackgroundPuppetController>();
            if (puppetController != null) puppetController.SetPuppetBreath(hasbreath);
        }
    }

    [HarmonyPatch(typeof(GameController))]
    [HarmonyPatch("setPuppetShake")]
    public class GameControllerPuppetShakePatch
    {
        static void Postfix(GameController __instance, bool shake)
        {
            var puppetController = __instance.bgcontroller.fullbgobject.GetComponent<BackgroundPuppetController>();
            if (puppetController != null) puppetController.SetPuppetShake(shake);
        }
    }

    [HarmonyPatch(typeof(HumanPuppetController))]
    [HarmonyPatch("startPuppetBob")]
    public class HumanPuppetControllerPuppetBobPatch
    {
        static bool Prefix(HumanPuppetController __instance)
        {
            return !__instance.just_testing;
        }
    }

    /// <summary>
    ///  Disable debugging routine
    /// </summary>
    [HarmonyPatch(typeof(HumanPuppetController))]
    [HarmonyPatch("testMovement")]
    public class HumanPuppetControllerTestMovementPatch
    {
        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(HumanPuppetController))]
    [HarmonyPatch("Start")]
    public class HumanPuppetControllerStartPatch
    {
        static bool Prefix(HumanPuppetController __instance)
        {
            // just disable for custom tromboners
            var isCustom = __instance.gameObject.GetComponent<CustomPuppetController>() != null;

            return !isCustom;
        }

        static void Postfix(HumanPuppetController __instance, bool __runOriginal)
        {
            if (!__runOriginal)
            {
                // apply the texture stuff for custom tromboners
                __instance.Invoke(nameof(HumanPuppetController.setTextures), 0.5f);
                __instance.applyFaceTex();
            }
        }
    }
}

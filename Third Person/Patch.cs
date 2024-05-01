using HarmonyLib;
using PulsarModLoader.Patches;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine.Rendering;
using UnityEngine;

namespace Third_Person
{
    [HarmonyPatch(typeof(PLTurret), "DeActivate")]
    class Turret
    {
        static void Postfix()
        {
            if (ThirdPerson.activated) PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_ThirdPerson());
        }
    }
    [HarmonyPatch(typeof(PLPawn), "Revive")]
    class Revive
    {
        static void Postfix()
        {
            if (ThirdPerson.activated) PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_ThirdPerson());
        }
    }
    [HarmonyPatch(typeof(PLPilotingSystem), "DeActivate")]
    class Pilot
    {
        static void Postfix()
        {
            if (ThirdPerson.activated) PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_ThirdPerson());
        }
    }

    [HarmonyPatch(typeof(PLInGameUI), "Update")]
    class Keybind
    {
        static void Postfix()
        {
            if (PulsarModLoader.Keybinds.KeybindManager.Instance.GetButtonDown("ThirdPersonButton"))
            {
                if (PLCameraSystem.Instance.CurrentCameraMode != null && PLCameraSystem.Instance.CurrentCameraMode.GetType() == typeof(PLCameraMode_ThirdPerson))
                {
                    PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_LocalPawn());
                    ThirdPerson.activated = false;
                }
                else
                {
                    PLCameraSystem.Instance.ChangeCameraMode(new PLCameraMode_ThirdPerson());
                    ThirdPerson.activated = true;
                }
            }
        }
    }


    [HarmonyPatch(typeof(PLExosuit), "Update")]
    class Exosuit
    {

        static void Postfix(PLExosuit __instance) 
        {
            bool flag = !ThirdPerson.activated && __instance.MyPawn == PLNetworkManager.Instance.ViewedPawn && !PLNetworkManager.Instance.ViewedPawn.ExteriorViewActive();
            if (__instance.MyPawn.GetExosuitIsActive() && __instance.InteriorHelmetTransform != null)
            {
                if (__instance.ExteriorHelmet != null)
                {
                    if (!flag)
                    {
                        __instance.ExteriorHelmet.shadowCastingMode = ShadowCastingMode.On;
                    }
                    else
                    {
                        __instance.ExteriorHelmet.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                    }
                }
            }
            if (__instance.HelmetUI != null && __instance.HelmetUI.gameObject.activeSelf != flag)
            {
                __instance.HelmetUI.gameObject.SetActive(flag);
            }
            if (__instance.InteriorHelmet != null)
            {
                bool flag2 = flag && __instance.MyPawn.GetExosuitIsActive();
                if (__instance.InteriorHelmet.gameObject.activeSelf != flag2)
                {
                    __instance.InteriorHelmet.gameObject.SetActive(flag2);
                }
            }
            if (__instance.MyPawn.MySkinnedMeshRenderer != null)
            {
                if (__instance.ExteriorHelmet != null)
                {
                    foreach (Light light in __instance.ExteriorHelmet_Lights)
                    {
                        if (light != null)
                        {
                            light.enabled = !flag;
                        }
                    }
                }
            }
        }




        /*
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var code = new List<CodeInstruction>(instructions);


            Label lab1 = il.DefineLabel();
            Label lab2 = il.DefineLabel();

            code[235].labels.Add(lab1);

            //End Code
            List<CodeInstruction> targetSequence = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PLPawn),"ExteriorViewActive")),
                new CodeInstruction(OpCodes.Ldc_I4_0, null),
                new CodeInstruction(OpCodes.Ceq, null),
                new CodeInstruction(OpCodes.Br_S,null),
                new CodeInstruction(OpCodes.Ldc_I4_0 , null),
            };
            List<CodeInstruction> patchSequence = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Br_S, lab1),
                new CodeInstruction(OpCodes.Ldc_I4_0 , null),

            };

            IEnumerable<CodeInstruction> newCode = HarmonyHelpers.PatchBySequence(code, targetSequence, patchSequence, HarmonyHelpers.PatchMode.AFTER, HarmonyHelpers.CheckMode.NONNULL, false);

            code = new List<CodeInstruction>(newCode);

            code[236].labels.Add(lab2);

            targetSequence = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0, null),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLExosuit),"MyPawn")),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PLNetworkManager),"Instance")),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PLNetworkManager),"ViewedPawn")),
            };
            patchSequence = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(ThirdPerson),"activated")),
                new CodeInstruction(OpCodes.Brtrue_S, lab2),

            };
            return HarmonyHelpers.PatchBySequence(code, targetSequence, patchSequence, HarmonyHelpers.PatchMode.BEFORE, HarmonyHelpers.CheckMode.NONNULL, false);
        }
        */
    }
}

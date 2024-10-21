using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using MoreCompany.Cosmetics;
using UnityEngine;

namespace Unobscured.Patches
{
    public class HideCosmeticsPatch
    {
        private static bool isFirstPerson = false;
        private const float FIRST_PERSON_DISTANCE_THRESHOLD = 0.5f;
/*        private static List<CosmeticInstance> cosmetics = new();

        [HarmonyPostfix, HarmonyPatch(typeof(CosmeticApplication), nameof(CosmeticApplication.OnEnable))]
        private static void OnEnablePatch(CosmeticApplication __instance)
        {
            StartOfRound.Instance.CameraSwitchEvent.AddListener(() =>
            {
                cosmetics = __instance.spawnedCosmetics;
            });
        }*/

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.LateUpdate))]
        private static void LateUpdatePatch(PlayerControllerB __instance)
        {
            if (!__instance.hasBegunSpectating) return;
            if (__instance.spectateCameraPivot == null || __instance.spectatedPlayerScript == null) return;

            float distance = Vector3.Distance(
                StartOfRound.Instance.spectateCamera.transform.position,
                __instance.spectatedPlayerScript.visorCamera.transform.position
            );

            Transform model = __instance.spectatedPlayerScript.transform.Find("ScavengerModel");
            if (model == null)
            {
                Unobscured.Logger.LogError("Model is null");
                return;
            }

            Transform metarig = model.Find("metarig");
            if (metarig == null)
            {
                Unobscured.Logger.LogError("Metarig is null");
                return;
            }

            var cosmeticApplication = metarig.GetComponent<CosmeticApplication>();
            if (cosmeticApplication == null)
            {
                Unobscured.Logger.LogError("CosmeticApplication is null");
                return;
            }
            var cosmetics = cosmeticApplication.spawnedCosmetics;

            foreach (var cosmetic in cosmetics)
            {
                cosmetic.gameObject.SetActive(distance > FIRST_PERSON_DISTANCE_THRESHOLD);
            }
        }
    }
}

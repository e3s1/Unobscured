using System.Collections;
using GameNetcodeStuff;
using HarmonyLib;
using MoreCompany.Cosmetics;
using UnityEngine;

namespace Unobscured.Patches
{
    public class HideCosmeticsPatch
    {
        private static PlayerControllerB? lastSpectatedPlayer;
        private const float FIRST_PERSON_DISTANCE_THRESHOLD = 0.5f;

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SpectateNextPlayer))]
        private static void SpectateNextPatch(PlayerControllerB __instance)
        {
            if (lastSpectatedPlayer != null)
            {
                var lastCosmeticApplication = getCosmeticApplicationFromPlayer(lastSpectatedPlayer);
                if (lastCosmeticApplication == null) return;

                Unobscured.Logger.LogDebug("Showing last player's cosmetics");

                foreach (var cosmetic in lastCosmeticApplication.spawnedCosmetics)
                {
                    cosmetic.gameObject.SetActive(true);
                }

            }

            lastSpectatedPlayer = __instance.spectatedPlayerScript;

        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.LateUpdate))]
        private static void LateUpdatePatch(PlayerControllerB __instance)
        {
            if (!__instance.hasBegunSpectating || __instance.spectateCameraPivot == null || __instance.spectatedPlayerScript == null) return;

            float distance = Vector3.Distance(
                StartOfRound.Instance.spectateCamera.transform.position,
                __instance.spectatedPlayerScript.visorCamera.transform.position
            );

            var cosmeticApplication = getCosmeticApplicationFromPlayer(__instance.spectatedPlayerScript);
            if (cosmeticApplication == null) return;

            var cosmetics = cosmeticApplication.spawnedCosmetics;

            foreach (var cosmetic in cosmetics)
            {
                cosmetic.gameObject.SetActive(distance > FIRST_PERSON_DISTANCE_THRESHOLD);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ReviveDeadPlayers))]
        private static void RevivePlayersPatch()
        {
            Unobscured.Logger.LogDebug("Force showing cosmetics");

            if (lastSpectatedPlayer == null)
            {
                Unobscured.Logger.LogDebug("Last spectated player is null, not doing anything");
                return;
            }

            var cosmeticApplication = getCosmeticApplicationFromPlayer(lastSpectatedPlayer);
            if (cosmeticApplication == null)
            {
                Unobscured.Logger.LogDebug("CosmeticApplication is null");
                return;
            }

            foreach (var cosmetic in cosmeticApplication.spawnedCosmetics)
            {
                cosmetic.gameObject.SetActive(true);
            }

            lastSpectatedPlayer = null;

        }

        private static CosmeticApplication? getCosmeticApplicationFromPlayer(PlayerControllerB player) {
            Transform model = player.transform.Find("ScavengerModel");
            if (model == null) return null;

            Transform metarig = model.Find("metarig");
            if (metarig == null) return null;

            return metarig.GetComponent<CosmeticApplication>();
        }
    }
}

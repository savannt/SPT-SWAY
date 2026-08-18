using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using SptSway.Runtime;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SptSway.Patches
{
    internal static class LocalPlayer
    {
        /// <summary>
        /// The player these animations belong to, or null if they belong to
        /// someone else. Bots run the same animator, and driving their sway
        /// from our model would be both wrong and expensive.
        /// </summary>
        public static Player Resolve(ProceduralWeaponAnimation pwa)
        {
            if (pwa == null) return null;
            if (pwa.PointOfView != EPointOfView.FirstPerson) return null;

            var world = Singleton<GameWorld>.Instance;
            var player = world != null ? world.MainPlayer : null;
            if (player == null) return null;

            return player.ProceduralWeaponAnimation == pwa ? player : null;
        }
    }

    /// <summary>
    /// The per-frame entry point. LerpCamera runs every rendered frame with the
    /// frame's delta and after BSG's own effectors have had their say, which
    /// makes it the right place to add ours on top.
    /// </summary>
    public class SwayTickPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.LerpCamera));
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance, float dt)
        {
            var player = LocalPlayer.Resolve(__instance);
            if (player == null) return;

            SwayDirector.Instance.Tick(player, __instance, dt);
        }
    }

    /// <summary>
    /// Arm fractures, blacked limbs and the tremor condition arrive here as a
    /// bitmask. Nothing else on the player exposes it in one piece.
    /// </summary>
    public class PhysicalConditionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation),
                nameof(ProceduralWeaponAnimation.PhysicalConditionUpdated));
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance, EPhysicalCondition full)
        {
            if (LocalPlayer.Resolve(__instance) == null) return;
            PhysicalConditionTracker.Current = full;
        }
    }

    /// <summary>
    /// A new weapon means new BSG defaults to capture and a stale inertia state
    /// to throw away.
    /// </summary>
    public class WeaponChangedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation),
                nameof(ProceduralWeaponAnimation.InitWeaponData));
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            if (LocalPlayer.Resolve(__instance) == null) return;
            SwayDirector.Instance.OnWeaponChanged();
        }
    }

    /// <summary>Firing disturbs the hold and raises the pulse.</summary>
    public class ShotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnMakingShot));
        }

        [PatchPostfix]
        private static void Postfix(Player __instance)
        {
            var world = Singleton<GameWorld>.Instance;
            if (world == null || world.MainPlayer != __instance) return;

            SwayDirector.Instance.OnShot();
        }
    }
}

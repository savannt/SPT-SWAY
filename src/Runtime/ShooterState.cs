using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using UnityEngine;

namespace SptSway.Runtime
{
    /// <summary>
    /// Everything the model needs to know about the shooter this frame, read
    /// once and cached. Each field is either something a real body would change
    /// its hold over, or something the config maps onto one of those.
    ///
    /// Every read is defensive. A field that vanishes in a future EFT build
    /// should cost that one signal, not the whole mod.
    /// </summary>
    public sealed class ShooterState
    {
        // -- raw inputs ---------------------------------------------------
        public bool  IsAiming;
        public bool  IsProne;
        public bool  IsMounted;
        public bool  IsBipod;
        public bool  IsSprinting;
        public bool  HoldingBreath;
        public float PoseLevel;          // 0 fully crouched .. 1 standing
        public float Lean;               // -1 .. 1
        public float MoveSpeed;          // m/s, smoothed by the game

        // -- physiology ---------------------------------------------------
        public float Stamina    = 1f;    // 1 fresh .. 0 spent
        public float Oxygen     = 1f;
        public float ArmStamina = 1f;
        public bool  Exhausted;

        // -- injury -------------------------------------------------------
        public bool ArmFractured;
        public bool ArmBlacked;
        public bool NeuroTremor;         // the Tremor physical condition, from stims and shock

        // -- weapon -------------------------------------------------------
        public float Ergonomics = 50f;
        public float WeightKg   = 3.5f;
        public float RecoilStat = 100f;

        // -- derived ------------------------------------------------------
        /// <summary>0 fresh .. 1 wrecked. The single number most sources scale by.</summary>
        public float Exertion;

        /// <summary>Ergonomics and weight folded into one multiplier.</summary>
        public float HandlingFactor = 1f;

        /// <summary>How much the weapon is being held up rather than resting on something. 1 unsupported .. 0 bipod.</summary>
        public float SupportFactor = 1f;

        public void Sample(Player player, ProceduralWeaponAnimation pwa, float dt)
        {
            IsAiming = pwa.IsAiming;

            var mc = player.MovementContext;
            if (mc != null)
            {
                IsProne     = mc.IsInPronePose;
                IsMounted   = mc.IsInMountedState || mc.InMountState;
                IsSprinting = mc.IsSprintEnabled;
                PoseLevel   = Mathf.Clamp01(mc.PoseLevel);
                Lean        = Mathf.Clamp(mc.Tilt, -1f, 1f);
                MoveSpeed   = Mathf.Max(0f, mc.SmoothedCharacterMovementSpeed);
            }

            IsBipod = pwa.IsBipodUsed;

            SamplePhysical(player);
            SampleWeapon(player);
            SampleInjury(player);

            // Exertion blends the three drains a body actually has. Stamina is
            // the legs, oxygen is the lungs, arm stamina is what holding a
            // rifle up costs you specifically.
            float stam = 1f - Stamina;
            float oxy  = 1f - Oxygen;
            float arms = 1f - ArmStamina;

            float wStam = SwayConfig.StaminaInfluence.Value;
            float wOxy  = SwayConfig.OxygenInfluence.Value;
            float wArms = SwayConfig.HandsStaminaInfluence.Value;
            float wSum  = wStam + wOxy + wArms;

            Exertion = wSum > 0.0001f
                ? Mathf.Clamp01((stam * wStam + oxy * wOxy + arms * wArms) / wSum)
                : 0f;

            if (!SwayConfig.FatigueEnabled.Value)
                Exertion = 0f;

            HandlingFactor = ComputeHandling();
            SupportFactor  = ComputeSupport(dt);
        }

        private void SamplePhysical(Player player)
        {
            var phys = player.Physical;
            if (phys == null) return;

            Stamina    = Normalised(phys.Stamina);
            Oxygen     = Normalised(phys.Oxygen);
            ArmStamina = Normalised(phys.HandsStamina);

            try { Exhausted = phys.Exhausted; } catch { Exhausted = Stamina <= 0.02f; }
            try { HoldingBreath = phys.HoldingBreath; } catch { HoldingBreath = false; }
        }

        private static float Normalised(Stamina s)
        {
            if (s == null) return 1f;
            try
            {
                // NormalValue is already 0..1 when the game feels like providing
                // it; fall back to the raw current over capacity when it does not.
                float n = s.NormalValue;
                if (n > 0f || s.Current <= 0f) return Mathf.Clamp01(n);
            }
            catch { /* fall through */ }

            try
            {
                float cap = s.TotalCapacity != null ? s.TotalCapacity.Value : 0f;
                return cap > 0.0001f ? Mathf.Clamp01(s.Current / cap) : 1f;
            }
            catch { return 1f; }
        }

        private void SampleWeapon(Player player)
        {
            var fc = player.HandsController as Player.FirearmController;
            if (fc == null) return;

            Weapon weapon = null;
            try { weapon = fc.Weapon; } catch { /* not a firearm this frame */ }
            if (weapon == null) return;

            try { Ergonomics = weapon.ErgonomicsTotal; } catch { }
            // TotalWeight includes mods and a loaded magazine, which is what the
            // shooter is actually holding up.
            try { WeightKg = weapon.TotalWeight; } catch { try { WeightKg = weapon.Weight; } catch { } }
            try { RecoilStat = weapon.RecoilTotal; } catch { }
        }

        private void SampleInjury(Player player)
        {
            ArmFractured = false;
            ArmBlacked   = false;
            NeuroTremor  = false;

            if (!SwayConfig.InjuryEnabled.Value) return;

            var cond = PhysicalConditionTracker.Current;
            bool leftDamaged  = (cond & EPhysicalCondition.LeftArmDamaged)  != 0;
            bool rightDamaged = (cond & EPhysicalCondition.RightArmDamaged) != 0;
            NeuroTremor       = (cond & EPhysicalCondition.Tremor)          != 0;

            var hc = player.HealthController;
            if (hc != null)
            {
                try
                {
                    ArmBlacked = hc.IsBodyPartDestroyed(EBodyPart.LeftArm)
                              || hc.IsBodyPartDestroyed(EBodyPart.RightArm);
                }
                catch { }
            }

            // The game flags an arm as damaged for both a fracture and a blacked
            // limb. If it is damaged but not destroyed, the cause is a fracture.
            ArmFractured = (leftDamaged || rightDamaged) && !ArmBlacked;
        }

        private float ComputeHandling()
        {
            // Ergonomics maps onto a bounded curve rather than a bare divide, so
            // a 90-ergo pistol cannot drive sway to zero and a 20-ergo machine gun
            // cannot drive it to infinity.
            float half  = Mathf.Max(1f, SwayConfig.ErgoHalfPoint.Value);
            float curve = SwayConfig.ErgoCurve.Value;
            float floor = SwayConfig.ErgoFloor.Value;
            float ceil  = Mathf.Max(floor + 0.01f, SwayConfig.ErgoCeiling.Value);

            float e = Mathf.Max(0f, Ergonomics);
            float t = Mathf.Pow(half, curve) / (Mathf.Pow(half, curve) + Mathf.Pow(e, curve));
            float ergo = Mathf.Lerp(floor, ceil, t);

            // Weight on top: a heavy weapon is harder to hold still even when it
            // is ergonomically pleasant.
            float refW = Mathf.Max(0.1f, SwayConfig.WeightReference.Value);
            float wDelta = (WeightKg - refW) / refW;
            float weight = 1f + wDelta * SwayConfig.WeightInfluence.Value * 0.4f;

            float recoil = 1f + (RecoilStat / 100f - 1f) * SwayConfig.RecoilStatInfluence.Value * 0.25f;

            return Mathf.Clamp(ergo * weight * recoil, 0.05f, 8f);
        }

        private float _supportSmoothed = 1f;

        private float ComputeSupport(float dt)
        {
            float target;

            if (IsBipod)        target = SwayConfig.BipodMultiplier.Value;
            else if (IsMounted) target = SwayConfig.MountedMultiplier.Value;
            else if (IsProne)   target = SwayConfig.ProneMultiplier.Value;
            else
            {
                // Between standing and crouched, blend by how low the player
                // actually is rather than snapping at the pose change.
                float influence = Mathf.Clamp01(SwayConfig.PoseLevelInfluence.Value);
                float posture   = Mathf.Lerp(1f, PoseLevel, influence);
                target = Mathf.Lerp(SwayConfig.CrouchMultiplier.Value,
                                    SwayConfig.StandMultiplier.Value,
                                    posture);
            }

            // Leaning is unsupported no matter what else is true.
            target *= Mathf.Lerp(1f, SwayConfig.LeanMultiplier.Value, Mathf.Abs(Lean));

            if (IsSprinting)
                target *= SwayConfig.SprintMultiplier.Value;

            float blend = Mathf.Clamp01(dt * SwayConfig.SupportBlendSpeed.Value);
            _supportSmoothed = Mathf.Lerp(_supportSmoothed, target, blend);
            return _supportSmoothed;
        }
    }
}

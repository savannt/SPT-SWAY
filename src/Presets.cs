namespace SptSway
{
    public enum SwayPreset
    {
        /// <summary>Everything this mod adds is switched off. Stock EFT.</summary>
        Vanilla,

        /// <summary>Readable and forgiving. Sway you notice but never fight.</summary>
        Arcade,

        /// <summary>The tuned default. Numbers taken from how a body actually behaves.</summary>
        Realistic,

        /// <summary>Realistic with the leash off. Fatigue and injury bite hard.</summary>
        Hardcore,

        /// <summary>Oversized and slow. Built to look good on video, not to shoot well.</summary>
        Cinematic,

        /// <summary>Slow sources kept, fast ones trimmed. For long-range shooting.</summary>
        Marksman,
    }

    /// <summary>
    /// Presets write straight over every config entry. Anything not named here
    /// keeps whatever the user last set, so a preset is a starting point rather
    /// than a reset.
    /// </summary>
    public static class Presets
    {
        public static void Apply(SwayPreset preset)
        {
            // Start from the tuned baseline, then diverge.
            Realistic();

            switch (preset)
            {
                case SwayPreset.Vanilla:   Vanilla();   break;
                case SwayPreset.Arcade:    Arcade();    break;
                case SwayPreset.Hardcore:  Hardcore();  break;
                case SwayPreset.Cinematic: Cinematic(); break;
                case SwayPreset.Marksman:  Marksman();  break;
            }
        }

        private static void Realistic()
        {
            SwayConfig.MasterIntensity.Value = 1f;
            SwayConfig.GlobalTimeScale.Value = 1f;
            SwayConfig.AdsMaster.Value = 1f;
            SwayConfig.HipMaster.Value = 1f;
            SwayConfig.KeepVanillaSway.Value = true;

            SwayConfig.RespEnabled.Value = true;
            SwayConfig.RespAmplitude.Value = 1f;
            SwayConfig.RespRateRest.Value = 14f;
            SwayConfig.RespWaveformSharpness.Value = 1f;
            SwayConfig.RespIrregularity.Value = 0.15f;

            SwayConfig.HeartEnabled.Value = true;
            SwayConfig.HeartAmplitude.Value = 1f;
            SwayConfig.HeartSystolicSharpness.Value = 7f;
            SwayConfig.HeartDicroticStrength.Value = 0.35f;

            SwayConfig.TremorEnabled.Value = true;
            SwayConfig.TremorAmplitude.Value = 1f;
            SwayConfig.TremorFrequency.Value = 9.5f;

            SwayConfig.DriftEnabled.Value = true;
            SwayConfig.DriftAmplitude.Value = 1f;

            SwayConfig.InertiaEnabled.Value = true;
            SwayConfig.InertiaStrength.Value = 1f;
            SwayConfig.InertiaDamping.Value = 0.62f;
            SwayConfig.InertiaFrequency.Value = 2.6f;

            SwayConfig.FatigueEnabled.Value = true;
            SwayConfig.InjuryEnabled.Value = true;
            SwayConfig.ShotEnabled.Value = true;
            SwayConfig.CameraCoupling.Value = 0.35f;
        }

        private static void Vanilla()
        {
            SwayConfig.RespEnabled.Value = false;
            SwayConfig.HeartEnabled.Value = false;
            SwayConfig.TremorEnabled.Value = false;
            SwayConfig.DriftEnabled.Value = false;
            SwayConfig.InertiaEnabled.Value = false;
            SwayConfig.ShotEnabled.Value = false;
            SwayConfig.SpringOverride.Value = false;
            SwayConfig.VanillaOverride.Value = false;
            SwayConfig.CameraCoupling.Value = 0f;
        }

        private static void Arcade()
        {
            SwayConfig.MasterIntensity.Value = 0.45f;
            SwayConfig.RespAmplitude.Value = 0.6f;
            SwayConfig.HeartAmplitude.Value = 0.35f;
            SwayConfig.TremorAmplitude.Value = 0.4f;
            SwayConfig.DriftAmplitude.Value = 0.5f;
            SwayConfig.InertiaStrength.Value = 0.55f;
            SwayConfig.InertiaDamping.Value = 0.95f;   // settles fast, barely overshoots
            SwayConfig.TremorFatigueGain.Value = 0.8f;
            SwayConfig.StaminaInfluence.Value = 0.5f;
            SwayConfig.OxygenInfluence.Value = 0.4f;
            SwayConfig.ExhaustedMultiplier.Value = 1.25f;
            SwayConfig.ArmFractureMultiplier.Value = 1.4f;
            SwayConfig.ArmBlackedMultiplier.Value = 1.2f;
            SwayConfig.CameraCoupling.Value = 0.15f;
            SwayConfig.ShotGain.Value = 0.05f;
        }

        private static void Hardcore()
        {
            SwayConfig.MasterIntensity.Value = 1.45f;
            SwayConfig.RespAmplitude.Value = 1.3f;
            SwayConfig.RespRateExhausted.Value = 38f;
            SwayConfig.HeartAmplitude.Value = 1.5f;
            SwayConfig.HeartExertionGain.Value = 2.4f;
            SwayConfig.TremorAmplitude.Value = 1.4f;
            SwayConfig.TremorFatigueGain.Value = 3.4f;
            SwayConfig.DriftAmplitude.Value = 1.25f;
            SwayConfig.InertiaStrength.Value = 1.5f;
            SwayConfig.InertiaDamping.Value = 0.45f;   // swings further, takes longer to settle
            SwayConfig.StaminaInfluence.Value = 2f;
            SwayConfig.OxygenInfluence.Value = 1.6f;
            SwayConfig.ExhaustedMultiplier.Value = 2.6f;
            SwayConfig.HoldFatigueRate.Value = 0.16f;
            SwayConfig.ArmFractureMultiplier.Value = 3.2f;
            SwayConfig.ArmBlackedMultiplier.Value = 2.2f;
            SwayConfig.ShotGain.Value = 0.2f;
            SwayConfig.CameraCoupling.Value = 0.5f;
        }

        private static void Cinematic()
        {
            SwayConfig.MasterIntensity.Value = 1.9f;
            SwayConfig.GlobalTimeScale.Value = 0.75f;   // everything slower and more legible
            SwayConfig.RespAmplitude.Value = 2.1f;
            SwayConfig.RespWaveformSharpness.Value = 0.4f;
            SwayConfig.HeartAmplitude.Value = 1.8f;
            SwayConfig.TremorAmplitude.Value = 0.6f;    // fine shake reads as noise on camera
            SwayConfig.DriftAmplitude.Value = 2f;
            SwayConfig.InertiaStrength.Value = 2.2f;
            SwayConfig.InertiaFrequency.Value = 1.6f;
            SwayConfig.InertiaDamping.Value = 0.4f;
            SwayConfig.InertiaMaxDeflection.Value = 9f;
            SwayConfig.CameraCoupling.Value = 0.85f;
        }

        private static void Marksman()
        {
            // Slow sources stay, because they are what you time a shot around.
            // Fast sources come down, because at magnification they are just noise.
            SwayConfig.RespAmplitude.Value = 1.1f;
            SwayConfig.RespPauseFraction.Value = 0.28f;   // longer natural firing window
            SwayConfig.RespHoldSuppression.Value = 0.05f; // holding breath really works
            SwayConfig.HeartAmplitude.Value = 1.3f;
            SwayConfig.HeartAds.Value = 1.5f;
            SwayConfig.TremorAmplitude.Value = 0.75f;
            SwayConfig.TremorAds.Value = 1.1f;
            SwayConfig.DriftAmplitude.Value = 0.9f;
            SwayConfig.InertiaAds.Value = 0.35f;
            SwayConfig.MountedMultiplier.Value = 0.15f;
            SwayConfig.BipodMultiplier.Value = 0.08f;
            SwayConfig.ProneMultiplier.Value = 0.3f;
            SwayConfig.CameraCoupling.Value = 0.25f;
        }
    }
}

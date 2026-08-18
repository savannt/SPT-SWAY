using BepInEx.Configuration;
using UnityEngine;

namespace SptSway
{
    /// <summary>
    /// Every number the sway model uses, exposed as a BepInEx config entry.
    /// Sections are numbered so the F12 menu and the .cfg file keep the same
    /// order the signal chain runs in: sources first, then the modifiers that
    /// scale them, then the raw BSG values we override on top.
    /// </summary>
    public static class SwayConfig
    {
        // -- section names ------------------------------------------------
        private const string General  = "00 - General";
        private const string Resp     = "01 - Respiration";
        private const string Heart    = "02 - Cardiac";
        private const string Tremor   = "03 - Physiological Tremor";
        private const string Postural = "04 - Postural Drift";
        private const string Inertia  = "05 - Weapon Inertia";
        private const string Weapon   = "06 - Weapon Handling";
        private const string Stance   = "07 - Stance & Support";
        private const string Fatigue  = "08 - Stamina & Fatigue";
        private const string Injury   = "09 - Injury";
        private const string Recoil   = "10 - Shot Disturbance";
        private const string Coupling = "11 - Camera Coupling";
        private const string Springs  = "12 - Springs (advanced)";
        private const string Vanilla  = "13 - Vanilla Effectors (advanced)";
        private const string Debug    = "99 - Debug";

        // ================= 00 General =====================================
        public static ConfigEntry<bool>             Enabled;
        public static ConfigEntry<KeyboardShortcut> ToggleKey;
        public static ConfigEntry<SwayPreset>       Preset;
        public static ConfigEntry<bool>             ApplyPreset;
        public static ConfigEntry<float>            MasterIntensity;
        public static ConfigEntry<float>            AdsMaster;
        public static ConfigEntry<float>            HipMaster;
        public static ConfigEntry<float>            GlobalTimeScale;
        public static ConfigEntry<bool>             KeepVanillaSway;

        // ================= 01 Respiration =================================
        public static ConfigEntry<bool>  RespEnabled;
        public static ConfigEntry<float> RespAmplitude;
        public static ConfigEntry<float> RespRateRest;
        public static ConfigEntry<float> RespRateExhausted;
        public static ConfigEntry<float> RespInhaleFraction;
        public static ConfigEntry<float> RespExhaleFraction;
        public static ConfigEntry<float> RespPauseFraction;
        public static ConfigEntry<float> RespWaveformSharpness;
        public static ConfigEntry<float> RespPitch;
        public static ConfigEntry<float> RespYaw;
        public static ConfigEntry<float> RespRoll;
        public static ConfigEntry<float> RespAds;
        public static ConfigEntry<float> RespHip;
        public static ConfigEntry<float> RespHoldSuppression;
        public static ConfigEntry<float> RespHoldReboundGain;
        public static ConfigEntry<float> RespHoldReboundDecay;
        public static ConfigEntry<float> RespOxygenInfluence;
        public static ConfigEntry<float> RespIrregularity;

        // ================= 02 Cardiac =====================================
        public static ConfigEntry<bool>  HeartEnabled;
        public static ConfigEntry<float> HeartAmplitude;
        public static ConfigEntry<float> HeartRateRest;
        public static ConfigEntry<float> HeartRateMax;
        public static ConfigEntry<float> HeartRateRise;
        public static ConfigEntry<float> HeartRateFall;
        public static ConfigEntry<float> HeartSystolicSharpness;
        public static ConfigEntry<float> HeartDicroticStrength;
        public static ConfigEntry<float> HeartDicroticDelay;
        public static ConfigEntry<float> HeartPitch;
        public static ConfigEntry<float> HeartYaw;
        public static ConfigEntry<float> HeartRoll;
        public static ConfigEntry<float> HeartAds;
        public static ConfigEntry<float> HeartHip;
        public static ConfigEntry<float> HeartExertionGain;

        // ================= 03 Tremor ======================================
        public static ConfigEntry<bool>  TremorEnabled;
        public static ConfigEntry<float> TremorAmplitude;
        public static ConfigEntry<float> TremorFrequency;
        public static ConfigEntry<float> TremorFrequencySpread;
        public static ConfigEntry<int>   TremorOctaves;
        public static ConfigEntry<float> TremorFatigueGain;
        public static ConfigEntry<float> TremorLoadGain;
        public static ConfigEntry<float> TremorPitch;
        public static ConfigEntry<float> TremorYaw;
        public static ConfigEntry<float> TremorRoll;
        public static ConfigEntry<float> TremorAds;
        public static ConfigEntry<float> TremorHip;

        // ================= 04 Postural drift ==============================
        public static ConfigEntry<bool>  DriftEnabled;
        public static ConfigEntry<float> DriftAmplitude;
        public static ConfigEntry<float> DriftFrequency;
        public static ConfigEntry<float> DriftPitch;
        public static ConfigEntry<float> DriftYaw;
        public static ConfigEntry<float> DriftRoll;
        public static ConfigEntry<float> DriftAds;
        public static ConfigEntry<float> DriftHip;
        public static ConfigEntry<float> DriftMoveGain;

        // ================= 05 Inertia =====================================
        public static ConfigEntry<bool>  InertiaEnabled;
        public static ConfigEntry<float> InertiaStrength;
        public static ConfigEntry<float> InertiaFrequency;
        public static ConfigEntry<float> InertiaDamping;
        public static ConfigEntry<float> InertiaMassCoupling;
        public static ConfigEntry<float> InertiaRollCoupling;
        public static ConfigEntry<float> InertiaMaxDeflection;
        public static ConfigEntry<float> InertiaAds;
        public static ConfigEntry<float> InertiaHip;
        public static ConfigEntry<float> InertiaMoveGain;
        public static ConfigEntry<float> InertiaTurnClamp;

        // ================= 06 Weapon handling =============================
        public static ConfigEntry<float> ErgoHalfPoint;
        public static ConfigEntry<float> ErgoCurve;
        public static ConfigEntry<float> ErgoFloor;
        public static ConfigEntry<float> ErgoCeiling;
        public static ConfigEntry<float> WeightInfluence;
        public static ConfigEntry<float> WeightReference;
        public static ConfigEntry<float> RecoilStatInfluence;

        // ================= 07 Stance & support ============================
        public static ConfigEntry<float> StandMultiplier;
        public static ConfigEntry<float> CrouchMultiplier;
        public static ConfigEntry<float> ProneMultiplier;
        public static ConfigEntry<float> MountedMultiplier;
        public static ConfigEntry<float> BipodMultiplier;
        public static ConfigEntry<float> LeanMultiplier;
        public static ConfigEntry<float> SprintMultiplier;
        public static ConfigEntry<float> PoseLevelInfluence;
        public static ConfigEntry<float> SupportBlendSpeed;

        // ================= 08 Stamina & fatigue ===========================
        public static ConfigEntry<bool>  FatigueEnabled;
        public static ConfigEntry<float> StaminaInfluence;
        public static ConfigEntry<float> OxygenInfluence;
        public static ConfigEntry<float> HandsStaminaInfluence;
        public static ConfigEntry<float> ExhaustedMultiplier;
        public static ConfigEntry<float> HoldFatigueRate;
        public static ConfigEntry<float> HoldFatigueDecay;
        public static ConfigEntry<float> HoldFatigueMax;

        // ================= 09 Injury ======================================
        public static ConfigEntry<bool>  InjuryEnabled;
        public static ConfigEntry<float> ArmFractureMultiplier;
        public static ConfigEntry<float> ArmBlackedMultiplier;
        public static ConfigEntry<float> TremorInjuryGain;

        // ================= 10 Shot disturbance ============================
        public static ConfigEntry<bool>  ShotEnabled;
        public static ConfigEntry<float> ShotGain;
        public static ConfigEntry<float> ShotDecay;
        public static ConfigEntry<float> ShotMaxAccumulation;
        public static ConfigEntry<float> ShotHeartRateKick;

        // ================= 11 Camera coupling =============================
        public static ConfigEntry<float> CameraCoupling;
        public static ConfigEntry<float> CameraRespShare;
        public static ConfigEntry<float> CameraHeartShare;
        public static ConfigEntry<float> CameraTremorShare;
        public static ConfigEntry<float> CameraDriftShare;
        public static ConfigEntry<float> CameraInertiaShare;

        // ================= 12 Springs =====================================
        public static ConfigEntry<float> DriveGain;
        public static ConfigEntry<bool>  SpringOverride;
        public static ConfigEntry<float> HandsRotDamping;
        public static ConfigEntry<float> HandsRotReturnSpeed;
        public static ConfigEntry<float> HandsRotAccelerationMax;
        public static ConfigEntry<float> HandsRotInputIntensity;
        public static ConfigEntry<float> HandsRotSoftness;
        public static ConfigEntry<float> HandsPosDamping;
        public static ConfigEntry<float> HandsPosReturnSpeed;
        public static ConfigEntry<float> CameraRotDamping;
        public static ConfigEntry<float> CameraRotReturnSpeed;

        // ================= 13 Vanilla effectors ===========================
        public static ConfigEntry<bool>  VanillaOverride;
        public static ConfigEntry<float> VanillaBreathIntensity;
        public static ConfigEntry<float> VanillaBreathFrequency;
        public static ConfigEntry<float> VanillaBreathShake;
        public static ConfigEntry<float> VanillaBreathHipPenalty;
        public static ConfigEntry<float> VanillaBreathCameraSens;
        public static ConfigEntry<bool>  VanillaBreathTremorOn;
        public static ConfigEntry<float> VanillaBreathTremorAmplitude;
        public static ConfigEntry<float> VanillaBreathTremorHardness;
        public static ConfigEntry<float> VanillaMotionIntensity;
        public static ConfigEntry<float> VanillaMotionSwayX;
        public static ConfigEntry<float> VanillaMotionSwayY;
        public static ConfigEntry<float> VanillaMotionSwayZ;
        public static ConfigEntry<float> VanillaMotionInputClamp;
        public static ConfigEntry<float> VanillaWalkIntensity;
        public static ConfigEntry<float> VanillaWalkStepFrequency;
        public static ConfigEntry<float> VanillaHandShakeIntensity;
        public static ConfigEntry<float> VanillaForceIntensity;
        public static ConfigEntry<float> VanillaAimSwayScale;
        public static ConfigEntry<float> VanillaSwayFalloff;

        // ================= 99 Debug =======================================
        public static ConfigEntry<bool>             DebugOverlay;
        public static ConfigEntry<KeyboardShortcut> DebugKey;
        public static ConfigEntry<bool>             VerboseLogging;

        private static ConfigFile _file;

        public static void Bind(ConfigFile cfg)
        {
            _file = cfg;

            // ---- general -------------------------------------------------
            Enabled   = cfg.Bind(General, "Enabled", true, "Master switch. Off means stock EFT behaviour.");
            ToggleKey = cfg.Bind(General, "Toggle Key", new KeyboardShortcut(KeyCode.F10), "Turns the whole mod on and off mid-raid.");
            Preset    = cfg.Bind(General, "Preset", SwayPreset.Realistic,
                "Pick a starting point, then tick 'Apply Preset'. Presets overwrite every value below.");
            ApplyPreset = cfg.Bind(General, "Apply Preset", false,
                "Tick to write the selected preset over the current settings. Unticks itself when done.");

            MasterIntensity = F(General, "Master Intensity", 1f, 0f, 5f,
                "Scales every sway source at once. The one knob to turn if you only turn one.");
            AdsMaster = F(General, "ADS Multiplier", 1f, 0f, 5f, "Extra scale applied while aiming down sights.");
            HipMaster = F(General, "Hipfire Multiplier", 1f, 0f, 5f, "Extra scale applied while hipfiring.");
            GlobalTimeScale = F(General, "Rate Multiplier", 1f, 0.1f, 4f,
                "Speeds up or slows down every oscillator. Below 1 feels drowsy, above 1 feels jittery.");
            KeepVanillaSway = cfg.Bind(General, "Keep Vanilla Sway", true,
                "Leave BSG's own sway running underneath. Turn off for a from-scratch feel driven only by this mod.");

            // ---- respiration ---------------------------------------------
            RespEnabled   = cfg.Bind(Resp, "Enabled", true, "Chest movement from breathing. The slow, dominant wander.");
            RespAmplitude = F(Resp, "Amplitude", 1f, 0f, 5f, "Size of the breathing arc.");
            RespRateRest  = F(Resp, "Rate Rested (bpm)", 14f, 4f, 40f, "Breaths per minute with full stamina. 12-16 is a calm adult.");
            RespRateExhausted = F(Resp, "Rate Exhausted (bpm)", 32f, 4f, 80f, "Breaths per minute at zero stamina.");
            RespInhaleFraction = F(Resp, "Inhale Fraction", 0.35f, 0.05f, 0.9f, "Share of the cycle spent breathing in. Real breathing runs about 1:2 in to out.");
            RespExhaleFraction = F(Resp, "Exhale Fraction", 0.45f, 0.05f, 0.9f, "Share of the cycle spent breathing out.");
            RespPauseFraction  = F(Resp, "End-Expiratory Pause", 0.20f, 0f, 0.7f, "Still moment at the bottom of the breath. This is the window a real shooter fires in.");
            RespWaveformSharpness = F(Resp, "Waveform Sharpness", 1f, 0f, 3f,
                "0 is a plain sine. 1 is the asymmetric in-out-hold shape of real breathing. Higher exaggerates it.");
            RespPitch = F(Resp, "Pitch Weight", 1f, -3f, 3f, "Vertical share. Breathing is mostly vertical.");
            RespYaw   = F(Resp, "Yaw Weight", 0.38f, -3f, 3f, "Horizontal share.");
            RespRoll  = F(Resp, "Roll Weight", 0.22f, -3f, 3f, "Cant share.");
            RespAds   = F(Resp, "ADS Multiplier", 0.7f, 0f, 3f, "Shouldering the stock damps the chest, it does not stop it.");
            RespHip   = F(Resp, "Hipfire Multiplier", 1.15f, 0f, 3f, "Nothing braces the weapon at the hip.");
            RespHoldSuppression = F(Resp, "Breath Hold Suppression", 0.12f, 0f, 1f,
                "What is left of the breathing arc while holding breath. 0 freezes it completely.");
            RespHoldReboundGain  = F(Resp, "Hold Rebound Gain", 1.8f, 1f, 4f, "How hard breathing overshoots after you release a held breath.");
            RespHoldReboundDecay = F(Resp, "Hold Rebound Decay", 0.45f, 0.05f, 3f, "How fast that overshoot settles, per second.");
            RespOxygenInfluence  = F(Resp, "Oxygen Influence", 1f, 0f, 3f, "How much low oxygen drives rate and amplitude up.");
            RespIrregularity     = F(Resp, "Irregularity", 0.15f, 0f, 1f,
                "Breath-to-breath variation. Zero is metronomic and reads as fake; a little is what sells it.");

            // ---- cardiac --------------------------------------------------
            HeartEnabled = cfg.Bind(Heart, "Enabled", true,
                "Heartbeat pushed through the rifle. Small, sharp, and the thing people notice through a scope.");
            HeartAmplitude = F(Heart, "Amplitude", 1f, 0f, 5f, "Size of the pulse kick.");
            HeartRateRest  = F(Heart, "Resting BPM", 65f, 30f, 120f, "Heart rate with full stamina.");
            HeartRateMax   = F(Heart, "Max BPM", 165f, 60f, 220f, "Heart rate when spent.");
            HeartRateRise  = F(Heart, "Rate Rise Speed", 0.9f, 0.05f, 5f, "How fast the heart climbs under exertion, per second.");
            HeartRateFall  = F(Heart, "Rate Fall Speed", 0.25f, 0.02f, 5f, "How fast it comes back down. Slower than it climbs, as in life.");
            HeartSystolicSharpness = F(Heart, "Systolic Sharpness", 7f, 1f, 24f,
                "Higher makes each beat a tighter spike instead of a soft swell.");
            HeartDicroticStrength = F(Heart, "Dicrotic Notch", 0.35f, 0f, 1f,
                "The smaller second bump of each beat, from the aortic valve closing. Subtle, but it is why a heartbeat does not read as a sine.");
            HeartDicroticDelay = F(Heart, "Dicrotic Delay", 0.34f, 0.05f, 0.9f, "Where in the beat that second bump lands.");
            HeartPitch = F(Heart, "Pitch Weight", 1f, -3f, 3f, "Vertical share.");
            HeartYaw   = F(Heart, "Yaw Weight", 0.32f, -3f, 3f, "Horizontal share.");
            HeartRoll  = F(Heart, "Roll Weight", 0.14f, -3f, 3f, "Cant share.");
            HeartAds   = F(Heart, "ADS Multiplier", 1.25f, 0f, 3f,
                "A shouldered rifle transmits the pulse better than a slung one, and the sight magnifies it.");
            HeartHip = F(Heart, "Hipfire Multiplier", 0.5f, 0f, 3f, "Barely readable from the hip.");
            HeartExertionGain = F(Heart, "Exertion Amplitude Gain", 1.6f, 0f, 5f, "Extra pulse amplitude when the heart is racing.");

            // ---- tremor ---------------------------------------------------
            TremorEnabled = cfg.Bind(Tremor, "Enabled", true,
                "The 8-12 Hz shimmer every human hand has. Never still, never repeating.");
            TremorAmplitude = F(Tremor, "Amplitude", 1f, 0f, 5f, "Size of the shimmer.");
            TremorFrequency = F(Tremor, "Centre Frequency (Hz)", 9.5f, 1f, 30f, "Physiological tremor sits around 8-12 Hz.");
            TremorFrequencySpread = F(Tremor, "Frequency Spread", 0.35f, 0f, 1f,
                "Detunes the layers so the tremor never locks into a pattern.");
            TremorOctaves = cfg.Bind(Tremor, "Octaves", 3, new ConfigDescription(
                "How many detuned noise layers to stack. More costs a little CPU and looks slightly finer.",
                new AcceptableValueRange<int>(1, 6)));
            TremorFatigueGain = F(Tremor, "Fatigue Gain", 2.2f, 0f, 8f, "How much a spent shooter shakes compared to a fresh one.");
            TremorLoadGain    = F(Tremor, "Weapon Load Gain", 1.2f, 0f, 6f, "How much a heavy, unwieldy weapon adds.");
            TremorPitch = F(Tremor, "Pitch Weight", 1f, -3f, 3f, "Vertical share.");
            TremorYaw   = F(Tremor, "Yaw Weight", 0.95f, -3f, 3f, "Horizontal share.");
            TremorRoll  = F(Tremor, "Roll Weight", 0.45f, -3f, 3f, "Cant share.");
            TremorAds   = F(Tremor, "ADS Multiplier", 1.3f, 0f, 3f, "Longer sight radius makes the same wobble far more visible.");
            TremorHip   = F(Tremor, "Hipfire Multiplier", 0.75f, 0f, 3f, "Hard to see without sights.");

            // ---- postural drift ------------------------------------------
            DriftEnabled = cfg.Bind(Postural, "Enabled", true,
                "Standing balance drift. Sub-hertz, wide, and the reason a rested shooter still cannot hold a dot perfectly still.");
            DriftAmplitude = F(Postural, "Amplitude", 1f, 0f, 5f, "Size of the wander.");
            DriftFrequency = F(Postural, "Frequency (Hz)", 0.28f, 0.02f, 3f, "Body sway lives around 0.2-0.5 Hz.");
            DriftPitch = F(Postural, "Pitch Weight", 0.7f, -3f, 3f, "Vertical share.");
            DriftYaw   = F(Postural, "Yaw Weight", 1f, -3f, 3f, "Horizontal share. Balance drift is wider than it is tall.");
            DriftRoll  = F(Postural, "Roll Weight", 0.3f, -3f, 3f, "Cant share.");
            DriftAds   = F(Postural, "ADS Multiplier", 0.85f, 0f, 3f, "A tighter stance trims it a little.");
            DriftHip   = F(Postural, "Hipfire Multiplier", 1f, 0f, 3f, "Unchanged from the hip.");
            DriftMoveGain = F(Postural, "Movement Gain", 1.5f, 0f, 6f, "Extra drift while walking, on top of BSG's own step bob.");

            // ---- inertia --------------------------------------------------
            InertiaEnabled = cfg.Bind(Inertia, "Enabled", true,
                "Weapon mass fighting your turn. The muzzle lags going in and overshoots coming out.");
            InertiaStrength = F(Inertia, "Strength", 1f, 0f, 5f, "How far the weapon trails the aim.");
            InertiaFrequency = F(Inertia, "Natural Frequency (Hz)", 2.6f, 0.2f, 12f,
                "Stiffness of the arms holding the weapon. Lower is looser and swingier.");
            InertiaDamping = F(Inertia, "Damping Ratio", 0.62f, 0.05f, 2f,
                "Below 1 overshoots and settles, 1 stops dead. Around 0.6 is what arms actually do.");
            InertiaMassCoupling = F(Inertia, "Mass Coupling", 1f, 0f, 3f,
                "How much weapon weight slows the response. This is what makes an RSASS feel unlike a PP-19.");
            InertiaRollCoupling = F(Inertia, "Roll Coupling", 0.45f, -2f, 2f, "How much a horizontal turn cants the weapon.");
            InertiaMaxDeflection = F(Inertia, "Max Deflection", 4.5f, 0.1f, 30f, "Hard ceiling on inertia in degrees, so a flick cannot throw the gun off screen.");
            InertiaAds = F(Inertia, "ADS Multiplier", 0.5f, 0f, 3f, "The shoulder absorbs most of it once the stock is planted.");
            InertiaHip = F(Inertia, "Hipfire Multiplier", 1.2f, 0f, 3f, "Nothing to absorb it at the hip.");
            InertiaMoveGain = F(Inertia, "Movement Gain", 1f, 0f, 5f, "Inertia from the body accelerating, not just from turning.");
            InertiaTurnClamp = F(Inertia, "Turn Rate Clamp", 900f, 10f, 5000f, "Degrees per second of turn beyond which inertia stops growing. Keeps mouse flicks sane.");

            // ---- weapon handling ------------------------------------------
            ErgoHalfPoint = F(Weapon, "Ergonomics Half Point", 55f, 5f, 150f,
                "Ergonomics value at which sway sits halfway between the floor and the ceiling.");
            ErgoCurve = F(Weapon, "Ergonomics Curve", 1.15f, 0.2f, 4f,
                "Above 1 punishes bad ergonomics harder, below 1 flattens the difference between guns.");
            ErgoFloor   = F(Weapon, "Ergonomics Floor", 0.55f, 0.05f, 1f, "Least sway a perfectly ergonomic weapon can reach.");
            ErgoCeiling = F(Weapon, "Ergonomics Ceiling", 2.1f, 1f, 6f, "Most sway an awful weapon can reach.");
            WeightInfluence = F(Weapon, "Weight Influence", 1f, 0f, 4f, "How much raw weapon weight feeds tremor and inertia.");
            WeightReference = F(Weapon, "Reference Weight (kg)", 3.6f, 0.5f, 15f, "Weight treated as neutral. Heavier adds, lighter subtracts.");
            RecoilStatInfluence = F(Weapon, "Recoil Stat Influence", 0.35f, 0f, 3f, "How much the weapon's recoil stat leaks into sway.");

            // ---- stance ---------------------------------------------------
            StandMultiplier   = F(Stance, "Standing", 1f, 0f, 3f, "Baseline.");
            CrouchMultiplier  = F(Stance, "Crouched", 0.78f, 0f, 3f, "A lower centre of mass steadies things.");
            ProneMultiplier   = F(Stance, "Prone", 0.42f, 0f, 3f, "Ground contact removes most postural sway.");
            MountedMultiplier = F(Stance, "Mounted", 0.22f, 0f, 3f, "Resting on cover. Breathing still gets through.");
            BipodMultiplier   = F(Stance, "Bipod Deployed", 0.15f, 0f, 3f, "The steadiest a shooter gets.");
            LeanMultiplier    = F(Stance, "Leaning Penalty", 1.25f, 0.5f, 3f, "Extra sway at full lean. Holding a lean is genuinely harder.");
            SprintMultiplier  = F(Stance, "Sprinting", 1.4f, 0f, 4f, "Applies during the sprint-out recovery window.");
            PoseLevelInfluence = F(Stance, "Pose Level Influence", 1f, 0f, 3f, "How smoothly the stance multipliers blend across crouch height.");
            SupportBlendSpeed  = F(Stance, "Support Blend Speed", 7f, 0.5f, 30f, "How fast support states fade in and out, per second. Prevents snapping.");

            // ---- fatigue --------------------------------------------------
            FatigueEnabled   = cfg.Bind(Fatigue, "Enabled", true, "Let stamina and oxygen drive sway.");
            StaminaInfluence = F(Fatigue, "Stamina Influence", 1.3f, 0f, 5f, "How much empty stamina amplifies everything.");
            OxygenInfluence  = F(Fatigue, "Oxygen Influence", 1f, 0f, 5f, "How much oxygen debt amplifies everything.");
            HandsStaminaInfluence = F(Fatigue, "Arm Stamina Influence", 1.1f, 0f, 5f,
                "Arm stamina drains while aiming a heavy weapon. This is how much that shows.");
            ExhaustedMultiplier = F(Fatigue, "Exhausted Multiplier", 1.9f, 1f, 5f, "Applied on top once stamina bottoms out.");
            HoldFatigueRate  = F(Fatigue, "Sustained Aim Rate", 0.09f, 0f, 1f, "How fast holding an aim builds fatigue, per second.");
            HoldFatigueDecay = F(Fatigue, "Sustained Aim Recovery", 0.35f, 0.01f, 3f, "How fast it drains once you lower the weapon.");
            HoldFatigueMax   = F(Fatigue, "Sustained Aim Ceiling", 0.8f, 0f, 3f, "Most extra sway sustained aiming can add.");

            // ---- injury ---------------------------------------------------
            InjuryEnabled = cfg.Bind(Injury, "Enabled", true, "Let arm damage affect the hold.");
            ArmFractureMultiplier = F(Injury, "Fractured Arm", 2.4f, 1f, 8f, "A broken arm cannot hold a rifle steady.");
            ArmBlackedMultiplier  = F(Injury, "Blacked Arm", 1.7f, 1f, 8f, "Destroyed limb, no fracture.");
            TremorInjuryGain      = F(Injury, "Injury Tremor Gain", 2.2f, 0f, 8f, "Injury feeds tremor harder than it feeds the slow sources.");

            // ---- shot disturbance -----------------------------------------
            ShotEnabled = cfg.Bind(Recoil, "Enabled", true, "Let firing disturb the hold beyond recoil itself.");
            ShotGain    = F(Recoil, "Per-Shot Gain", 0.12f, 0f, 1f, "Sway added by each shot.");
            ShotDecay   = F(Recoil, "Decay", 1.1f, 0.05f, 6f, "How fast that settles, per second.");
            ShotMaxAccumulation = F(Recoil, "Ceiling", 0.9f, 0f, 4f, "Most a long burst can stack up.");
            ShotHeartRateKick   = F(Recoil, "Heart Rate Kick", 1.5f, 0f, 20f, "BPM added per shot. Firefights raise your pulse.");

            // ---- camera coupling ------------------------------------------
            CameraCoupling = F(Coupling, "Master Camera Coupling", 0.35f, 0f, 2f,
                "How much sway moves the view instead of only the weapon. 0 keeps the crosshair still and moves the gun, 1 moves your head with it.");
            CameraRespShare    = F(Coupling, "Respiration Share", 1f, 0f, 3f, "Breathing's share of camera coupling.");
            CameraHeartShare   = F(Coupling, "Cardiac Share", 0.8f, 0f, 3f, "Pulse's share.");
            CameraTremorShare  = F(Coupling, "Tremor Share", 0.35f, 0f, 3f, "Tremor's share. Keep this low or the view buzzes.");
            CameraDriftShare   = F(Coupling, "Drift Share", 1f, 0f, 3f, "Postural drift's share.");
            CameraInertiaShare = F(Coupling, "Inertia Share", 0.25f, 0f, 3f, "Inertia's share.");

            // ---- springs ---------------------------------------------------
            DriveGain = F(Springs, "Drive Gain", 8f, 0.1f, 60f,
                "Converts the model's output in degrees into the impulse BSG's springs expect. " +
                "This is the calibration between the physics and the game, not a taste setting: " +
                "if everything feels uniformly too strong or too weak, change Master Intensity first, " +
                "and only come here if the weapon feels like it is fighting the spring rather than riding it.");

            SpringOverride = cfg.Bind(Springs, "Override Springs", false,
                "Take over BSG's spring constants. Off leaves them exactly as shipped.");
            HandsRotDamping     = F(Springs, "Hands Rotation Damping", 0.55f, 0f, 1f, "Higher settles the weapon faster.");
            HandsRotReturnSpeed = F(Springs, "Hands Rotation Return Speed", 7f, 0.1f, 60f, "How hard the weapon is pulled back to centre.");
            HandsRotAccelerationMax = F(Springs, "Hands Rotation Acceleration Max", 12f, 0.1f, 200f, "Ceiling on a single frame's impulse.");
            HandsRotInputIntensity  = F(Springs, "Hands Rotation Input Intensity", 1f, 0f, 5f, "Scales everything entering the spring.");
            HandsRotSoftness    = F(Springs, "Hands Rotation Softness", 1f, 0f, 5f, "How gently the spring meets its travel limits.");
            HandsPosDamping     = F(Springs, "Hands Position Damping", 0.55f, 0f, 1f, "Same, for positional movement.");
            HandsPosReturnSpeed = F(Springs, "Hands Position Return Speed", 7f, 0.1f, 60f, "Same, for positional movement.");
            CameraRotDamping     = F(Springs, "Camera Rotation Damping", 0.6f, 0f, 1f, "Damping on the view spring.");
            CameraRotReturnSpeed = F(Springs, "Camera Rotation Return Speed", 8f, 0.1f, 60f, "Return speed on the view spring.");

            // ---- vanilla effectors -----------------------------------------
            VanillaOverride = cfg.Bind(Vanilla, "Override Vanilla Effectors", false,
                "Rewrite BSG's own effector parameters every frame. This is the deep end. Off means untouched.");
            VanillaBreathIntensity  = F(Vanilla, "Breath Intensity", 1f, 0f, 5f, "Scales BreathEffector.Intensity.");
            VanillaBreathFrequency  = F(Vanilla, "Breath Frequency", 1f, 0.1f, 5f, "Scales the vanilla breathing rate.");
            VanillaBreathShake      = F(Vanilla, "Breath Shake", 1f, 0f, 5f, "Scales the shake term inside the breath effector.");
            VanillaBreathHipPenalty = F(Vanilla, "Breath Hip Penalty", 1f, 0f, 5f, "Scales the extra breath sway applied at the hip.");
            VanillaBreathCameraSens = F(Vanilla, "Breath Camera Sensitivity", 1f, 0f, 5f, "How much vanilla breathing moves the camera.");
            VanillaBreathTremorOn   = cfg.Bind(Vanilla, "Vanilla Tremor Enabled", true, "BSG's own tremor. Turn off if you would rather only run this mod's.");
            VanillaBreathTremorAmplitude = F(Vanilla, "Vanilla Tremor Amplitude", 1f, 0f, 5f, "Scales BSG tremor size.");
            VanillaBreathTremorHardness  = F(Vanilla, "Vanilla Tremor Hardness", 1f, 0.1f, 5f, "Scales BSG tremor sharpness.");
            VanillaMotionIntensity = F(Vanilla, "Motion Intensity", 1f, 0f, 5f, "Scales MotionEffector.Intensity, which is BSG's own inertia.");
            VanillaMotionSwayX = F(Vanilla, "Motion Sway Factor X", 1f, 0f, 5f, "Scales MotionEffector.SwayFactors.x.");
            VanillaMotionSwayY = F(Vanilla, "Motion Sway Factor Y", 1f, 0f, 5f, "Scales MotionEffector.SwayFactors.y.");
            VanillaMotionSwayZ = F(Vanilla, "Motion Sway Factor Z", 1f, 0f, 5f, "Scales MotionEffector.SwayFactors.z.");
            VanillaMotionInputClamp = F(Vanilla, "Motion Input Clamp", 1f, 0.1f, 5f, "Scales the clamp on mouse input feeding vanilla inertia.");
            VanillaWalkIntensity     = F(Vanilla, "Walk Bob Intensity", 1f, 0f, 5f, "Scales WalkEffector.Intensity.");
            VanillaWalkStepFrequency = F(Vanilla, "Walk Step Frequency", 1f, 0.1f, 5f, "Scales the step cadence of the walk bob.");
            VanillaHandShakeIntensity = F(Vanilla, "Hand Shake Intensity", 1f, 0f, 5f, "Scales the low-stamina hand shake effector.");
            VanillaForceIntensity     = F(Vanilla, "Force Effector Intensity", 1f, 0f, 5f, "Scales explosion and impact shake.");
            VanillaAimSwayScale = F(Vanilla, "Aim Sway Scale", 1f, 0f, 5f, "Scales the PWA aim sway envelope, min and max together.");
            VanillaSwayFalloff  = F(Vanilla, "Sway Falloff", 1f, 0.1f, 5f, "Scales how quickly aim sway decays.");

            // ---- debug ------------------------------------------------------
            DebugOverlay   = cfg.Bind(Debug, "Overlay", false, "On-screen readout of every live value the model is using.");
            DebugKey       = cfg.Bind(Debug, "Overlay Key", new KeyboardShortcut(KeyCode.F11), "Toggles that readout.");
            VerboseLogging = cfg.Bind(Debug, "Verbose Logging", false, "Chatty console output. Useful when a value is not doing what you expect.");
        }

        private static ConfigEntry<float> F(string section, string key, float def, float min, float max, string desc)
        {
            return _file.Bind(section, key, def, new ConfigDescription(desc, new AcceptableValueRange<float>(min, max)));
        }
    }
}

using EFT;
using EFT.Animations;
using UnityEngine;

namespace SptSway.Runtime
{
    /// <summary>
    /// Owns the model for the local player and drives it once per frame.
    ///
    /// The signal chain is deliberately linear and in one place, so that any
    /// number you see on screen can be traced back to exactly one config entry:
    ///
    ///   sources -> per-source ADS/hip weights -> handling -> fatigue ->
    ///   injury -> support/stance -> shot disturbance -> master -> springs
    /// </summary>
    public sealed class SwayDirector
    {
        public static readonly SwayDirector Instance = new SwayDirector();

        public readonly ShooterState  State   = new ShooterState();
        public readonly Respiration   Breath  = new Respiration();
        public readonly Cardiac       Heart   = new Cardiac();
        public readonly Tremor        Shake   = new Tremor();
        public readonly PosturalDrift Drift   = new PosturalDrift();
        public readonly WeaponInertia Inertia = new WeaponInertia();

        private readonly VanillaTuning _tuning = new VanillaTuning();

        private float _holdFatigue;
        private float _shotCharge;

        // last frame's output, kept for the debug overlay
        public Vector3 LastHands;
        public Vector3 LastCamera;
        public float   LastTotalMultiplier = 1f;

        private SwayDirector() { }

        public void OnWeaponChanged()
        {
            _tuning.Reset();
            Inertia.Reset();
        }

        public void OnShot()
        {
            if (!SwayConfig.ShotEnabled.Value) return;
            _shotCharge = Mathf.Min(_shotCharge + SwayConfig.ShotGain.Value,
                                    SwayConfig.ShotMaxAccumulation.Value);
            Heart.AddShotKick(SwayConfig.ShotHeartRateKick.Value);
        }

        public void Tick(Player player, ProceduralWeaponAnimation pwa, float dt)
        {
            if (player == null || pwa == null) return;

            var hands = pwa.HandsContainer;
            if (hands == null) return;

            // A hitch or an alt-tab must not be allowed to integrate a spring
            // across half a second of missing time.
            dt = Mathf.Clamp(dt, 0.0005f, 0.05f);

            _tuning.Apply(pwa);

            if (!SwayConfig.Enabled.Value)
            {
                LastHands = LastCamera = Vector3.zero;
                return;
            }

            State.Sample(player, pwa, dt);

            DriveInertia(player, pwa, dt);

            Vector3 resp    = Breath.Evaluate(State, dt);
            Vector3 heart   = Heart.Evaluate(State, dt);
            Vector3 tremor  = Shake.Evaluate(State, dt);
            Vector3 drift   = Drift.Evaluate(State, dt);
            Vector3 inertia = Inertia.Evaluate(State, dt);

            float global = GlobalMultiplier(dt);
            LastTotalMultiplier = global;

            // Handling scales the sources the shooter's grip actually governs.
            // Inertia is excluded because weapon mass already shapes it inside
            // the oscillator, and scaling it twice makes heavy guns unusable.
            float handling = State.HandlingFactor;

            Vector3 handsSum =
                  resp   * handling
                + heart  * handling
                + tremor * handling
                + drift
                + inertia;

            handsSum *= global;

            // The camera takes a configurable share of each source. Sending
            // everything to the weapon alone looks fake, and sending everything
            // to the camera makes the game unplayable, so it is per-source.
            float cc = SwayConfig.CameraCoupling.Value;
            Vector3 cameraSum = cc <= 0f ? Vector3.zero : (
                  resp    * SwayConfig.CameraRespShare.Value    * handling
                + heart   * SwayConfig.CameraHeartShare.Value   * handling
                + tremor  * SwayConfig.CameraTremorShare.Value  * handling
                + drift   * SwayConfig.CameraDriftShare.Value
                + inertia * SwayConfig.CameraInertiaShare.Value
            ) * (cc * global);

            LastHands  = handsSum;
            LastCamera = cameraSum;

            // The springs are stepped on the physics clock while this runs on
            // the render clock, so the impulse is normalised to a 60 Hz frame.
            // Without this the whole mod would be twice as strong at 120 fps.
            float frameNorm = dt * 60f * SwayConfig.DriveGain.Value;

            if (hands.HandsRotation != null && handsSum.sqrMagnitude > 0f)
                hands.HandsRotation.AddAcceleration(handsSum * frameNorm);

            if (hands.CameraRotation != null && cameraSum.sqrMagnitude > 0f)
                hands.CameraRotation.AddAcceleration(cameraSum * frameNorm);
        }

        private void DriveInertia(Player player, ProceduralWeaponAnimation pwa, float dt)
        {
            // Taken from where the view actually ended up rather than from raw
            // mouse input, so sensitivity changes, controllers and any mod that
            // moves the view all feed inertia the same way.
            Transform cam = pwa.HandsContainer != null ? pwa.HandsContainer.CameraTransform : null;
            Vector3 euler = cam != null ? cam.eulerAngles : Vector3.zero;

            Vector3 velocity = Vector3.zero;
            try { velocity = player.Velocity; } catch { }

            Inertia.Drive(new Vector2(euler.y, euler.x), velocity, State, dt);
        }

        private float GlobalMultiplier(float dt)
        {
            float m = SwayConfig.MasterIntensity.Value;

            m *= State.IsAiming ? SwayConfig.AdsMaster.Value : SwayConfig.HipMaster.Value;
            m *= State.SupportFactor;

            if (SwayConfig.FatigueEnabled.Value)
            {
                // Holding an aim is work. It builds while the weapon is up and
                // drains once it comes down, which is why you cannot solve a
                // long overwatch by simply never lowering the rifle.
                float target = State.IsAiming ? SwayConfig.HoldFatigueMax.Value : 0f;
                float rate = State.IsAiming ? SwayConfig.HoldFatigueRate.Value
                                            : SwayConfig.HoldFatigueDecay.Value;
                _holdFatigue = Mathf.MoveTowards(_holdFatigue, target, rate * dt);
                m *= 1f + _holdFatigue;

                if (State.Exhausted)
                    m *= SwayConfig.ExhaustedMultiplier.Value;
            }

            if (SwayConfig.InjuryEnabled.Value)
            {
                if (State.ArmFractured) m *= SwayConfig.ArmFractureMultiplier.Value;
                if (State.ArmBlacked)   m *= SwayConfig.ArmBlackedMultiplier.Value;
            }

            _shotCharge = Mathf.Max(0f, _shotCharge - dt * SwayConfig.ShotDecay.Value);
            m *= 1f + _shotCharge;

            return m;
        }
    }

    /// <summary>
    /// EFT hands the physical condition mask to the weapon animator rather than
    /// exposing it on the player, so we take a copy on the way past.
    /// </summary>
    public static class PhysicalConditionTracker
    {
        public static EPhysicalCondition Current = EPhysicalCondition.None;
    }
}

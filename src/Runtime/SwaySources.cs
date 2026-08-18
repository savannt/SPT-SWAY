using SptSway.Util;
using UnityEngine;

namespace SptSway.Runtime
{
    /// <summary>
    /// A single physical source of sway. Each one produces degrees of rotation
    /// as (pitch, yaw, roll) and knows nothing about the others.
    /// </summary>
    public interface ISwaySource
    {
        Vector3 Evaluate(ShooterState s, float dt);
        void Reset();
    }

    /// <summary>
    /// Removes the DC offset from a signal so a source can never drag the point
    /// of aim off centre over time, however asymmetric its waveform is.
    /// </summary>
    internal struct DcBlocker
    {
        private float _mean;
        private bool  _primed;

        public float Apply(float value, float dt, float timeConstant)
        {
            return value - Track(value, dt, timeConstant);
        }

        /// <summary>Advances the running mean and returns it, for callers centring several axes on one signal.</summary>
        public float Track(float value, float dt, float timeConstant)
        {
            if (!_primed) { _mean = value; _primed = true; }
            float k = 1f - Mathf.Exp(-dt / Mathf.Max(0.01f, timeConstant));
            _mean += (value - _mean) * k;
            return _mean;
        }

        public void Reset() { _mean = 0f; _primed = false; }
    }

    // =====================================================================
    /// <summary>
    /// Breathing. The slowest and largest source, and the one a shooter times
    /// their trigger break against.
    ///
    /// The waveform is not a sine. A real breath rises quickly, falls more
    /// slowly, and then sits still for a moment at the bottom before the next
    /// one starts. That still moment is the whole point: it is the window you
    /// fire in, and a sine has no such window.
    /// </summary>
    public sealed class Respiration : ISwaySource
    {
        private float _phase;
        private float _cycleJitter = 1f;
        private float _holdBlend;
        private float _rebound;
        private bool  _wasHolding;
        private DcBlocker _dc;

        public float Phase => _phase;
        public float RateBpm { get; private set; }

        public Vector3 Evaluate(ShooterState s, float dt)
        {
            if (!SwayConfig.RespEnabled.Value) return Vector3.zero;

            float oxyDebt = Mathf.Clamp01((1f - s.Oxygen) * SwayConfig.RespOxygenInfluence.Value);
            float drive   = Mathf.Clamp01(Mathf.Max(s.Exertion, oxyDebt));

            RateBpm = Mathf.Lerp(SwayConfig.RespRateRest.Value,
                                 SwayConfig.RespRateExhausted.Value,
                                 drive) * _cycleJitter;

            float prev = _phase;
            _phase += (RateBpm / 60f) * dt * SwayConfig.GlobalTimeScale.Value;

            if (_phase >= 1f)
            {
                _phase -= Mathf.Floor(_phase);
                // Re-roll the jitter once per breath rather than per frame, so
                // breaths differ from each other instead of shimmering.
                float irr = SwayConfig.RespIrregularity.Value;
                _cycleJitter = 1f + SwayNoise.Signed(prev * 13.37f + Time.time * 0.1f, 4801) * irr * 0.35f;
            }

            // Yaw and roll are not scaled copies of pitch. A chest expanding
            // moves the shoulders on a slightly later schedule than the sternum,
            // so the off-axis components lag by a fraction of a breath. Sampling
            // the same waveform at a small phase offset gives that, and keeps the
            // motion an ellipse rather than a straight diagonal line.
            float wPitch = Waveform(_phase);
            float wYaw   = Waveform(Frac(_phase + 0.09f));
            float wRoll  = Waveform(Frac(_phase + 0.17f));

            // One shared mean, so the three axes stay in step with each other
            // while still being individually centred.
            float mean = _dc.Track(wPitch, dt, 6f);
            wPitch -= mean; wYaw -= mean; wRoll -= mean;

            // Breath hold. The chest does not lock solid, it just gets very
            // quiet, and then pays for it when you let go.
            bool holding = s.HoldingBreath;
            float holdTarget = holding ? SwayConfig.RespHoldSuppression.Value : 1f;
            _holdBlend = Mathf.Lerp(_holdBlend, holdTarget, Mathf.Clamp01(dt * 8f));

            if (_wasHolding && !holding)
                _rebound = SwayConfig.RespHoldReboundGain.Value - 1f;
            _wasHolding = holding;
            _rebound = Mathf.Max(0f, _rebound - dt * SwayConfig.RespHoldReboundDecay.Value);

            float amp = SwayConfig.RespAmplitude.Value
                      * _holdBlend
                      * (1f + _rebound)
                      * (1f + drive * 0.9f)
                      * 0.55f;                    // degrees at amplitude 1

            float ads = s.IsAiming ? SwayConfig.RespAds.Value : SwayConfig.RespHip.Value;
            amp *= ads;

            return new Vector3(
                wPitch * SwayConfig.RespPitch.Value,
                wYaw   * SwayConfig.RespYaw.Value,
                wRoll  * SwayConfig.RespRoll.Value
            ) * amp;
        }

        private static float Frac(float v)
        {
            return v - Mathf.Floor(v);
        }

        private float Waveform(float p)
        {
            float ti = Mathf.Max(0.01f, SwayConfig.RespInhaleFraction.Value);
            float te = Mathf.Max(0.01f, SwayConfig.RespExhaleFraction.Value);
            float tp = Mathf.Max(0f,    SwayConfig.RespPauseFraction.Value);
            float sum = ti + te + tp;
            ti /= sum; te /= sum;

            float shaped;
            if (p < ti)
            {
                // Inhale: brisk, muscular, accelerating out of the pause.
                float u = p / ti;
                shaped = u * u * (3f - 2f * u);
            }
            else if (p < ti + te)
            {
                // Exhale: passive elastic recoil of the chest wall, so it starts
                // fast and trails off rather than mirroring the inhale.
                float u = (p - ti) / te;
                shaped = 1f - (1f - Mathf.Exp(-3.2f * u)) / (1f - Mathf.Exp(-3.2f));
            }
            else
            {
                shaped = 0f;
            }

            float sine = 0.5f - 0.5f * Mathf.Cos(p * 2f * Mathf.PI);
            float k = SwayConfig.RespWaveformSharpness.Value;

            if (k <= 1f)
                return Mathf.Lerp(sine, shaped, k);

            // Past 1 the shape gets pushed further from the sine, exaggerating
            // the pause without changing the period.
            float extra = Mathf.Clamp01(k - 1f);
            return Mathf.Lerp(shaped, Mathf.Pow(shaped, 1f + extra * 1.5f), extra);
        }

        public void Reset()
        {
            _phase = 0f; _holdBlend = 1f; _rebound = 0f; _wasHolding = false; _dc.Reset();
        }
    }

    // =====================================================================
    /// <summary>
    /// The heartbeat, transmitted through the chest and the support arm into
    /// the weapon. Tiny at rest and impossible to ignore through magnification
    /// after a sprint.
    ///
    /// The waveform is a sharp systolic spike followed by a smaller dicrotic
    /// bump where the aortic valve slams shut. That second bump is what stops
    /// a pulse reading as a sine wave to the eye.
    /// </summary>
    public sealed class Cardiac : ISwaySource
    {
        private float _phase;
        private float _bpm = 65f;
        private float _shotKick;
        private DcBlocker _dc;

        public float Bpm => _bpm;

        public void AddShotKick(float bpm)
        {
            _shotKick = Mathf.Min(_shotKick + bpm, 60f);
        }

        public Vector3 Evaluate(ShooterState s, float dt)
        {
            if (!SwayConfig.HeartEnabled.Value) return Vector3.zero;

            float target = Mathf.Lerp(SwayConfig.HeartRateRest.Value,
                                      SwayConfig.HeartRateMax.Value,
                                      s.Exertion) + _shotKick;

            // The heart climbs faster than it comes down, which is why you can
            // sprint into a fight and then wait a long time to settle.
            float speed = target > _bpm ? SwayConfig.HeartRateRise.Value : SwayConfig.HeartRateFall.Value;
            _bpm = Mathf.Lerp(_bpm, target, Mathf.Clamp01(dt * speed * 2f));
            _shotKick = Mathf.Max(0f, _shotKick - dt * 6f);

            _phase += (_bpm / 60f) * dt * SwayConfig.GlobalTimeScale.Value;
            _phase -= Mathf.Floor(_phase);

            float beat = Beat(_phase);
            beat = _dc.Apply(beat, dt, 3f);

            float exertionGain = 1f + s.Exertion * SwayConfig.HeartExertionGain.Value;
            float amp = SwayConfig.HeartAmplitude.Value * exertionGain * 0.085f;
            amp *= s.IsAiming ? SwayConfig.HeartAds.Value : SwayConfig.HeartHip.Value;

            // A pulse arrives as one push, not as three independent wobbles, so
            // all three axes share the same waveform and differ only in weight.
            return new Vector3(
                beat * SwayConfig.HeartPitch.Value,
                beat * SwayConfig.HeartYaw.Value,
                beat * SwayConfig.HeartRoll.Value
            ) * amp;
        }

        private float Beat(float p)
        {
            const float systoleWidth = 0.30f;
            float k = SwayConfig.HeartSystolicSharpness.Value;

            float systole = 0f;
            if (p < systoleWidth)
                systole = Mathf.Pow(Mathf.Sin(Mathf.PI * p / systoleWidth), k);

            float d = SwayConfig.HeartDicroticDelay.Value;
            float dw = systoleWidth * 0.8f;
            float dicrotic = 0f;
            if (p >= d && p < d + dw)
                dicrotic = Mathf.Pow(Mathf.Sin(Mathf.PI * (p - d) / dw), k * 1.4f)
                         * SwayConfig.HeartDicroticStrength.Value;

            return systole + dicrotic;
        }

        public void Reset() { _phase = 0f; _bpm = SwayConfig.HeartRateRest.Value; _shotKick = 0f; _dc.Reset(); }
    }

    // =====================================================================
    /// <summary>
    /// Physiological tremor: the 8-12 Hz shimmer that every human hand has and
    /// no amount of training removes. Fatigue widens it, weapon weight widens
    /// it, and magnification makes it obvious.
    /// </summary>
    public sealed class Tremor : ISwaySource
    {
        private float _t;

        public Vector3 Evaluate(ShooterState s, float dt)
        {
            if (!SwayConfig.TremorEnabled.Value) return Vector3.zero;

            _t += dt * SwayConfig.GlobalTimeScale.Value;
            float f = SwayConfig.TremorFrequency.Value;
            int oct = SwayConfig.TremorOctaves.Value;
            float spread = SwayConfig.TremorFrequencySpread.Value;

            float x = SwayNoise.Fractal(_t * f, 101, oct, spread);
            float y = SwayNoise.Fractal(_t * f, 227, oct, spread);
            float z = SwayNoise.Fractal(_t * f, 353, oct, spread);

            float fatigue = 1f + s.Exertion * SwayConfig.TremorFatigueGain.Value;

            float refW = Mathf.Max(0.1f, SwayConfig.WeightReference.Value);
            float load = 1f + Mathf.Max(0f, (s.WeightKg - refW) / refW) * SwayConfig.TremorLoadGain.Value;

            float injury = 1f;
            if (SwayConfig.InjuryEnabled.Value)
            {
                float g = SwayConfig.TremorInjuryGain.Value;
                if (s.ArmFractured) injury += g * 0.6f;
                if (s.ArmBlacked)   injury += g * 0.4f;
                if (s.NeuroTremor)  injury += g * 0.5f;
            }

            float amp = SwayConfig.TremorAmplitude.Value * fatigue * load * injury * 0.06f;
            amp *= s.IsAiming ? SwayConfig.TremorAds.Value : SwayConfig.TremorHip.Value;

            return new Vector3(
                x * SwayConfig.TremorPitch.Value,
                y * SwayConfig.TremorYaw.Value,
                z * SwayConfig.TremorRoll.Value
            ) * amp;
        }

        public void Reset() { _t = 0f; }
    }

    // =====================================================================
    /// <summary>
    /// Postural drift: the slow wander of a standing body correcting its own
    /// balance, well under 1 Hz. It is why a rested shooter with a light rifle
    /// still cannot pin a dot to a target, and it mostly disappears once you go
    /// prone or put the weapon on something.
    /// </summary>
    public sealed class PosturalDrift : ISwaySource
    {
        private float _t;

        public Vector3 Evaluate(ShooterState s, float dt)
        {
            if (!SwayConfig.DriftEnabled.Value) return Vector3.zero;

            _t += dt * SwayConfig.GlobalTimeScale.Value;
            float f = SwayConfig.DriftFrequency.Value;

            float x = SwayNoise.Fractal(_t * f, 1901, 2, 0.4f);
            float y = SwayNoise.Fractal(_t * f, 2903, 2, 0.4f);
            float z = SwayNoise.Fractal(_t * f, 3907, 2, 0.4f);

            // Walking is a balance problem, so it feeds the same source.
            float move = 1f + Mathf.Clamp01(s.MoveSpeed / 4f) * SwayConfig.DriftMoveGain.Value;
            float fatigue = 1f + s.Exertion * 0.8f;

            float amp = SwayConfig.DriftAmplitude.Value * move * fatigue * 0.22f;
            amp *= s.IsAiming ? SwayConfig.DriftAds.Value : SwayConfig.DriftHip.Value;

            return new Vector3(
                x * SwayConfig.DriftPitch.Value,
                y * SwayConfig.DriftYaw.Value,
                z * SwayConfig.DriftRoll.Value
            ) * amp;
        }

        public void Reset() { _t = 0f; }
    }

    // =====================================================================
    /// <summary>
    /// Weapon inertia. The weapon is a mass on the end of two compliant arms,
    /// so it trails a turn going in and overshoots coming out.
    ///
    /// This is a real damped harmonic oscillator driven by the turn rate rather
    /// than a lerp-toward-zero, which is what gives the overshoot its shape:
    /// a heavy weapon does not just move less, it moves later and settles for
    /// longer, and the config exposes both halves of that separately.
    /// </summary>
    public sealed class WeaponInertia : ISwaySource
    {
        private Vector3 _x;               // current deflection, degrees
        private Vector3 _v;               // rate of change
        private Vector2 _lastAngles;
        private Vector3 _lastVelocity;
        private bool _primed;

        public Vector3 Deflection => _x;

        public void Drive(Vector2 currentAngles, Vector3 playerVelocity, ShooterState s, float dt)
        {
            if (!_primed)
            {
                _lastAngles = currentAngles;
                _lastVelocity = playerVelocity;
                _primed = true;
                return;
            }

            float clamp = SwayConfig.InertiaTurnClamp.Value;

            float dYaw   = Mathf.DeltaAngle(_lastAngles.x, currentAngles.x) / Mathf.Max(dt, 0.0001f);
            float dPitch = Mathf.DeltaAngle(_lastAngles.y, currentAngles.y) / Mathf.Max(dt, 0.0001f);
            _lastAngles = currentAngles;

            dYaw   = Mathf.Clamp(dYaw,   -clamp, clamp);
            dPitch = Mathf.Clamp(dPitch, -clamp, clamp);

            Vector3 accel = (playerVelocity - _lastVelocity) / Mathf.Max(dt, 0.0001f);
            _lastVelocity = playerVelocity;
            accel = Vector3.ClampMagnitude(accel, 40f);

            // Heavier weapons take a larger impulse from the same turn, because
            // the arms have to fight more angular momentum.
            float refW = Mathf.Max(0.1f, SwayConfig.WeightReference.Value);
            float mass = 1f + Mathf.Max(-0.6f, (s.WeightKg - refW) / refW) * SwayConfig.InertiaMassCoupling.Value;

            float gain = SwayConfig.InertiaStrength.Value * mass * 0.0016f;
            float moveGain = SwayConfig.InertiaMoveGain.Value * 0.004f;

            _target = new Vector3(
                -dPitch * gain,
                -dYaw   * gain,
                -dYaw   * gain * SwayConfig.InertiaRollCoupling.Value
            );

            _target += new Vector3(accel.y, 0f, -accel.x) * moveGain;
        }

        private Vector3 _target;

        public Vector3 Evaluate(ShooterState s, float dt)
        {
            if (!SwayConfig.InertiaEnabled.Value)
            {
                _x = Vector3.zero; _v = Vector3.zero;
                return Vector3.zero;
            }

            // Stiffer arms mean a higher natural frequency; a heavy weapon on
            // the same arms lowers it, which is the whole reason a machine gun
            // feels sluggish and a pistol feels twitchy.
            float refW = Mathf.Max(0.1f, SwayConfig.WeightReference.Value);
            float massRatio = Mathf.Clamp(s.WeightKg / refW, 0.35f, 3.5f);
            float f = SwayConfig.InertiaFrequency.Value / Mathf.Sqrt(massRatio);

            float omega = 2f * Mathf.PI * Mathf.Max(0.05f, f);
            float zeta  = SwayConfig.InertiaDamping.Value;

            // Semi-implicit integration, and a substep cap so a frame spike
            // cannot make the spring explode.
            int steps = Mathf.Clamp(Mathf.CeilToInt(dt * omega / 0.4f), 1, 8);
            float h = dt / steps;

            for (int i = 0; i < steps; i++)
            {
                Vector3 a = (_target - _x) * (omega * omega) - _v * (2f * zeta * omega);
                _v += a * h;
                _x += _v * h;
            }

            float limit = SwayConfig.InertiaMaxDeflection.Value;
            _x = Vector3.ClampMagnitude(_x, limit);

            float ads = s.IsAiming ? SwayConfig.InertiaAds.Value : SwayConfig.InertiaHip.Value;
            return _x * ads;
        }

        public void Reset()
        {
            _x = Vector3.zero; _v = Vector3.zero; _target = Vector3.zero; _primed = false;
        }
    }
}

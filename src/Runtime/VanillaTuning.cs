using EFT.Animations;
using UnityEngine;

namespace SptSway.Runtime
{
    /// <summary>
    /// Scales BSG's own effector parameters.
    ///
    /// Everything here is a multiplier against the values the game shipped with,
    /// captured once the first time we see a weapon. That matters: writing
    /// absolute numbers would break the moment BSG retunes a weapon class, and
    /// re-reading the live value every frame would compound the scale until the
    /// gun left the screen.
    /// </summary>
    public sealed class VanillaTuning
    {
        private bool _captured;

        // captured originals
        private float _breathIntensity, _breathFrequency, _breathShake, _breathHipPenalty, _breathCameraSens;
        private float _tremorX, _tremorY, _tremorZ, _tremorHardX, _tremorHardY, _tremorHardZ;
        private bool  _tremorOn;
        private float _motionIntensity, _motionClamp;
        private Vector3 _motionSway;
        private float _walkIntensity, _walkStep;
        private float _handShake;
        private float _forceIntensity;
        private Vector3 _aimSwayMin, _aimSwayMax;
        private float _swayFalloff;

        private float _handsRotDamping, _handsRotReturn, _handsRotAccelMax, _handsRotInput, _handsRotSoftness;
        private float _handsPosDamping, _handsPosReturn;
        private float _camRotDamping, _camRotReturn;

        public void Reset() { _captured = false; }

        public void Apply(ProceduralWeaponAnimation pwa)
        {
            if (pwa == null) return;

            Capture(pwa);
            if (!_captured) return;

            if (SwayConfig.VanillaOverride.Value)
                ApplyEffectors(pwa);
            else
                RestoreEffectors(pwa);

            if (SwayConfig.SpringOverride.Value)
                ApplySprings(pwa);
            else
                RestoreSprings(pwa);
        }

        private void Capture(ProceduralWeaponAnimation pwa)
        {
            if (_captured) return;

            var breath = pwa.Breath;
            if (breath == null) return;   // weapon not fully wired up yet, try again next frame

            _breathIntensity  = breath.Intensity;
            _breathFrequency  = breath._breathFrequency;
            _breathShake      = breath._shakeIntensity;
            _breathHipPenalty = breath.HipPenalty;
            _breathCameraSens = breath._cameraSensetivity;
            _tremorOn         = breath.TremorOn;

            if (breath.TremorXRandom != null) { _tremorX = breath.TremorXRandom.Amplitude; _tremorHardX = breath.TremorXRandom.Hardness; }
            if (breath.TremorYRandom != null) { _tremorY = breath.TremorYRandom.Amplitude; _tremorHardY = breath.TremorYRandom.Hardness; }
            if (breath.TremorZRandom != null) { _tremorZ = breath.TremorZRandom.Amplitude; _tremorHardZ = breath.TremorZRandom.Hardness; }

            var motion = pwa.MotionReact;
            if (motion != null)
            {
                _motionIntensity = motion.Intensity;
                _motionSway      = motion.SwayFactors;
                _motionClamp     = motion.RotationInputClamp;
            }

            var walk = pwa.Walk;
            if (walk != null) { _walkIntensity = walk.Intensity; _walkStep = walk.StepFrequency; }

            var shake = pwa.HandShakeEffector;
            if (shake != null) _handShake = shake._shakeIntensity;

            var force = pwa.ForceReact;
            if (force != null) _forceIntensity = force.Intensity;

            _aimSwayMin  = pwa.AimSwayMin;
            _aimSwayMax  = pwa.AimSwayMax;
            _swayFalloff = pwa.SwayFalloff;

            var hc = pwa.HandsContainer;
            if (hc != null)
            {
                if (hc.HandsRotation != null)
                {
                    _handsRotDamping  = hc.HandsRotation.Damping;
                    _handsRotReturn   = hc.HandsRotation.ReturnSpeed;
                    _handsRotAccelMax = hc.HandsRotation.AccelerationMax;
                    _handsRotInput    = hc.HandsRotation.InputIntensity;
                    _handsRotSoftness = hc.HandsRotation.Softness;
                }
                if (hc.HandsPosition != null)
                {
                    _handsPosDamping = hc.HandsPosition.Damping;
                    _handsPosReturn  = hc.HandsPosition.ReturnSpeed;
                }
                if (hc.CameraRotation != null)
                {
                    _camRotDamping = hc.CameraRotation.Damping;
                    _camRotReturn  = hc.CameraRotation.ReturnSpeed;
                }
            }

            _captured = true;
        }

        private void ApplyEffectors(ProceduralWeaponAnimation pwa)
        {
            var breath = pwa.Breath;
            if (breath != null)
            {
                breath.Intensity          = _breathIntensity  * SwayConfig.VanillaBreathIntensity.Value;
                breath._breathIntensity   = _breathIntensity  * SwayConfig.VanillaBreathIntensity.Value;
                breath._breathFrequency   = _breathFrequency  * SwayConfig.VanillaBreathFrequency.Value;
                breath._shakeIntensity    = _breathShake      * SwayConfig.VanillaBreathShake.Value;
                breath.HipPenalty         = _breathHipPenalty * SwayConfig.VanillaBreathHipPenalty.Value;
                breath._cameraSensetivity = _breathCameraSens * SwayConfig.VanillaBreathCameraSens.Value;
                breath.TremorOn           = _tremorOn && SwayConfig.VanillaBreathTremorOn.Value;

                float ta = SwayConfig.VanillaBreathTremorAmplitude.Value;
                float th = SwayConfig.VanillaBreathTremorHardness.Value;
                if (breath.TremorXRandom != null) { breath.TremorXRandom.Amplitude = _tremorX * ta; breath.TremorXRandom.Hardness = _tremorHardX * th; }
                if (breath.TremorYRandom != null) { breath.TremorYRandom.Amplitude = _tremorY * ta; breath.TremorYRandom.Hardness = _tremorHardY * th; }
                if (breath.TremorZRandom != null) { breath.TremorZRandom.Amplitude = _tremorZ * ta; breath.TremorZRandom.Hardness = _tremorHardZ * th; }
            }

            var motion = pwa.MotionReact;
            if (motion != null)
            {
                motion.Intensity = _motionIntensity * SwayConfig.VanillaMotionIntensity.Value;
                motion.SwayFactors = new Vector3(
                    _motionSway.x * SwayConfig.VanillaMotionSwayX.Value,
                    _motionSway.y * SwayConfig.VanillaMotionSwayY.Value,
                    _motionSway.z * SwayConfig.VanillaMotionSwayZ.Value);
                motion.RotationInputClamp = _motionClamp * SwayConfig.VanillaMotionInputClamp.Value;
            }

            var walk = pwa.Walk;
            if (walk != null)
            {
                walk.Intensity     = _walkIntensity * SwayConfig.VanillaWalkIntensity.Value;
                walk.StepFrequency = _walkStep      * SwayConfig.VanillaWalkStepFrequency.Value;
            }

            var shake = pwa.HandShakeEffector;
            if (shake != null) shake._shakeIntensity = _handShake * SwayConfig.VanillaHandShakeIntensity.Value;

            var force = pwa.ForceReact;
            if (force != null) force.Intensity = _forceIntensity * SwayConfig.VanillaForceIntensity.Value;

            float aim = SwayConfig.VanillaAimSwayScale.Value;
            pwa.AimSwayMin  = _aimSwayMin * aim;
            pwa.AimSwayMax  = _aimSwayMax * aim;
            pwa.SwayFalloff = _swayFalloff * SwayConfig.VanillaSwayFalloff.Value;
        }

        private void RestoreEffectors(ProceduralWeaponAnimation pwa)
        {
            var breath = pwa.Breath;
            if (breath != null)
            {
                breath.Intensity          = _breathIntensity;
                breath._breathIntensity   = _breathIntensity;
                breath._breathFrequency   = _breathFrequency;
                breath._shakeIntensity    = _breathShake;
                breath.HipPenalty         = _breathHipPenalty;
                breath._cameraSensetivity = _breathCameraSens;
                breath.TremorOn           = _tremorOn;
                if (breath.TremorXRandom != null) { breath.TremorXRandom.Amplitude = _tremorX; breath.TremorXRandom.Hardness = _tremorHardX; }
                if (breath.TremorYRandom != null) { breath.TremorYRandom.Amplitude = _tremorY; breath.TremorYRandom.Hardness = _tremorHardY; }
                if (breath.TremorZRandom != null) { breath.TremorZRandom.Amplitude = _tremorZ; breath.TremorZRandom.Hardness = _tremorHardZ; }
            }

            var motion = pwa.MotionReact;
            if (motion != null)
            {
                motion.Intensity          = _motionIntensity;
                motion.SwayFactors        = _motionSway;
                motion.RotationInputClamp = _motionClamp;
            }

            var walk = pwa.Walk;
            if (walk != null) { walk.Intensity = _walkIntensity; walk.StepFrequency = _walkStep; }

            var shake = pwa.HandShakeEffector;
            if (shake != null) shake._shakeIntensity = _handShake;

            var force = pwa.ForceReact;
            if (force != null) force.Intensity = _forceIntensity;

            pwa.AimSwayMin  = _aimSwayMin;
            pwa.AimSwayMax  = _aimSwayMax;
            pwa.SwayFalloff = _swayFalloff;
        }

        private void ApplySprings(ProceduralWeaponAnimation pwa)
        {
            var hc = pwa.HandsContainer;
            if (hc == null) return;

            if (hc.HandsRotation != null)
            {
                hc.HandsRotation.Damping         = SwayConfig.HandsRotDamping.Value;
                hc.HandsRotation.ReturnSpeed     = SwayConfig.HandsRotReturnSpeed.Value;
                hc.HandsRotation.AccelerationMax = SwayConfig.HandsRotAccelerationMax.Value;
                hc.HandsRotation.InputIntensity  = SwayConfig.HandsRotInputIntensity.Value;
                hc.HandsRotation.Softness        = SwayConfig.HandsRotSoftness.Value;
            }
            if (hc.HandsPosition != null)
            {
                hc.HandsPosition.Damping     = SwayConfig.HandsPosDamping.Value;
                hc.HandsPosition.ReturnSpeed = SwayConfig.HandsPosReturnSpeed.Value;
            }
            if (hc.CameraRotation != null)
            {
                hc.CameraRotation.Damping     = SwayConfig.CameraRotDamping.Value;
                hc.CameraRotation.ReturnSpeed = SwayConfig.CameraRotReturnSpeed.Value;
            }
        }

        private void RestoreSprings(ProceduralWeaponAnimation pwa)
        {
            var hc = pwa.HandsContainer;
            if (hc == null) return;

            if (hc.HandsRotation != null)
            {
                hc.HandsRotation.Damping         = _handsRotDamping;
                hc.HandsRotation.ReturnSpeed     = _handsRotReturn;
                hc.HandsRotation.AccelerationMax = _handsRotAccelMax;
                hc.HandsRotation.InputIntensity  = _handsRotInput;
                hc.HandsRotation.Softness        = _handsRotSoftness;
            }
            if (hc.HandsPosition != null)
            {
                hc.HandsPosition.Damping     = _handsPosDamping;
                hc.HandsPosition.ReturnSpeed = _handsPosReturn;
            }
            if (hc.CameraRotation != null)
            {
                hc.CameraRotation.Damping     = _camRotDamping;
                hc.CameraRotation.ReturnSpeed = _camRotReturn;
            }
        }
    }
}

using UnityEngine;

namespace SptSway.Util
{
    /// <summary>
    /// One-pole low-pass, frame-rate independent.
    ///
    /// The coefficient is derived from the real elapsed time rather than being a
    /// fixed lerp factor, so the same cutoff behaves identically at 60 and at
    /// 240 fps. A plain <c>Lerp(a, b, 0.2f)</c> would silently become a
    /// different filter every time the frame rate moved.
    /// </summary>
    internal struct OnePole
    {
        private Vector3 _state;
        private bool _primed;

        /// <summary>Filters <paramref name="value"/> at <paramref name="cutoffHz"/>. A cutoff of zero or less passes the signal through untouched.</summary>
        public Vector3 Filter(Vector3 value, float dt, float cutoffHz)
        {
            if (cutoffHz <= 0f) { _state = value; _primed = true; return value; }
            if (!_primed) { _state = value; _primed = true; return value; }

            float a = 1f - Mathf.Exp(-2f * Mathf.PI * cutoffHz * dt);
            _state += (value - _state) * Mathf.Clamp01(a);
            return _state;
        }

        public void Reset() { _state = Vector3.zero; _primed = false; }
    }

    /// <summary>Scalar form of <see cref="OnePole"/>.</summary>
    internal struct OnePole1
    {
        private float _state;
        private bool _primed;

        public float Filter(float value, float dt, float cutoffHz)
        {
            if (cutoffHz <= 0f) { _state = value; _primed = true; return value; }
            if (!_primed) { _state = value; _primed = true; return value; }

            float a = 1f - Mathf.Exp(-2f * Mathf.PI * cutoffHz * dt);
            _state += (value - _state) * Mathf.Clamp01(a);
            return _state;
        }

        public void Reset() { _state = 0f; _primed = false; }
    }
}

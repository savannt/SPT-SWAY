using UnityEngine;

namespace SptSway.Util
{
    /// <summary>
    /// Band-limited 1-D value noise.
    ///
    /// Unity's PerlinNoise sampled along a line is cheap but has a visible
    /// period and a habit of parking near zero, which reads as a stutter rather
    /// than a tremor. This is a hash-based value noise with a smootherstep
    /// interpolant instead: continuous in the first and second derivative, so a
    /// stack of these layers looks like flesh rather than like a sawtooth.
    /// </summary>
    public static class SwayNoise
    {
        /// <summary>Signed noise in roughly [-1, 1] at position <paramref name="x"/> on stream <paramref name="seed"/>.</summary>
        public static float Signed(float x, int seed)
        {
            int i = Mathf.FloorToInt(x);
            float f = x - i;
            float t = Smootherstep(f);
            float a = Hash(i, seed);
            float b = Hash(i + 1, seed);
            return Mathf.Lerp(a, b, t);
        }

        /// <summary>
        /// Several detuned octaves summed and normalised. <paramref name="spread"/>
        /// pushes each octave off an exact frequency multiple so the layers never
        /// realign into an audible-looking beat.
        /// </summary>
        public static float Fractal(float x, int seed, int octaves, float spread)
        {
            float sum = 0f;
            float norm = 0f;
            float amp = 1f;
            float freq = 1f;

            for (int o = 0; o < octaves; o++)
            {
                // 1.87 rather than 2.0: an irrational-ish ratio keeps the octaves
                // from ever sharing a period.
                float detune = 1f + spread * (Hash(o * 71 + 13, seed) * 0.5f);
                sum += Signed(x * freq * detune, seed + o * 977) * amp;
                norm += amp;
                amp *= 0.55f;
                freq *= 1.87f;
            }

            return norm > 0f ? sum / norm : 0f;
        }

        private static float Smootherstep(float t)
        {
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        private static float Hash(int i, int seed)
        {
            unchecked
            {
                uint h = (uint)(i * 374761393 + seed * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h / (float)uint.MaxValue) * 2f - 1f;
            }
        }
    }
}

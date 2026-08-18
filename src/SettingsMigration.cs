using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace SptSway
{
    /// <summary>
    /// Brings an existing config file up to date when the shipped defaults are
    /// retuned.
    ///
    /// BepInEx only writes a default into a .cfg the first time it sees the key,
    /// so retuning a default in code does nothing for anyone who has already run
    /// the mod once. That is normally correct — it is what stops an update from
    /// wiping your settings — but it also means a tuning fix would never reach
    /// the people who need it.
    ///
    /// The compromise: a retuned entry is rewritten only if it still holds the
    /// value the previous version shipped. Anything you changed yourself is
    /// left exactly as you left it, because it no longer matches the old default.
    /// </summary>
    internal static class SettingsMigration
    {
        /// <summary>Bump this whenever a batch of defaults is retuned, and add the old values to the table.</summary>
        public const int CurrentRevision = 1;

        public static void Run(ManualLogSource log)
        {
            int from = SwayConfig.Revision.Value;
            if (from >= CurrentRevision)
                return;

            int changed = 0;

            if (from < 1)
                changed += ApplyRevision1();

            SwayConfig.Revision.Value = CurrentRevision;

            if (changed > 0)
                log.LogInfo("[SPT-SWAY] retuned " + changed + " setting(s) still at their old defaults " +
                            "(config revision " + from + " -> " + CurrentRevision + "). " +
                            "Anything you had changed yourself was left alone.");
        }

        /// <summary>
        /// Revision 1: the first release sent far too much of every source into
        /// the camera, so the view wandered on its own and the tremor read as a
        /// buzz rather than as a hand. Camera coupling comes down hard, the fast
        /// sources come down with it, and the pulse spike is blunted.
        /// </summary>
        private static int ApplyRevision1()
        {
            int n = 0;

            n += Retune(SwayConfig.CameraCoupling,     0.35f);
            n += Retune(SwayConfig.CameraRespShare,    1f);
            n += Retune(SwayConfig.CameraHeartShare,   0.8f);
            n += Retune(SwayConfig.CameraTremorShare,  0.35f);
            n += Retune(SwayConfig.CameraDriftShare,   1f);
            n += Retune(SwayConfig.CameraInertiaShare, 0.25f);

            n += Retune(SwayConfig.TremorAmplitude, 1f);
            n += Retune(SwayConfig.TremorAds,       1.3f);
            n += Retune(SwayConfig.TremorOctaves,   3);

            n += Retune(SwayConfig.HeartAmplitude,         1f);
            n += Retune(SwayConfig.HeartSystolicSharpness, 7f);

            n += Retune(SwayConfig.DriftAmplitude, 1f);

            return n;
        }

        private static int Retune(ConfigEntry<float> entry, float previousDefault)
        {
            if (entry == null) return 0;
            if (!Mathf.Approximately(entry.Value, previousDefault)) return 0;

            entry.Value = (float)entry.DefaultValue;
            return 1;
        }

        private static int Retune(ConfigEntry<int> entry, int previousDefault)
        {
            if (entry == null) return 0;
            if (entry.Value != previousDefault) return 0;

            entry.Value = (int)entry.DefaultValue;
            return 1;
        }
    }
}

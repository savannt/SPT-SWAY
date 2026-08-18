using System.Text;
using BepInEx;
using BepInEx.Logging;
using SptSway.Patches;
using SptSway.Runtime;
using UnityEngine;

namespace SptSway
{
    [BepInPlugin(Guid, "SPT-SWAY", Version)]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid    = "com.savannt.sptsway";
        public const string Version = "2.0.0";

        internal static ManualLogSource Log;

        private bool _overlay;
        private GUIStyle _overlayStyle;
        private readonly StringBuilder _sb = new StringBuilder(1024);

        private void Awake()
        {
            Log = Logger;

            SwayConfig.Bind(Config);
            SettingsMigration.Run(Log);

            // Ticking the preset box in the F12 menu is the only way to apply
            // one, so it acts as a button and unticks itself.
            SwayConfig.ApplyPreset.SettingChanged += (_, __) =>
            {
                if (!SwayConfig.ApplyPreset.Value) return;
                Presets.Apply(SwayConfig.Preset.Value);
                SwayConfig.ApplyPreset.Value = false;
                Log.LogInfo("[SPT-SWAY] applied preset: " + SwayConfig.Preset.Value);
            };

            new SwayTickPatch().Enable();
            new PhysicalConditionPatch().Enable();
            new WeaponChangedPatch().Enable();
            new ShotPatch().Enable();

            Log.LogInfo("[SPT-SWAY] " + Version + " loaded. " +
                        SwayConfig.ToggleKey.Value + " toggles, " +
                        SwayConfig.DebugKey.Value + " shows the readout.");
        }

        private void Update()
        {
            if (SwayConfig.ToggleKey.Value.IsDown())
            {
                SwayConfig.Enabled.Value = !SwayConfig.Enabled.Value;
                Log.LogInfo("[SPT-SWAY] " + (SwayConfig.Enabled.Value ? "on" : "off"));
            }

            if (SwayConfig.DebugKey.Value.IsDown())
                SwayConfig.DebugOverlay.Value = !SwayConfig.DebugOverlay.Value;

            _overlay = SwayConfig.DebugOverlay.Value;
        }

        private void OnGUI()
        {
            if (!_overlay) return;

            if (_overlayStyle == null)
            {
                _overlayStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.UpperLeft,
                    richText = false,
                };
                _overlayStyle.normal.textColor = Color.white;
            }

            var d = SwayDirector.Instance;
            var s = d.State;

            _sb.Length = 0;
            _sb.AppendLine("SPT-SWAY " + Version + (SwayConfig.Enabled.Value ? "" : "  [DISABLED]"));
            _sb.AppendLine("--------------------------------");
            _sb.AppendLine(Row("aiming",    s.IsAiming.ToString()));
            _sb.AppendLine(Row("stance",    s.IsBipod ? "bipod" : s.IsMounted ? "mounted" : s.IsProne ? "prone" : "pose " + s.PoseLevel.ToString("F2")));
            _sb.AppendLine(Row("support",   s.SupportFactor.ToString("F3")));
            _sb.AppendLine(Row("handling",  s.HandlingFactor.ToString("F3")));
            _sb.AppendLine(Row("ergo/kg",   s.Ergonomics.ToString("F0") + " / " + s.WeightKg.ToString("F2")));
            _sb.AppendLine(Row("stamina",   s.Stamina.ToString("F2") + "  oxy " + s.Oxygen.ToString("F2") + "  arms " + s.ArmStamina.ToString("F2")));
            _sb.AppendLine(Row("exertion",  s.Exertion.ToString("F3") + (s.Exhausted ? "  EXHAUSTED" : "")));
            _sb.AppendLine(Row("breath",    d.Breath.RateBpm.ToString("F1") + " bpm  phase " + d.Breath.Phase.ToString("F2") + (s.HoldingBreath ? "  HELD" : "")));
            _sb.AppendLine(Row("pulse",     d.Heart.Bpm.ToString("F0") + " bpm"));
            _sb.AppendLine(Row("injury",    (s.ArmFractured ? "fracture " : "") + (s.ArmBlacked ? "blacked " : "") + (s.NeuroTremor ? "tremor" : "")));
            _sb.AppendLine(Row("multiplier", d.LastTotalMultiplier.ToString("F3")));
            _sb.AppendLine(Row("hands",     Fmt(d.LastHands)));
            _sb.AppendLine(Row("camera",    Fmt(d.LastCamera)));

            GUI.Label(new Rect(12f, 12f, 460f, 400f), _sb.ToString(), _overlayStyle);
        }

        private static string Row(string label, string value)
        {
            return label.PadRight(12) + value;
        }

        private static string Fmt(Vector3 v)
        {
            return "p " + v.x.ToString("F3") + "  y " + v.y.ToString("F3") + "  r " + v.z.ToString("F3");
        }
    }
}

using System;
using UnityEngine;

namespace MainGameAdvIkBridge
{
    [Serializable]
    internal sealed class PluginSettings
    {
        public bool EnableBridge = true;
        public bool ApplyInHSceneOnly = false;
        public float ScanIntervalSeconds = 0.5f;

        public bool ShoulderRotationEnabled = true;
        public bool ReverseShoulderLeft = false;
        public bool ReverseShoulderRight = false;
        public bool EnableSpineFKHints = true;
        public bool EnableShoulderFKHints = false;
        public bool EnableToeFKHints = false;
        public bool IndependentShoulders = false;
        public float ShoulderWeight = 1.5f;
        public float ShoulderRightWeight = 1.5f;
        public float ShoulderOffset = 0.2f;
        public float ShoulderRightOffset = 0.2f;
        public float SpineStiffness = 0.0f;

        public bool ForceMainGameBreathingConfig = false;
        public bool MainGameBreathing = false;
        public float MainGameBreathScale = 1.0f;
        public float MainGameBreathRateScale = 1.0f;

        public bool EnableLogs = false;
        public bool VerboseLogs = false;
        public bool ResetLogOnStart = true;

        public KeyCode RebindHotkey = KeyCode.F8;
        public KeyCode ReloadSettingsHotkey = KeyCode.F9;
    }
}

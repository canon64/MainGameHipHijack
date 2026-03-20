using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace MainGameAdvIkBridge
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInProcess("KoikatsuSunshine")]
    [BepInProcess("KoikatsuSunshine_VR")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.kks.main.advikbridge";
        public const string PluginName = "MainGameAdvIkBridge";
        public const string Version = "0.1.1";

        private static readonly string[] AssemblyNameCandidates =
        {
            "KKS_AdvIKPlugin",
            "AdvIKPlugin"
        };

        private static readonly string[] ControllerPropertyNames =
        {
            "ShoulderRotationEnabled",
            "ReverseShoulderL",
            "ReverseShoulderR",
            "EnableSpineFKHints",
            "EnableShoulderFKHints",
            "EnableToeFKHints",
            "IndependentShoulders",
            "ShoulderWeight",
            "ShoulderRightWeight",
            "ShoulderOffset",
            "ShoulderRightOffset",
            "SpineStiffness"
        };

        private SimpleFileLogger _fileLogger;

        private Assembly _advAssembly;
        private Type _advControllerType;
        private Type _advPluginType;
        private readonly Dictionary<string, PropertyInfo> _controllerProperties = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);

        private PropertyInfo _mainGameBreathingProperty;
        private PropertyInfo _mainGameBreathScaleProperty;
        private PropertyInfo _mainGameBreathRateScaleProperty;

        private readonly HashSet<string> _warnedKeys = new HashSet<string>(StringComparer.Ordinal);

        private ConfigEntry<bool> _cfgEnableBridge;
        private ConfigEntry<bool> _cfgApplyInHSceneOnly;
        private ConfigEntry<float> _cfgScanIntervalSeconds;
        private ConfigEntry<bool> _cfgUseShoulderStabilizerSettings;
        private ConfigEntry<string> _cfgShoulderStabilizerSettingsPath;

        private ConfigEntry<bool> _cfgShoulderRotationEnabled;
        private ConfigEntry<bool> _cfgReverseShoulderLeft;
        private ConfigEntry<bool> _cfgReverseShoulderRight;
        private ConfigEntry<bool> _cfgEnableSpineFKHints;
        private ConfigEntry<bool> _cfgEnableShoulderFKHints;
        private ConfigEntry<bool> _cfgEnableToeFKHints;
        private ConfigEntry<bool> _cfgIndependentShoulders;
        private ConfigEntry<float> _cfgShoulderWeight;
        private ConfigEntry<float> _cfgShoulderRightWeight;
        private ConfigEntry<float> _cfgShoulderOffset;
        private ConfigEntry<float> _cfgShoulderRightOffset;
        private ConfigEntry<float> _cfgSpineStiffness;

        private ConfigEntry<bool> _cfgForceMainGameBreathingConfig;
        private ConfigEntry<bool> _cfgMainGameBreathing;
        private ConfigEntry<float> _cfgMainGameBreathScale;
        private ConfigEntry<float> _cfgMainGameBreathRateScale;

        private ConfigEntry<bool> _cfgEnableLogs;
        private ConfigEntry<bool> _cfgVerboseLogs;
        private ConfigEntry<bool> _cfgResetLogOnStart;

        private ShoulderStabilizerSettings _shoulderSettingsCache;
        private DateTime _shoulderSettingsLastWriteUtc;
        private string _shoulderSettingsLoadedPath;

        private float _nextScanTime;
        private float _nextBindRetryTime;
        private bool _bridgeBound;
        private bool _loggedBridgeReady;

        private void Awake()
        {
            string pluginDir = Path.GetDirectoryName(Info.Location) ?? Directory.GetCurrentDirectory();
            string canonPluginsDir = Directory.GetParent(pluginDir)?.FullName ?? pluginDir;
            string shoulderSettingsDefaultPath = Path.Combine(
                canonPluginsDir,
                "MainGirlShoulderIkStabilizer",
                "ShoulderIkStabilizerSettings.json");

            _cfgEnableBridge = Config.Bind("General", "EnableBridge", true, "Enable AdvIK bridge apply loop.\nAdvIK 連携の適用ループを有効化します。");
            _cfgApplyInHSceneOnly = Config.Bind("General", "ApplyInHSceneOnly", false, "Apply only while HSceneProc exists.\nHシーン中のみ適用します。");
            _cfgScanIntervalSeconds = Config.Bind(
                "General",
                "ScanIntervalSeconds",
                0.5f,
                new ConfigDescription("Apply interval in seconds.\n適用間隔（秒）です。", new AcceptableValueRange<float>(0.1f, 5f)));
            _cfgUseShoulderStabilizerSettings = Config.Bind("Input", "UseShoulderStabilizerSettings", true, "Use values from ShoulderIkStabilizerSettings.json when available.\n有効時は ShoulderIkStabilizerSettings.json の値を優先して使用します。");
            _cfgShoulderStabilizerSettingsPath = Config.Bind("Input", "ShoulderStabilizerSettingsPath", shoulderSettingsDefaultPath, "Path to ShoulderIkStabilizerSettings.json.\nShoulderIkStabilizerSettings.json のパスです。");

            _cfgShoulderRotationEnabled = Config.Bind("Shoulder", "ShoulderRotationEnabled", true, "AdvIK ShoulderRotationEnabled.\n肩回転補正を有効化します。");
            _cfgReverseShoulderLeft = Config.Bind("Shoulder", "ReverseShoulderLeft", false, "AdvIK ReverseShoulderL.\n左肩の到達方向補正を反転します。");
            _cfgReverseShoulderRight = Config.Bind("Shoulder", "ReverseShoulderRight", false, "AdvIK ReverseShoulderR.\n右肩の到達方向補正を反転します。");
            _cfgEnableSpineFKHints = Config.Bind("Shoulder", "EnableSpineFKHints", true, "AdvIK EnableSpineFKHints.\nSpine FK ヒントを有効化します。");
            _cfgEnableShoulderFKHints = Config.Bind("Shoulder", "EnableShoulderFKHints", false, "AdvIK EnableShoulderFKHints.\nShoulder FK ヒントを有効化します。");
            _cfgEnableToeFKHints = Config.Bind("Shoulder", "EnableToeFKHints", false, "AdvIK EnableToeFKHints.\nToe FK ヒントを有効化します。");
            _cfgIndependentShoulders = Config.Bind("Shoulder", "IndependentShoulders", false, "AdvIK IndependentShoulders.\n左右肩を独立パラメータで制御します。");
            _cfgShoulderWeight = Config.Bind(
                "Shoulder",
                "ShoulderWeight",
                1.5f,
                new ConfigDescription("AdvIK ShoulderWeight.\n左肩ウェイトです。", new AcceptableValueRange<float>(0f, 5f)));
            _cfgShoulderRightWeight = Config.Bind(
                "Shoulder",
                "ShoulderRightWeight",
                1.5f,
                new ConfigDescription("AdvIK ShoulderRightWeight.\n右肩ウェイトです。", new AcceptableValueRange<float>(0f, 5f)));
            _cfgShoulderOffset = Config.Bind(
                "Shoulder",
                "ShoulderOffset",
                0.2f,
                new ConfigDescription("AdvIK ShoulderOffset.\n左肩オフセットです。", new AcceptableValueRange<float>(-1f, 1f)));
            _cfgShoulderRightOffset = Config.Bind(
                "Shoulder",
                "ShoulderRightOffset",
                0.2f,
                new ConfigDescription("AdvIK ShoulderRightOffset.\n右肩オフセットです。", new AcceptableValueRange<float>(-1f, 1f)));
            _cfgSpineStiffness = Config.Bind(
                "Shoulder",
                "SpineStiffness",
                0.0f,
                new ConfigDescription("AdvIK SpineStiffness.\n背骨の剛性です（高いほど曲がりにくい）。", new AcceptableValueRange<float>(0f, 1f)));

            _cfgForceMainGameBreathingConfig = Config.Bind("Breathing", "ForceMainGameBreathingConfig", false, "Also force AdvIK static main-game breathing config entries.\nAdvIK の本編呼吸設定も強制上書きします。");
            _cfgMainGameBreathing = Config.Bind("Breathing", "MainGameBreathing", false, "AdvIK MainGameBreathing.Value.\n本編呼吸を有効化します。");
            _cfgMainGameBreathScale = Config.Bind(
                "Breathing",
                "MainGameBreathScale",
                1.0f,
                new ConfigDescription("AdvIK MainGameBreathScale.Value.\n呼吸の大きさ倍率です。", new AcceptableValueRange<float>(0.25f, 3f)));
            _cfgMainGameBreathRateScale = Config.Bind(
                "Breathing",
                "MainGameBreathRateScale",
                1.0f,
                new ConfigDescription("AdvIK MainGameBreathRateScale.Value.\n呼吸速度倍率です。", new AcceptableValueRange<float>(0.25f, 3f)));

            _cfgEnableLogs = Config.Bind("Logging", "EnableLogs", false, "Write plugin log file.\nログ出力を有効化します（OFF時は出力しません）。");
            _cfgVerboseLogs = Config.Bind("Logging", "VerboseLogs", false, "Verbose bridge logs.\n詳細ログを有効化します。");
            _cfgResetLogOnStart = Config.Bind("Logging", "ResetLogOnStart", true, "Clear log file on startup.\n起動時に専用ログをリセットします。");

            string logPath = Path.Combine(pluginDir, "MainGameAdvIkBridge.log");
            _fileLogger = new SimpleFileLogger(logPath, _cfgResetLogOnStart.Value);

            LogInfo("start version=" + Version + " cfg=" + Config.ConfigFilePath);
            TryBindAdvIk("awake");
        }

        private void Update()
        {
            if (!_cfgEnableBridge.Value)
                return;

            if (!_bridgeBound && Time.unscaledTime >= _nextBindRetryTime)
            {
                TryBindAdvIk("retry");
            }
            if (!_bridgeBound)
                return;

            if (_cfgApplyInHSceneOnly.Value && FindObjectOfType<HSceneProc>() == null)
                return;

            if (Time.unscaledTime < _nextScanTime)
                return;

            _nextScanTime = Time.unscaledTime + Mathf.Clamp(_cfgScanIntervalSeconds.Value, 0.1f, 5f);
            ApplyBridgeSettings();
        }

        private void TryBindAdvIk(string reason)
        {
            _bridgeBound = false;
            _loggedBridgeReady = false;
            _advAssembly = null;
            _advControllerType = null;
            _advPluginType = null;
            _controllerProperties.Clear();
            _mainGameBreathingProperty = null;
            _mainGameBreathScaleProperty = null;
            _mainGameBreathRateScaleProperty = null;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < AssemblyNameCandidates.Length && _advAssembly == null; i++)
            {
                string candidate = AssemblyNameCandidates[i];
                for (int j = 0; j < assemblies.Length; j++)
                {
                    Assembly asm = assemblies[j];
                    string asmName = asm.GetName().Name;
                    if (string.Equals(asmName, candidate, StringComparison.OrdinalIgnoreCase))
                    {
                        _advAssembly = asm;
                        break;
                    }
                }
            }

            if (_advAssembly == null)
            {
                for (int i = 0; i < assemblies.Length; i++)
                {
                    Assembly asm = assemblies[i];
                    Type controllerType = asm.GetType("AdvIKPlugin.AdvIKCharaController", false);
                    if (controllerType != null)
                    {
                        _advAssembly = asm;
                        break;
                    }
                }
            }

            if (_advAssembly == null)
            {
                _nextBindRetryTime = Time.unscaledTime + Mathf.Max(1f, Mathf.Clamp(_cfgScanIntervalSeconds.Value, 0.1f, 5f));
                LogVerbose("bind skipped reason=" + reason + " detail=assembly-not-loaded");
                return;
            }

            _advControllerType = _advAssembly.GetType("AdvIKPlugin.AdvIKCharaController", false);
            _advPluginType = _advAssembly.GetType("AdvIKPlugin.AdvIKPlugin", false);

            if (_advControllerType == null)
            {
                _nextBindRetryTime = Time.unscaledTime + Mathf.Max(1f, Mathf.Clamp(_cfgScanIntervalSeconds.Value, 0.1f, 5f));
                LogWarnOnce("bind-controller-missing", "bind failed reason=" + reason + " detail=AdvIKCharaController-not-found");
                return;
            }

            for (int i = 0; i < ControllerPropertyNames.Length; i++)
            {
                string propertyName = ControllerPropertyNames[i];
                PropertyInfo property = _advControllerType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                    _controllerProperties[propertyName] = property;
                else
                    LogWarnOnce("controller-prop-" + propertyName, "controller property unavailable: " + propertyName);
            }

            if (_advPluginType != null)
            {
                _mainGameBreathingProperty = _advPluginType.GetProperty("MainGameBreathing", BindingFlags.Public | BindingFlags.Static);
                _mainGameBreathScaleProperty = _advPluginType.GetProperty("MainGameBreathScale", BindingFlags.Public | BindingFlags.Static);
                _mainGameBreathRateScaleProperty = _advPluginType.GetProperty("MainGameBreathRateScale", BindingFlags.Public | BindingFlags.Static);
            }

            _bridgeBound = true;
            LogInfo("bind ok reason=" + reason + " asm=" + _advAssembly.GetName().Name + " controllerType=" + _advControllerType.FullName);
        }

        private void ApplyBridgeSettings()
        {
            EffectiveSettings effective = BuildEffectiveSettings();
            if (effective == null)
                return;

            ChaControl[] characters = FindObjectsOfType<ChaControl>();
            if (characters == null || characters.Length == 0)
            {
                LogVerbose("apply skipped detail=no-cha-control");
                return;
            }

            int foundControllers = 0;
            int changedValues = 0;

            for (int i = 0; i < characters.Length; i++)
            {
                ChaControl cha = characters[i];
                if (cha == null || !cha.gameObject)
                    continue;

                Component controller = cha.gameObject.GetComponent(_advControllerType);
                if (controller == null)
                    continue;

                foundControllers++;
                changedValues += ApplyControllerValues(controller, effective);
            }

            if (effective.ForceMainGameBreathingConfig)
            {
                if (ApplyStaticConfigEntryValue(_mainGameBreathingProperty, effective.MainGameBreathing))
                    changedValues++;
                if (ApplyStaticConfigEntryValue(_mainGameBreathScaleProperty, effective.MainGameBreathScale))
                    changedValues++;
                if (ApplyStaticConfigEntryValue(_mainGameBreathRateScaleProperty, effective.MainGameBreathRateScale))
                    changedValues++;
            }

            if (!_loggedBridgeReady)
            {
                _loggedBridgeReady = true;
                LogInfo("apply ready controllers=" + foundControllers + " characters=" + characters.Length);
            }
            else
            {
                LogVerbose("apply tick controllers=" + foundControllers + " characters=" + characters.Length + " changed=" + changedValues);
            }
        }

        private int ApplyControllerValues(Component controller, EffectiveSettings effective)
        {
            int changed = 0;

            if (ApplyControllerBool(controller, "ShoulderRotationEnabled", effective.ShoulderRotationEnabled))
                changed++;
            if (ApplyControllerBool(controller, "ReverseShoulderL", effective.ReverseShoulderLeft))
                changed++;
            if (ApplyControllerBool(controller, "ReverseShoulderR", effective.ReverseShoulderRight))
                changed++;
            if (ApplyControllerBool(controller, "EnableSpineFKHints", effective.EnableSpineFKHints))
                changed++;
            if (ApplyControllerBool(controller, "EnableShoulderFKHints", effective.EnableShoulderFKHints))
                changed++;
            if (ApplyControllerBool(controller, "EnableToeFKHints", effective.EnableToeFKHints))
                changed++;
            if (ApplyControllerBool(controller, "IndependentShoulders", effective.IndependentShoulders))
                changed++;

            if (ApplyControllerFloat(controller, "ShoulderWeight", effective.ShoulderWeight))
                changed++;
            if (ApplyControllerFloat(controller, "ShoulderRightWeight", effective.ShoulderRightWeight))
                changed++;
            if (ApplyControllerFloat(controller, "ShoulderOffset", effective.ShoulderOffset))
                changed++;
            if (ApplyControllerFloat(controller, "ShoulderRightOffset", effective.ShoulderRightOffset))
                changed++;
            if (ApplyControllerFloat(controller, "SpineStiffness", effective.SpineStiffness))
                changed++;

            return changed;
        }

        private bool ApplyControllerBool(Component controller, string propertyName, bool targetValue)
        {
            PropertyInfo property;
            if (!_controllerProperties.TryGetValue(propertyName, out property))
                return false;

            try
            {
                object currentRaw = property.GetValue(controller, null);
                bool current = currentRaw is bool b && b;
                if (current == targetValue)
                    return false;

                property.SetValue(controller, targetValue, null);
                return true;
            }
            catch (Exception ex)
            {
                LogWarnOnce("apply-bool-" + propertyName, "failed bool apply " + propertyName + " error=" + ex.Message);
                return false;
            }
        }

        private bool ApplyControllerFloat(Component controller, string propertyName, float targetValue)
        {
            PropertyInfo property;
            if (!_controllerProperties.TryGetValue(propertyName, out property))
                return false;

            try
            {
                object currentRaw = property.GetValue(controller, null);
                float current = Convert.ToSingle(currentRaw, CultureInfo.InvariantCulture);
                if (Mathf.Abs(current - targetValue) <= 0.0001f)
                    return false;

                property.SetValue(controller, targetValue, null);
                return true;
            }
            catch (Exception ex)
            {
                LogWarnOnce("apply-float-" + propertyName, "failed float apply " + propertyName + " error=" + ex.Message);
                return false;
            }
        }

        private bool ApplyStaticConfigEntryValue(PropertyInfo entryProperty, object targetValue)
        {
            if (entryProperty == null)
                return false;

            try
            {
                object entry = entryProperty.GetValue(null, null);
                if (entry == null)
                    return false;

                PropertyInfo valueProperty = entry.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                if (valueProperty == null || !valueProperty.CanRead || !valueProperty.CanWrite)
                    return false;

                object current = valueProperty.GetValue(entry, null);
                if (Equals(current, targetValue))
                    return false;

                valueProperty.SetValue(entry, targetValue, null);
                return true;
            }
            catch (Exception ex)
            {
                LogWarnOnce("static-config-" + entryProperty.Name, "failed static config apply " + entryProperty.Name + " error=" + ex.Message);
                return false;
            }
        }

        private void LogInfo(string message)
        {
            if (!IsLoggingEnabled())
                return;

            Logger.LogInfo("[" + PluginName + "] " + message);
            _fileLogger?.Write("INFO", message);
        }

        private void LogVerbose(string message)
        {
            if (!IsLoggingEnabled())
                return;
            if (_cfgVerboseLogs == null || !_cfgVerboseLogs.Value)
                return;

            Logger.LogInfo("[" + PluginName + "] " + message);
            _fileLogger?.Write("DEBUG", message);
        }

        private void LogWarnOnce(string key, string message)
        {
            if (_warnedKeys.Contains(key))
                return;

            _warnedKeys.Add(key);
            if (!IsLoggingEnabled())
                return;

            Logger.LogWarning("[" + PluginName + "] " + message);
            _fileLogger?.Write("WARN", message);
        }

        private bool IsLoggingEnabled()
        {
            return _cfgEnableLogs != null && _cfgEnableLogs.Value;
        }

        private EffectiveSettings BuildEffectiveSettings()
        {
            var effective = new EffectiveSettings
            {
                ShoulderRotationEnabled = _cfgShoulderRotationEnabled.Value,
                ReverseShoulderLeft = _cfgReverseShoulderLeft.Value,
                ReverseShoulderRight = _cfgReverseShoulderRight.Value,
                EnableSpineFKHints = _cfgEnableSpineFKHints.Value,
                EnableShoulderFKHints = _cfgEnableShoulderFKHints.Value,
                EnableToeFKHints = _cfgEnableToeFKHints.Value,
                IndependentShoulders = _cfgIndependentShoulders.Value,
                ShoulderWeight = Mathf.Clamp(_cfgShoulderWeight.Value, 0f, 5f),
                ShoulderRightWeight = Mathf.Clamp(_cfgShoulderRightWeight.Value, 0f, 5f),
                ShoulderOffset = Mathf.Clamp(_cfgShoulderOffset.Value, -1f, 1f),
                ShoulderRightOffset = Mathf.Clamp(_cfgShoulderRightOffset.Value, -1f, 1f),
                SpineStiffness = Mathf.Clamp(_cfgSpineStiffness.Value, 0f, 1f),
                ForceMainGameBreathingConfig = _cfgForceMainGameBreathingConfig.Value,
                MainGameBreathing = _cfgMainGameBreathing.Value,
                MainGameBreathScale = Mathf.Clamp(_cfgMainGameBreathScale.Value, 0.25f, 3f),
                MainGameBreathRateScale = Mathf.Clamp(_cfgMainGameBreathRateScale.Value, 0.25f, 3f)
            };

            if (_cfgUseShoulderStabilizerSettings.Value)
            {
                ShoulderStabilizerSettings fromShoulder;
                if (TryLoadShoulderStabilizerSettings(_cfgShoulderStabilizerSettingsPath.Value, out fromShoulder))
                {
                    effective.ShoulderRotationEnabled = fromShoulder.ShoulderRotationEnabled;
                    effective.ReverseShoulderLeft = fromShoulder.ReverseShoulderL;
                    effective.ReverseShoulderRight = fromShoulder.ReverseShoulderR;
                    effective.IndependentShoulders = fromShoulder.IndependentShoulders;
                    effective.ShoulderWeight = Mathf.Clamp(fromShoulder.ShoulderWeight, 0f, 5f);
                    effective.ShoulderRightWeight = Mathf.Clamp(fromShoulder.ShoulderRightWeight, 0f, 5f);
                    effective.ShoulderOffset = Mathf.Clamp(fromShoulder.ShoulderOffset, -1f, 1f);
                    effective.ShoulderRightOffset = Mathf.Clamp(fromShoulder.ShoulderRightOffset, -1f, 1f);
                }
            }

            return effective;
        }

        private bool TryLoadShoulderStabilizerSettings(string path, out ShoulderStabilizerSettings settings)
        {
            settings = null;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                string fullPath = Path.GetFullPath(path);
                if (!File.Exists(fullPath))
                {
                    LogWarnOnce("shoulder-settings-missing", "shoulder settings not found: " + fullPath);
                    return false;
                }

                DateTime lastWriteUtc = File.GetLastWriteTimeUtc(fullPath);
                if (_shoulderSettingsCache != null
                    && string.Equals(_shoulderSettingsLoadedPath, fullPath, StringComparison.OrdinalIgnoreCase)
                    && _shoulderSettingsLastWriteUtc == lastWriteUtc)
                {
                    settings = _shoulderSettingsCache;
                    return true;
                }

                string json = File.ReadAllText(fullPath);
                ShoulderStabilizerSettings parsed = JsonUtility.FromJson<ShoulderStabilizerSettings>(json);
                if (parsed == null)
                {
                    LogWarnOnce("shoulder-settings-parse", "failed to parse shoulder settings: " + fullPath);
                    return false;
                }

                _shoulderSettingsLoadedPath = fullPath;
                _shoulderSettingsLastWriteUtc = lastWriteUtc;
                _shoulderSettingsCache = parsed;
                settings = parsed;
                LogVerbose("shoulder settings loaded: " + fullPath);
                return true;
            }
            catch (Exception ex)
            {
                LogWarnOnce("shoulder-settings-read", "failed to read shoulder settings: " + ex.Message);
                return false;
            }
        }

        [Serializable]
        private sealed class ShoulderStabilizerSettings
        {
            public bool ShoulderRotationEnabled = true;
            public bool IndependentShoulders;
            public bool ReverseShoulderL;
            public bool ReverseShoulderR;
            public float ShoulderWeight = 1.5f;
            public float ShoulderOffset = 0.2f;
            public float ShoulderRightWeight = 1.5f;
            public float ShoulderRightOffset = 0.2f;
        }

        private sealed class EffectiveSettings
        {
            public bool ShoulderRotationEnabled;
            public bool ReverseShoulderLeft;
            public bool ReverseShoulderRight;
            public bool EnableSpineFKHints;
            public bool EnableShoulderFKHints;
            public bool EnableToeFKHints;
            public bool IndependentShoulders;
            public float ShoulderWeight;
            public float ShoulderRightWeight;
            public float ShoulderOffset;
            public float ShoulderRightOffset;
            public float SpineStiffness;
            public bool ForceMainGameBreathingConfig;
            public bool MainGameBreathing;
            public float MainGameBreathScale;
            public float MainGameBreathRateScale;
        }
    }
}

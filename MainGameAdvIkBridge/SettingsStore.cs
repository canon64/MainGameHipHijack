using System;
using System.IO;
using UnityEngine;

namespace MainGameAdvIkBridge
{
    internal sealed class SettingsStore
    {
        private readonly string _path;

        public SettingsStore(string path)
        {
            _path = path;
        }

        public PluginSettings Load()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    var created = Normalize(new PluginSettings());
                    Save(created);
                    return created;
                }

                string json = File.ReadAllText(_path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    var created = Normalize(new PluginSettings());
                    Save(created);
                    return created;
                }

                PluginSettings loaded = JsonUtility.FromJson<PluginSettings>(json);
                if (loaded == null)
                {
                    var created = Normalize(new PluginSettings());
                    Save(created);
                    return created;
                }

                return Normalize(loaded);
            }
            catch
            {
                return Normalize(new PluginSettings());
            }
        }

        public void Save(PluginSettings settings)
        {
            PluginSettings normalized = Normalize(settings ?? new PluginSettings());
            string json = JsonUtility.ToJson(normalized, true);
            File.WriteAllText(_path, json);
        }

        private static PluginSettings Normalize(PluginSettings s)
        {
            if (s == null)
                s = new PluginSettings();

            s.ScanIntervalSeconds = Mathf.Clamp(s.ScanIntervalSeconds, 0.1f, 5f);

            s.ShoulderWeight = Mathf.Clamp(s.ShoulderWeight, 0f, 5f);
            s.ShoulderRightWeight = Mathf.Clamp(s.ShoulderRightWeight, 0f, 5f);
            s.ShoulderOffset = Mathf.Clamp(s.ShoulderOffset, 0f, 1f);
            s.ShoulderRightOffset = Mathf.Clamp(s.ShoulderRightOffset, 0f, 1f);
            s.SpineStiffness = Mathf.Clamp(s.SpineStiffness, 0f, 1f);

            s.MainGameBreathScale = Mathf.Clamp(s.MainGameBreathScale, 0.25f, 3f);
            s.MainGameBreathRateScale = Mathf.Clamp(s.MainGameBreathRateScale, 0.25f, 3f);

            return s;
        }
    }
}

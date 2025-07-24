using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace nurturing
{
    /// <summary>
    /// ゲーム共通設定（エキス数など）を
    /// SaveData/GameSettings.csv で管理します。
    /// </summary>
    public static class SettingsManager
    {
        private static readonly string SaveDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveData");
        private static readonly string SettingsFile =
            Path.Combine(SaveDir, "GameSettings.csv");
        private static Dictionary<string, string> _settings = new Dictionary<string, string>();

        static SettingsManager()
        {
            LoadSettings();
        }

        private static void LoadSettings()
        {
            if (!Directory.Exists(SaveDir))
                Directory.CreateDirectory(SaveDir);

            if (!File.Exists(SettingsFile))
            {
                // デフォルト設定
                _settings["ExtractCount"] = "100";
                SaveSettings();
            }
            else
            {
                foreach (var line in File.ReadAllLines(SettingsFile))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 2)
                        _settings[parts[0]] = parts[1];
                }
                // キー不足時の補完
                if (!_settings.ContainsKey("ExtractCount"))
                    _settings["ExtractCount"] = "10";
            }
        }

        private static void SaveSettings()
        {
            var lines = _settings
                .Select(kvp => $"{kvp.Key},{kvp.Value}")
                .ToArray();
            File.WriteAllLines(SettingsFile, lines);
        }

        /// <summary>
        /// 現在保持しているエキス数
        /// </summary>
        public static int ExtractCount
        {
            get
            {
                if (_settings.TryGetValue("ExtractCount", out var v)
                    && int.TryParse(v, out var i))
                    return i;
                return 10;
            }
            set
            {
                _settings["ExtractCount"] = value.ToString();
                SaveSettings();
            }
        }
    }
}

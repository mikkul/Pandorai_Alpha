using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Pandorai.Sounds;

namespace Pandorai
{
    static class Persistency
    {
        class Settings
        {
            public int MusicVolume { get; set; }
            public int SoundsVolume { get; set; }
        }

        public const string SettingsFilePath = "settings.json";

        public static void LoadSettings()
        {
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SettingsFilePath);
            if(!File.Exists(fullPath))
            {
                return;
            }

            var json = File.ReadAllText(fullPath);
            var settings = JsonConvert.DeserializeObject<Settings>(json);
            SoundManager.MusicVolume = settings.MusicVolume;
            SoundManager.SoundsVolume = settings.SoundsVolume;
        }

        public static void SaveSettings()
        {
            var settings = new Settings();
            settings.MusicVolume = SoundManager.MusicVolume;
            settings.SoundsVolume = SoundManager.SoundsVolume;

            var json = JsonConvert.SerializeObject(settings);
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SettingsFilePath);
            File.WriteAllText(fullPath, json);
        }
    }
}
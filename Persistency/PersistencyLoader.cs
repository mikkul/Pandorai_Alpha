using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Pandorai.Sounds;

namespace Pandorai.Persistency
{
    static class PersistencyLoader
    {
        public const string SettingsFilePath = "settings.json";
        public const string SavedGameFilePath = "savedGame.json";

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

        public static bool IsSavedGame()
        {
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SavedGameFilePath);
            return File.Exists(fullPath);
        }
        
        public static void SaveGame()
        {
            var gameState = new GameState();

            var json = JsonConvert.SerializeObject(gameState);
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SavedGameFilePath);
            File.WriteAllText(fullPath, json);
        }

        public static void LoadSavedGame()
        {
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SavedGameFilePath);
            if(!File.Exists(fullPath))
            {
                throw new FileNotFoundException("There is no saved game file");
            }

            var json = File.ReadAllText(fullPath);
            var gameState = JsonConvert.DeserializeObject<GameState>(json);
        }
    }
}
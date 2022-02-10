using System.Collections.Generic;
using System.IO;
using System.Timers;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Pandorai.Sounds
{
    public static class SoundManager
    {
        public static int SongTransitionMs = 1000;
        public static string CurrentSong { get; private set; }

        private static Dictionary<string, Song> _music = new();
        private static Dictionary<string, SoundEffect> _sounds = new();

        public static void LoadSounds(string musicFolderPath, string soundEffectsFolderPath)
        {
            var musicFiles = Directory.EnumerateFiles(Path.Combine(Game1.game.Content.RootDirectory, musicFolderPath));
            var soundFiles = Directory.EnumerateFiles(Path.Combine(Game1.game.Content.RootDirectory, soundEffectsFolderPath), "*.*", SearchOption.AllDirectories);

            foreach (var fullFilePath in musicFiles)
            {
                var filePath = Path.ChangeExtension(fullFilePath, null);
                filePath = filePath.Replace(Game1.game.Content.RootDirectory, null);
                if(filePath[0] == '/')
                {
                    filePath = filePath.Remove(0, 1);
                }
                var song = Game1.game.Content.Load<Song>(filePath);
                _music.Add(Path.GetFileName(filePath), song);
            }

            foreach (var fullFilePath in soundFiles)
            {
                var filePath = Path.ChangeExtension(fullFilePath, null);
                filePath = filePath.Replace(Game1.game.Content.RootDirectory, null);
                if(filePath[0] == '/')
                {
                    filePath = filePath.Remove(0, 1);
                }
                var soundEffect = Game1.game.Content.Load<SoundEffect>(filePath);
                _sounds.Add(Path.GetFileName(filePath), soundEffect);
            }    
        }
        
        public static void PlaySound(string name)
        {
            if(_sounds.ContainsKey(name))
            {
                _sounds[name].CreateInstance().Play();
            }
        }

        public static void PlayMusic(string name)
        {
            if(CurrentSong == name)
            {
                return;
            }

            if(_music.ContainsKey(name))
            {
                TransitionToSong(name);
                CurrentSong = name;
            }
        }

        private static void TransitionToSong(string name)
        {
            int timerIncrementStep = 10;
            float volumeStep = (float)timerIncrementStep / (float)SongTransitionMs;

            var fadeInTimer = new Timer(timerIncrementStep);
            double fadeInElapsedTime = 0;
            fadeInTimer.Elapsed += (s, a) =>
            {
                MediaPlayer.Volume += volumeStep;
                fadeInElapsedTime += fadeInTimer.Interval;
                if(fadeInElapsedTime > SongTransitionMs)
                {
                    fadeInTimer.Stop();
                    fadeInTimer.Dispose();
                }
            };

            var fadeOutTimer = new Timer(timerIncrementStep);
            double fadeOutElapsedTime = 0;
            fadeOutTimer.Elapsed += (s, a) =>
            {
                MediaPlayer.Volume -= volumeStep;
                fadeOutElapsedTime += fadeOutTimer.Interval;
                if(fadeOutElapsedTime > SongTransitionMs)
                {
                    try
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.Play(_music["Break_theme"]);
                        MediaPlayer.Play(_music[name]);
                    }
                    catch(System.Exception){}
                    
                    fadeInTimer.Start();
                    fadeOutTimer.Stop();
                    fadeOutTimer.Dispose();
                }
            };

            fadeOutTimer.Start();
        }
    }
}
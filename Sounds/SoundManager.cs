using System;
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

        private static Dictionary<string, SoundEffect> _music = new();
        private static Dictionary<string, SoundEffect> _sounds = new();

        private static SoundEffectInstance _currentSongInstance;

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
                var song = Game1.game.Content.Load<SoundEffect>(filePath);
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
        
        public static void PlaySound(string name, float volume = 1f)
        {
            if(_sounds.ContainsKey(name))
            {
                var soundInstance = _sounds[name].CreateInstance();
                soundInstance.Volume = volume;
                soundInstance.Play();
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
            var songInstance = _music[name].CreateInstance();
            songInstance.IsLooped = true;
            songInstance.Volume = 0;

            int timerIncrementStep = 10;
            float volumeStep = (float)timerIncrementStep / (float)SongTransitionMs;

            var fadeInTimer = new Timer(timerIncrementStep);
            double fadeInElapsedTime = 0;
            fadeInTimer.Elapsed += (s, a) =>
            {
                try
                {
                    songInstance.Volume += volumeStep;
                }
                catch(Exception){}
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
                if(_currentSongInstance != null)
                {
                    try
                    {
                        _currentSongInstance.Volume -= volumeStep;
                    }
                    catch(Exception){}
                }
                fadeOutElapsedTime += fadeOutTimer.Interval;
                if(fadeOutElapsedTime > SongTransitionMs)
                {
                    _currentSongInstance?.Stop();
                    songInstance.Play();
                    _currentSongInstance = songInstance;
                    
                    fadeInTimer.Start();
                    fadeOutTimer.Stop();
                    fadeOutTimer.Dispose();
                }
            };

            fadeOutTimer.Start();
        }
    }
}
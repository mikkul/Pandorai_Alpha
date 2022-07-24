using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Pandorai.Utility;

namespace Pandorai.Sounds
{
    public static class SoundManager
    {
        private static Dictionary<string, SoundEffect> _music = new();
        private static Dictionary<string, SoundEffect> _sounds = new();

        private static SoundEffectInstance _currentSongInstance;

        private static int SongTransitionMs = 1000;
        private static int musicVolume = 100;
        private static int soundsVolume = 100;

        public static string CurrentSong { get; private set; }
        public static int MusicVolume 
        { 
            get => musicVolume; 
            set
            {
                musicVolume = value;
                if(_currentSongInstance != null)
                {
                    _currentSongInstance.Volume = musicVolume / 100f;
                }
            }
        }
        public static int SoundsVolume 
        { 
            get => soundsVolume; 
            set => soundsVolume = value; 
        }

        public static void LoadSounds(string musicFolderPath, string soundEffectsFolderPath)
        {
            var musicFiles = Directory.EnumerateFiles(Path.Combine(Main.Game.Content.RootDirectory, musicFolderPath));
            var soundFiles = Directory.EnumerateFiles(Path.Combine(Main.Game.Content.RootDirectory, soundEffectsFolderPath), "*.*", SearchOption.AllDirectories);

            foreach (var fullFilePath in musicFiles)
            {
                var filePath = Path.ChangeExtension(fullFilePath, null);
                filePath = filePath.Replace(Main.Game.Content.RootDirectory, null);
                if (filePath.StartsWith("/") || filePath.StartsWith(@"\"))
                {
                    filePath = filePath.Remove(0, 1);
                }
                var song = Main.Game.Content.Load<SoundEffect>(filePath);
                _music.Add(Path.GetFileName(filePath), song);
            }

            foreach (var fullFilePath in soundFiles)
            {
                var filePath = Path.ChangeExtension(fullFilePath, null);
                filePath = filePath.Replace(Main.Game.Content.RootDirectory, null);
                if (filePath.StartsWith("/") || filePath.StartsWith(@"\"))
                {
                    filePath = filePath.Remove(0, 1);
                }
                var soundEffect = Main.Game.Content.Load<SoundEffect>(filePath);
                _sounds.Add(Path.GetFileName(filePath), soundEffect);
            }
        }

        public static void PlaySound(string name, Vector2 position, float volume = 1f)
        {
            bool isSoundInRange = Main.Game.Camera.Viewport.Displace(Main.Game.Camera.Position).Contains(position);
            if (_sounds.ContainsKey(name) && isSoundInRange)
            {
                var soundInstance = _sounds[name].CreateInstance();
                soundInstance.Volume = volume * (SoundsVolume / 100f);
                soundInstance.Play();
            }
        }

        public static void PlayMusic(string name)
        {
            if (CurrentSong == name)
            {
                return;
            }

            if (_music.ContainsKey(name))
            {
                TransitionToSong(name);
                CurrentSong = name;
            }
        }

        public static void StopMusic()
        {
            _currentSongInstance?.Stop();
        }

        private static void TransitionToSong(string name)
        {
            var songInstance = _music[name].CreateInstance();
            songInstance.IsLooped = true;
            songInstance.Volume = 0;

            int timerIncrementStep = 10;
            float volumeStep = (float)timerIncrementStep / (float)SongTransitionMs * (MusicVolume / 100f);

            var fadeInTimer = new Timer(timerIncrementStep);
            double fadeInElapsedTime = 0;
            fadeInTimer.Elapsed += (s, a) =>
            {
                try
                {
                    songInstance.Volume += volumeStep;
                }
                catch (Exception) { }
                fadeInElapsedTime += fadeInTimer.Interval;
                if (fadeInElapsedTime > SongTransitionMs)
                {
                    fadeInTimer.Stop();
                    fadeInTimer.Dispose();
                }
            };

            var fadeOutTimer = new Timer(timerIncrementStep);
            double fadeOutElapsedTime = 0;
            fadeOutTimer.Elapsed += (s, a) =>
            {
                if (_currentSongInstance != null)
                {
                    try
                    {
                        _currentSongInstance.Volume -= volumeStep;
                    }
                    catch (Exception) { }
                }
                fadeOutElapsedTime += fadeOutTimer.Interval;
                if (fadeOutElapsedTime > SongTransitionMs)
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
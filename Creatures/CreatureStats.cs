using System;
using Pandorai.ParticleSystems;
using Pandorai.UI;
using Pandorai.Utility;

namespace Pandorai.Creatures
{
    public class CreatureStats
    {
        private int _health;
        private int _mana;
        private int _level;
        private int _experience;
        private readonly Creature _owner;

        public CreatureStats(Creature owner)
        {
            _owner = owner;
        }

        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                SetLevel();
            }
        }

        public int Experience
        {
            get => _experience;
            set
            {
                var delta = value - _experience;
                _experience = value;
                SetExperience(delta);
            }
        }

        public int Health
        {
            get => _health;
            set => _health = Math.Min(_health + value, MaxHealth);
        }
        public int MaxHealth { get; set; }

        public int Mana
        {
            get => _mana;
            set => _mana = Math.Min(_mana + value, MaxMana);
        }
        public int MaxMana { get; set; }

        public int Speed { get; set; }
        public int Strength { get; set; }

        public static int GetLevelFromExperience(int experience)
        {
            int level = -1;
            for (int i = 0; i <= experience; i += 1000 + level * 100)
            {
                level++;
            }
            return level;
        }

        public static int GetKillExperience(int level)
        {
            var experience = (int)(Math.Pow(level, 1.2) * 100);
            return experience;
        }

        public CreatureStats Clone(Creature newOwner)
        {
            return new CreatureStats(newOwner)
            {
                Level = Level,
            };
        }

        private void SetExperience(int delta)
        {
            if(_owner.IsPossessedCreature())
            {
                MessageLog.DisplayMessage($"You gained {delta} experience. Current experience: {Experience}");
            }

            var newLevel = GetLevelFromExperience(Experience);
            if (newLevel > Level)
            {
                Level = newLevel;
            }
        }

        private void SetLevel()
        {
            if(_owner.IsPossessedCreature())
            {
                MessageLog.DisplayMessage($"You have advanced to level {Level}");

                var effectTime = 4000;

                Game1.game.Camera.CameraShake = new Rendering.Shake(effectTime, 40, 15, Game1.game.mainRng);
                Game1.game.Camera.ShakeCamera();

                var wavePS = new PSExplosion(Game1.game.Player.PossessedCreature.Position, 300, Game1.game.smokeParticleTexture, effectTime, 200f, 100, Helper.GetColorFromHex("#f5dd42"), true, Game1.game);
                ParticleSystemManager.AddSystem(wavePS, true);
            }
        }
    }
}
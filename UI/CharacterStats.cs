using System.ComponentModel;
using Pandorai.Creatures;

namespace Pandorai.UI
{
    public class CharacterStats
    {
        [Category("A")]
        public int Level { get; private set; }
        [Category("A")]
        public int Experience { get; private set; }
        [Category("A")]
        [DisplayName("Skill points")]
        public int SkillPoints { get; private set; }

        [Category("B")]
        public int Health { get; private set; }
        [Category("B")]
        [DisplayName("Max health")]
        public int MaxHealth { get; private set; }

        [Category("C")]
        public int Mana { get; private set; }
        [Category("C")]
        [DisplayName("Max mana")]
        public int MaxMana { get; private set; }

        [Category("D")]
        public int Speed { get; private set; }
        [Category("D")]
        public int Strength { get; private set; }

        [Category("E")]
        public int Stealth { get; private set; }

        [Category("F")]
        [DisplayName("Fire resistance")]
        public int FireResistance { get; private set; }
        [Category("F")]
        [DisplayName("Ice resistance")]
        public int IceResistance { get; private set; }

        public CharacterStats(CreatureStats stats)
        {
            Level = stats.Level;
            Experience = stats.Experience;
            SkillPoints = stats.SkillPoints;

            Health = stats.Health;
            MaxHealth = stats.MaxHealth;

            Mana = stats.Mana;
            MaxMana = stats.MaxMana;

            Speed = stats.Speed;
            Strength = stats.Strength;

            Stealth = stats.Stealth;

            FireResistance = stats.FireResistance;
            IceResistance = stats.IceResistance;
        }
    }
}
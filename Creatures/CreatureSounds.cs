using System.Collections.Generic;
using Pandorai.Utility;

namespace Pandorai.Creatures
{
    public class CreatureSounds
    {
        private int _attackSoundCounter;
        private int _hurtSoundCounter;
        private int _deathSoundCounter;
        private int _aggroSoundCounter;
        private int _ambientSoundCounter;
        private int _footstepSoundCounter;

        public string Attack 
        {
            get
            {
                _attackSoundCounter++;
                if(_attackSoundCounter >= AttackSounds.Count) _attackSoundCounter = 0;
                return AttackSounds[_attackSoundCounter];
            }
        }
        public string Hurt 
        {
            get
            {
                _hurtSoundCounter++;
                if(_hurtSoundCounter >= HurtSounds.Count) _hurtSoundCounter = 0;
                return HurtSounds[_hurtSoundCounter];
            }
        }
        public string Death 
        {
            get
            {
                _deathSoundCounter++;
                if(_deathSoundCounter >= DeathSounds.Count) _deathSoundCounter = 0;
                return DeathSounds[_deathSoundCounter];
            }
        }
        public string Aggro 
        {
            get
            {
                _aggroSoundCounter++;
                if(_aggroSoundCounter >= AggroSounds.Count) _aggroSoundCounter = 0;
                return AggroSounds[_aggroSoundCounter];
            }
        }
        public string Ambient 
        {
            get
            {
                _ambientSoundCounter++;
                if(_ambientSoundCounter >= AmbientSounds.Count) _ambientSoundCounter = 0;
                return AmbientSounds[_ambientSoundCounter];
            }
        }
        public string Footstep 
        {
            get
            {
                _footstepSoundCounter++;
                if(_footstepSoundCounter >= FootstepSounds.Count) _footstepSoundCounter = 0;
                return FootstepSounds[_footstepSoundCounter];
            }
        }

        public List<string> AttackSounds { get; set; } = new List<string>();
        public List<string> HurtSounds { get; set; } = new List<string>();
        public List<string> DeathSounds { get; set; } = new List<string>();
        public List<string> AggroSounds { get; set; } = new List<string>();
        public List<string> AmbientSounds { get; set; } = new List<string>();
        public List<string> FootstepSounds { get; set; } = new List<string>();

        public CreatureSounds Clone()
        {
            return new CreatureSounds
            {
                AttackSounds = new List<string>(AttackSounds),
                HurtSounds = new List<string>(HurtSounds),
                DeathSounds = new List<string>(DeathSounds),
                AggroSounds = new List<string>(AggroSounds),
                AmbientSounds = new List<string>(AmbientSounds),
                FootstepSounds = new List<string>(FootstepSounds),
            };
        }
    }
}
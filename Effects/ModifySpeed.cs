using Pandorai.Creatures;

namespace Pandorai.Effects
{
    public class ModifySpeed : Effect
    {
        public int Amount;
        public int Duration;

        private int TurnCounter;

        public override void SetAttribute(string name, string value)
        {
            if (name == "Amount")
            {
                Amount = int.Parse(value);
            }
            else if (name == "Duration")
            {
                Duration = int.Parse(value);
            }
        }

        private Creature _usingCreature;

        public override void Use(Creature usingCreature)
        {
            _usingCreature = usingCreature;
            usingCreature.Speed += Amount;
            usingCreature.TurnCame += CheckDuration;
        }

        private void CheckDuration()
        {
            TurnCounter++;
            if (TurnCounter >= Duration)
            {
                _usingCreature.Speed -= Amount;
            }
        }
    }
}
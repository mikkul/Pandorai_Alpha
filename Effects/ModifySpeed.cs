using Pandorai.Creatures;

namespace Pandorai.Effects
{
    public class ModifySpeed : Effect
    {
        public int Amount;
        public int Duration;

        public override void SetAttribute(string name, string value)
        {
            if(name == "Amount")
            {
                Amount = int.Parse(value);
            }
            else if(name == "Duration")
            {
                Duration = int.Parse(value);
            }
        }

        public override void Use(Creature usingCreature)
        {
            throw new System.NotImplementedException();
        }
    }
}
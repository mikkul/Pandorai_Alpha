using Pandorai.Creatures;

namespace Pandorai.Effects
{
    public class ModifyFireResistance : Effect
    {
        public int Amount;

        public override void SetAttribute(string name, string value)
        {
			if(name == "Amount")
			{
				Amount = int.Parse(value);
			}
        }

        public override void Use(Creature usingCreature)
        {
            usingCreature.Stats.FireResistance += Amount;
            DisplayMessage(usingCreature);
        }

        protected override string GetMessage()
        {
            if(Amount > 0)
            {
                return $"Your fire resistance increased by \\c[green]{Amount}%";
            }
            else
            {
                return $"Your fire resistance decreased by \\c[red]{-Amount}%";
            }
        }
    }
}
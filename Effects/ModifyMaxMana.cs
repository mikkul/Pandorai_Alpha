using Pandorai.Creatures;

namespace Pandorai.Effects
{
    public class ModifyMaxMana : Effect
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
            usingCreature.Stats.MaxMana += Amount;
            DisplayMessage(usingCreature);
        }

		protected override string GetMessage()
        {
            if(Amount > 0)
            {
                return $"Your max mana increased by \\c[green]{Amount}";
            }
            else
            {
                return $"Your max mana decreased by \\c[red]{-Amount}";
            }
        }
    }
}
using Pandorai.Creatures;

namespace Pandorai.Effects
{
	public class ModifyMana : Effect
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
			usingCreature.Stats.Mana += Amount;
			DisplayMessage(usingCreature);
		}

		protected override string GetMessage()
        {
            if(Amount > 0)
            {
                return $"Your mana increased by \\c[green]{Amount}";
            }
            else
            {
                return $"Your mana decreased by \\c[red]{-Amount}";
            }
        }
	}
}

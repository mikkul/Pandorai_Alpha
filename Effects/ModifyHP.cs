using Pandorai.Creatures;

namespace Pandorai.Effects
{
	public class ModifyHP : Effect
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
			usingCreature.Stats.Health += Amount;
			DisplayMessage(usingCreature);
		}

		protected override string GetMessage()
        {
            if(Amount > 0)
            {
                return $"Your health increased by \\c[green]{Amount}";
            }
            else
            {
                return $"Your health decreased by \\c[red]{-Amount}";
            }
        }
	}
}

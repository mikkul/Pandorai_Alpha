using Pandorai.Creatures;

namespace Pandorai.Effects
{
	public class ModifyStrength : Effect
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
			usingCreature.Stats.Strength += Amount;
		}
	}
}

using Pandorai.Creatures;

namespace Pandorai.Effects
{
    public class ModifyMaxHP : Effect
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
            usingCreature.Stats.MaxHealth += Amount;
            DisplayMessage(usingCreature);
        }

		protected override string GetMessage()
        {
            if(Amount > 0)
            {
                return $"Your max health increased by \\c[green]{Amount}";
            }
            else
            {
                return $"Your max health decreased by \\c[red]{-Amount}";
            }
        }    
    }
}
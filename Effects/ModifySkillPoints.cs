using Pandorai.Creatures;
using Pandorai.Sounds;

namespace Pandorai.Effects
{
    public class ModifySkillPoints : Effect
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
            usingCreature.Stats.SkillPoints += Amount;
            SoundManager.PlaySound("FX149");
            DisplayMessage(usingCreature);
        }

		protected override string GetMessage()
        {
            if(Amount > 0)
            {
                return $"Your gained \\c[green]{Amount} skill points";
            }
            else
            {
                return $"Your lost \\c[red]{Amount} skill points";
            }
        }   
    }
}
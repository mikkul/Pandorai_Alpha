using Pandorai.Creatures;

namespace Pandorai.Structures.Behaviours
{
    public class BookStand : Behaviour
    {
        public int ExperienceGained { get; set; }

        public bool IsRead { get; set; }

        public override void SetAttribute(string name, string value)
        {
            if (name == "ExperienceGained")
			{
				ExperienceGained = int.Parse(value);
			}
        }

        public override Behaviour Clone()
        {
            return new BookStand
            {
                ExperienceGained = ExperienceGained,
            };
        }

		public override void Bind()
		{
			Structure.Interacted += Interact;
		}

		public override void Unbind()
		{
			Structure.Interacted -= Interact;
		}

        public override void Interact(Creature creature)
        {
            if(creature != Main.Game.Player.PossessedCreature)
            {
                return;
            }

            if(IsRead)
            {
                return;
            }

            creature.Stats.Experience += (int)(ExperienceGained * Main.Game.ExperienceMultiplier);
            IsRead = true;
        }

        public override void ForceHandler(ForceType force)
        {
        }
    }
}
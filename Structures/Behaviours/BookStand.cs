using Pandorai.Creatures;

namespace Pandorai.Structures.Behaviours
{
    public class BookStand : Behaviour
    {
        public int ExperienceGained { get; set; }

        private bool _isRead = false;

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

            if(_isRead)
            {
                return;
            }

            creature.Stats.Experience += ExperienceGained;
            _isRead = true;
        }

        public override void ForceHandler(ForceType force)
        {
        }
    }
}
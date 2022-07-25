using Pandorai.Creatures;
using Pandorai.Sounds;
using Pandorai.Utility;

namespace Pandorai.Structures.Behaviours
{
    public class BookStand : Behaviour
    {
        public int ExperienceGained { get; set; }
        public int TextureWhenRead { get; set; }

        public bool IsRead { get; set; }

        public override void SetAttribute(string name, string value)
        {
            if (name == "ExperienceGained")
			{
				ExperienceGained = int.Parse(value);
			}
            else if (name == "TextureWhenRead")
			{
				TextureWhenRead = int.Parse(value);
			}
        }

        public override Behaviour Clone()
        {
            return new BookStand
            {
                ExperienceGained = ExperienceGained,
                TextureWhenRead = TextureWhenRead,
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

            Structure.Texture = TextureWhenRead;
            creature.Stats.Experience += (int)(ExperienceGained * Main.Game.ExperienceMultiplier);
            IsRead = true;
            SoundManager.PlaySound("metal-ringing", Structure.Tile.Index.IndexToWorldPosition(), 0.25f);
        }

        public override void ForceHandler(ForceType force)
        {
        }
    }
}
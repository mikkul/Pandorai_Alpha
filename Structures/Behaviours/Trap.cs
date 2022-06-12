using Microsoft.Xna.Framework;
using Pandorai.Creatures;

namespace Pandorai.Structures.Behaviours
{
    public class Trap : Behaviour
    {
        public int Id = -1;
        public bool Activated;

        public void Activate()
        {
            if(Activated)
            {
                return;
            }
            Activated = true;

            Structure.Texture = 0;
            Structure.ColorTint = Color.Black;
        }

		public override void Bind()
		{
            Structure.Tile.Tile.CreatureCame += Interact;
		}

		public override void Unbind()
		{
            Structure.Tile.Tile.CreatureCame -= Interact;
		}

        public override void Interact(Creature creature)
        {
            if(!Activated)
            {
                return;
            }

            creature.GetHit(1000, creature);
        }

        public override Behaviour Clone()
        {
            return new Trap
            {
                Id = Id,
            };
        }

        public override void ForceHandler(ForceType force)
        {
        }

        public override void SetAttribute(string name, string value)
        {
        }
    }
}
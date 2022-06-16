using Pandorai.Creatures;
using Pandorai.Utility;

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

            Structure.Tile.Tile.BaseColor = Structure.Tile.Tile.BaseColor.Brighten(-0.4f);
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
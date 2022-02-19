using System.Linq;
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.UI;
using Pandorai.Utility;

namespace Pandorai.Structures.Behaviours
{
    public class Teleporter : Behaviour
    {
        public int Id;

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
            var otherTeleporter = StructureManager.Structures.Find(x => 
            {
                var teleporterBehaviour = x.GetBehaviour<Teleporter>();
                return teleporterBehaviour != null && teleporterBehaviour.Id == Id && teleporterBehaviour != this;
            });
            if(otherTeleporter == null)
            {
                return;
            }

            var adjacentTiles = GenHelper.Get8Neighbours(otherTeleporter.Tile.Index);
            var freeTile = adjacentTiles.FirstOrDefault(x => !Game1.game.Map.GetTile(x).CollisionFlag);
            if(freeTile == Point.Zero)
            {
                MessageLog.DisplayMessage("The teleporter appears to be blocked");
                return;
            }

            creature.RequestMovement(freeTile);
        }

        public override Behaviour Clone()
        {
            return new Teleporter
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
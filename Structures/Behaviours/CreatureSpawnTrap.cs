using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.MapGeneration;
using Pandorai.Sounds;

namespace Pandorai.Structures.Behaviours
{
	public class CreatureSpawnTrap : Behaviour
	{
        public Dictionary<string, int> Creatures { get; set; } = new ();

        public bool Interacted { get; set; }

		public override Behaviour Clone()
		{
			var clone = new CreatureSpawnTrap
			{
			};
			return clone;
		}

		public override void SetAttribute(string name, string value)
		{
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
            if(Interacted)
            {
                return;
            }

            Interacted = true;

            var neighbours = GenHelper.Get8Neighbours(Structure.Tile.Index);

            foreach (var creaturePair in Creatures)
            {
                for (int i = 0; i < creaturePair.Value; i++)
                {
                    Point location = Point.Zero;
                    foreach (var neighbour in neighbours)
                    {
                        if(!Main.Game.Map.GetTile(neighbour).CollisionFlag)
                        {
                            location = neighbour;
                        }
                    }
                    if(location == Point.Zero)
                    {
                        break;
                    }

                    Creature creatureInstance = CreatureLoader.GetCreature(creaturePair.Key);
                    creatureInstance.MapIndex = location;
                    creatureInstance.Position = creatureInstance.MapIndex.ToVector2() * Main.Game.Options.TileSize;
                    var tile = Main.Game.Map.Tiles[location.X, location.Y];
                    tile.CollisionFlag = true;
                    Main.Game.CreatureManager.AddCreature(creatureInstance);                    
                }
            }

            SoundManager.PlaySound("teleport");
		}

		public override void ForceHandler(ForceType force)
		{
		}
	}
}

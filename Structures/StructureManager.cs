using System.Collections.Generic;
using Pandorai.Creatures;
using Pandorai.Structures.Behaviours;
using Pandorai.Tilemaps;

namespace Pandorai.Structures
{
	public static class StructureManager
	{
		public static List<Structure> Structures = new();

		public static void AddStructure(Structure structure)
		{
			Structures.Add(structure);
		}

		public static void CheckStructureInteraction(Creature incomingCreature, TileInfo info)
		{
			if (incomingCreature.Class != CreatureClass.Human) return;

			if (info.Tile.MapObject != null && info.Tile.MapObject.Structure != null && Main.Game.CreatureManager.GetCreature(info.Index) == null)
			{
				if (info.Tile.MapObject.Type == ObjectType.Interactive)
				{
					var armor = info.Tile.MapObject.Structure.GetBehaviour<Armor>();
					if(armor != null && armor.Hits > 0)
					{
						info.Tile.MapObject.Structure.UseForce(ForceType.Physical);
						incomingCreature.game.TurnManager.PlayerIsReady();
					}
					else
					{
						info.Tile.MapObject.Structure.Interact(incomingCreature);
						incomingCreature.game.TurnManager.PlayerIsReady();
					}
				}
			}
		}
	}
}

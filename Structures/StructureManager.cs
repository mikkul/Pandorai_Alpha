using Pandorai.Creatures;
using Pandorai.Tilemaps;

namespace Pandorai.Structures
{
	public static class StructureManager
	{
		public static void CheckStructureInteraction(Creature incomingCreature, TileInfo info)
		{
			if (incomingCreature.Class != CreatureClass.Human) return;

			if (info.Tile.MapObject != null && info.Tile.MapObject.Structure != null)
			{
				if (info.Tile.MapObject.Type == ObjectType.Interactive)
				{
					info.Tile.MapObject.Structure.Interact(incomingCreature);
					incomingCreature.game.TurnManager.PlayerIsReady();
				}
			}
		}
	}
}

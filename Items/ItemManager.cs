using Pandorai.Creatures;
using Pandorai.Tilemaps;
using System.Collections.Generic;

namespace Pandorai.Items
{
	public static class ItemManager
	{
		private static List<CreatureClass> itemCollectingClasses = new List<CreatureClass>
		{
			CreatureClass.Human,
		};

		public static void CheckItemInteraction(Creature incomingCreature, TileInfo info)
		{
			if (!itemCollectingClasses.Contains(incomingCreature.Class)) return;

			if(info.Tile.MapObject != null && info.Tile.MapObject.Item != null)
			{
				if(info.Tile.MapObject.Type == ObjectType.Collectible)
				{
					incomingCreature.Inventory.AddElement(info.Tile.MapObject.Item);
					info.Tile.MapObject = null;
				}
			}
		}
	}
}

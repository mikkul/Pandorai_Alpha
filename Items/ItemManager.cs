using Pandorai.Creatures;
using Pandorai.Tilemaps;
using System.Collections.Generic;
using System.Linq;

namespace Pandorai.Items
{
	public static class ItemManager
	{
		private static List<CreatureClass> itemCollectingClasses = new List<CreatureClass>
		{
			CreatureClass.Human,
		};

		public static List<Item> Items = new();

		public static void AddItem(Item item)
		{
			Items.Add(item);
		}

		public static int GetItemCount(string itemName)
		{
			return Items.Where(x => x.Id == itemName).Count();
		}

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

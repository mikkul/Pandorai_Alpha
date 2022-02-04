using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Items;
using Pandorai.Tooltips;
using System.Collections.Generic;

namespace Pandorai.Tilemaps
{
	public delegate void CreatureIncomingHandler(Creature incomingCreature);
	public delegate void ItemOnTileHandler(Item droppedItem);

	public class Tile
	{
		public event CreatureIncomingHandler CreatureCame;
		public event ItemOnTileHandler ItemUsed;

		public int BaseType;
		public int BaseTextureIndex;
		public bool CollisionFlag; // true - solid object, there is collision; false - no collision, is passable
		public bool IgnoreCollisionFlagOnSearch = false;

		public bool IsSticky = false;

		public bool IsDecal = false;
		public Color DecalColor = Color.LightGreen;

		public bool IsHighlighted = false;
		public Color HighlightColor = Color.White;

		public Color BaseColor = Color.White;

		public TooltipInfo TooltipInfo = null;

		public MapObject MapObject;

		public bool Visited = false;

		public Tile(int baseType, int texture, bool flag)
		{
			BaseType = baseType;
			BaseTextureIndex = texture;
			CollisionFlag = flag;
			MapObject = null;
		}

		public void OnCreatureCame(Creature incomingCreature)
		{
			CreatureCame?.Invoke(incomingCreature);
		}

		public void OnItemUsed(Item usedItem)
		{
			ItemUsed?.Invoke(usedItem);
		}

		public bool HasStructure()
		{
			return MapObject != null && MapObject.Structure != null;
		}

		public bool HasItem()
		{
			return MapObject != null && MapObject.Item != null;
		}

		public Tile Copy()
		{
			return new Tile(BaseType, BaseTextureIndex, CollisionFlag)
			{
				IgnoreCollisionFlagOnSearch = IgnoreCollisionFlagOnSearch,
				IsDecal = IsDecal,
				DecalColor = DecalColor,
				IsHighlighted = IsHighlighted,
				HighlightColor = HighlightColor,
				MapObject = MapObject
			};
		}
	}
}

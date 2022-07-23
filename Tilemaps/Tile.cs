using Microsoft.Xna.Framework;
using Newtonsoft.Json;
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
		public List<int> TextureIndices = new();
		public bool CollisionFlag; // true - solid object, there is collision; false - no collision, is passable
		public bool IgnoreCollisionFlagOnSearch = false;

		public List<string> ActiveTriggers = new();
		public string MusicTheme { get; set; }

		public TileModifier Modifier;

		public bool IsDecal = false;
		public Color DecalColor = Color.LightGreen;

		public bool IsHighlighted = false;
		public Color HighlightColor = Color.White;

		public Color BaseColor = Color.White;

		public TooltipInfo TooltipInfo = null;

		[JsonIgnore]
		public MapObject MapObject;

		public bool Visited = false;

		private Tile()
		{
		}

		public Tile(int baseType, int texture, bool flag)
		{
			BaseType = baseType;
			TextureIndices.Add(texture);
			CollisionFlag = flag;
			MapObject = null;
		}

		public Tile(int baseType, List<int> textures, bool flag)
		{
			BaseType = baseType;
			TextureIndices = textures;
			CollisionFlag = flag;
			MapObject = null;
		}

		public void SetTexture(int textureIndex)
		{
			TextureIndices.Clear();
			TextureIndices.Add(textureIndex);
		}

		public void AddTexture(int textureIndex)
		{
			TextureIndices.Add(textureIndex);
		}

		public void RemoveTexture(int textureIndex)
		{
			TextureIndices.Remove(textureIndex);
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
			return new Tile(BaseType, TextureIndices, CollisionFlag)
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

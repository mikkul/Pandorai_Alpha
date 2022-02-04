using Microsoft.Xna.Framework;
using Pandorai.Items;
using Pandorai.Structures;

namespace Pandorai.Tilemaps
{
	public class MapObject
	{
		public ObjectType Type;
		public int TextureIndex;
		public Item Item;
		public Structure Structure;
		public Color Colour = Color.White;

		public MapObject(ObjectType type, int index)
		{
			Type = type;
			TextureIndex = index;
		}
	}

	public enum ObjectType
	{
		Collectible,
		Interactive,
		Static,
	}
}

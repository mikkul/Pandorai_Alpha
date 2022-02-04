using Microsoft.Xna.Framework;

namespace Pandorai.Tilemaps
{
	public class TileInfo
	{
		public Point Index;
		public Tile Tile;

		public TileInfo(Point index, Tile tile)
		{
			Index = index;
			Tile = tile;
		}

		public static TileInfo GetInfo(Point index, Game1 game)
		{
			return new TileInfo(index, game.Map.GetTile(index));
		}
	}
}

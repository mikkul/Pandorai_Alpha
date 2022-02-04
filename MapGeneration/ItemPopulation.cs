using Pandorai.Tilemaps;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pandorai.Items;
using Pandorai.Creatures;
using Pandorai.Structures;
using System.Diagnostics;

namespace Pandorai.MapGeneration
{
	public static class ItemPopulation
	{
		static Random rng = new Random();

		public static void PopulateItems(Map map, Game1 game)
		{
			List<Point> emptyTiles = new List<Point>();

			List<Point> filledTiles = new List<Point>();

			for (int x = 0; x < map.Tiles.GetLength(0); x++)
			{
				for (int y = 0; y < map.Tiles.GetLength(1); y++)
				{
					if(map.Tiles[x, y].BaseType == 0 && map.Tiles[x, y].MapObject == null)
					{
						emptyTiles.Add(new Point(x, y));
					}
				}
			}

			for (int i = 0; i < 1; i++)
			{
				Point randomTile = Point.Zero;

				while (filledTiles.Contains(randomTile) || randomTile == Point.Zero)
				{
					randomTile = emptyTiles[rng.Next(emptyTiles.Count)];
				}

				var hero = CreatureLoader.GetCreature("Hero");
				hero.Position = randomTile.ToVector2() * game.Map.TileSize;
				hero.MapIndex = randomTile;
				hero.Inventory.AddElement(ItemLoader.GetItem("MassDestructionRune"));

				game.CreatureManager.AddCreature(hero);

				map.Tiles[randomTile.X, randomTile.Y].CollisionFlag = true;
				map.Tiles[randomTile.X, randomTile.Y].IgnoreCollisionFlagOnSearch = true;

				game.Player.PossessedCreature = hero;
			}
		}
	}
}

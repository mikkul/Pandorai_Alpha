using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Pandorai.Tilemaps;
using Pandorai.Structures;
using Pandorai.Items;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;

namespace Pandorai.MapGeneration
{
	public class Area
	{
		public Tile[,] TileData;

		public int Width;
		public int Height;

		public ActiveMap Level;

		public Point DesiredLocation;

		private string fileName;

		public Area(string _fileName)
		{
			fileName = _fileName;
		}

		public void LoadArea(Game1 game)
		{
			string jsonString = File.ReadAllText(Path.Combine(game.Content.RootDirectory, "MapAreas", fileName));

			dynamic json = JsonConvert.DeserializeObject(jsonString);

			JArray jarray = json.TileData;

			int[][] intTiles = (int[][])jarray.ToObject(typeof(int[][]));

			Dictionary<int, string> legend = new Dictionary<int, string>();

			foreach (var item in json.Legend)
			{
				legend.Add(item.Key.ToObject(typeof(int)), item.Value.ToObject(typeof(string)));
			}

			Level = Enum.Parse(typeof(ActiveMap), json.Level.ToObject(typeof(string)));

			DesiredLocation = new Point(json.DesiredLocation.X.ToObject(typeof(int)), json.DesiredLocation.Y.ToObject(typeof(int)));

			TileData = new Tile[intTiles.Length, intTiles[0].Length];

			Width = TileData.GetLength(0);
			Height = TileData.GetLength(1);

			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					TileData[x, y] = new Tile(0, 0, false);

					string tileType = legend[intTiles[y][x]];

					if(WorldOptions.TypeLegend.ContainsKey(tileType))
					{
						Type objType = WorldOptions.TypeLegend[tileType];

						if (objType.IsSubclassOf(typeof(Structure)))
						{
							TileData[x, y].MapObject = new MapObject(ObjectType.Interactive, 0)
							{
								Structure = (Structure)Activator.CreateInstance(objType, game, new TileInfo(new Point(x, y), TileData[x, y])),
							};
							TileData[x, y].CollisionFlag = true;
						}
						else if (objType.IsSubclassOf(typeof(Item)))
						{
							TileData[x, y].MapObject = new MapObject(ObjectType.Collectible, 0)
							{
								Item = (Item)Activator.CreateInstance(objType)
							};
						}
					}
					else
					{
						if(tileType == "Wall")
						{
							TileData[x, y].BaseType = 1;
							TileData[x, y].BaseTextureIndex = 9;
							TileData[x, y].CollisionFlag = true;
						}
						else if(tileType == "Empty")
						{
							TileData[x, y].BaseType = 0;
							TileData[x, y].BaseTextureIndex = 0;
							TileData[x, y].CollisionFlag = false;
						}
					}
				}
			}
		}
	}
}

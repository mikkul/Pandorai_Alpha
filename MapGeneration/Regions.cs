using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Pandorai.Tilemaps;

namespace Pandorai.MapGeneration
{
	public class Regions
	{
		public List<Region> RegionList = new List<Region>();

		public Dictionary<Region, List<Region>> RegionConnectionTree = new Dictionary<Region, List<Region>>();

		public Dictionary<(Region, Region), List<Point>> RegionHallways = new Dictionary<(Region, Region), List<Point>>();

		public int[,] OperatingArea;

		public int RegionIdCounter = 1;

		public void UpdateOpArea(List<Point> area, int id)
		{
			foreach (var point in area)
			{
				OperatingArea[point.X, point.Y] = id;
			}
		}

		public void CalculateConnectionGraph(Tile[,] map)
		{
			foreach (var region in RegionList)
			{
				RegionConnectionTree[region] = new List<Region>();

				foreach (var reg in RegionList)
				{
					if (region == reg) continue;

					var rayCastInfo = GenHelper.Raycast(region.MidPoint, reg.MidPoint,
						p1 =>
						{
							var neighbours = GenHelper.Get8Neighbours(p1);
							bool check = false;
							foreach (var point in neighbours)
							{
								check = map[point.X, point.Y].BaseType == 0 && OperatingArea[point.X, point.Y] != region.Id;
							}
							return check;
						},
						p2 => map[p2.X, p2.Y].BaseType == 1);

					if (rayCastInfo.Hit)
					{
						if (OperatingArea[rayCastInfo.Position.X, rayCastInfo.Position.Y] == reg.Id)
						{
							RegionConnectionTree[region].Add(reg);
							if(!RegionHallways.ContainsKey((region, reg)))
							{
								RegionHallways[(region, reg)] = new List<Point>(rayCastInfo.PointsBetween);
							}
						}
					}
				}
			}
		}

		public void PrintConnectionGraph(Main game)
		{
			System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(WorldOptions.Width * game.Map.TileSize, WorldOptions.Height * game.Map.TileSize);

			foreach (var pair in RegionConnectionTree)
			{
				var color = System.Drawing.Color.FromArgb(pair.Key.Color.R, pair.Key.Color.G, pair.Key.Color.B);
				var pen = new System.Drawing.Pen(System.Drawing.Color.Black, 10);
				var brush = new System.Drawing.SolidBrush(color);

				var point1 = new System.Drawing.Point(pair.Key.MidPoint.X * game.Map.TileSize, pair.Key.MidPoint.Y * game.Map.TileSize);

				using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
				{
					int offset = 100 + (int)Math.Pow(pair.Key.Area.Count, 0.8);
					int size = 200 + (int)Math.Pow(pair.Key.Area.Count, 0.8) * 2;
					graphics.FillEllipse(brush, point1.X - offset, point1.Y - offset, size, size);
				}

				//if (pair.Key.Name != "Temple") continue;

				foreach (var reg in pair.Value)
				{
					var point2 = new System.Drawing.Point(reg.MidPoint.X * game.Map.TileSize, reg.MidPoint.Y * game.Map.TileSize);

					using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
					{
						graphics.DrawLine(pen, point1, point2);
					}
				}
			}

			bitmap.Save(System.IO.Path.Combine(game.Content.RootDirectory, "regionConnections.bmp"));
		}

		public void AddRegion(Region region)
		{
			RegionList.Add(region);
		}

		public List<Point> CalculateRegionBorder(Region region)
		{
			List<Point> borders = new List<Point>();

			foreach (var point in region.Area)
			{
				var neighbours = GenHelper.Get8Neighbours(point);

				foreach (var neighbour in neighbours)
				{
					if (neighbour.X >= 0 && neighbour.X < OperatingArea.GetLength(0) && neighbour.Y >= 0 && neighbour.Y < OperatingArea.GetLength(1))
					{
						if (borders.Contains(neighbour)) continue;
						if (OperatingArea[neighbour.X, neighbour.Y] != region.Id) borders.Add(neighbour);
					}
				}
			}

			return borders;
		}

		public Region GetRegion(int id)
		{
			return RegionList.Find(region => region.Id == id);
		}

		public Region GetRegion(string name)
		{
			return RegionList.Find(region => region.Name == name);
		}
	}
}

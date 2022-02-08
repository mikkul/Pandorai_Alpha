using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Items;
using Pandorai.Rendering;
using Pandorai.Structures;
using Pandorai.Tilemaps;
using Pandorai.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Pandorai.MapGeneration.CustomRegions;
using Pandorai.Tooltips;
using Pandorai.Sprites;
using Pandorai.ParticleSystems;
using Pandorai.Structures.Behaviours;

namespace Pandorai.MapGeneration
{
	public class MapGenerator
	{
		public Game1 _game;

		Random rng = new Random();

		Tile[,] map;

		int[,] operatingArea;

		public Regions Rooms = new Regions();

		public Tile[,] GenerateMap(Game1 game, string regionSpreadsheet)
        {
			_game = game;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            map = new Tile[WorldOptions.Width, WorldOptions.Height];
            map.Populate(() => null);

            CustomRegionLoader customRegLoader = new CustomRegionLoader(regionSpreadsheet);

            List<Rectangle> filledSpace = new List<Rectangle>();

            List<Point> usedTiles = new List<Point>();

            List<RegMapInfo> createdRegions = new List<RegMapInfo>();

            foreach (var reg in customRegLoader.Regions)
            {
                for (int i = 0; i < reg.number; i++)
                {
                    var regInfo = GenerateRegion(reg.template);
                    createdRegions.Add(regInfo);
                }
            }

            PlaceRegions(filledSpace, usedTiles, createdRegions);

            FillUnusedSpace(usedTiles);

			ProcessRooms();

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            return map;
        }

		private void ProcessRooms()
		{
			operatingArea = new int[map.GetLength(0), map.GetLength(1)];
			Rooms.OperatingArea = new int[map.GetLength(0), map.GetLength(1)];

			List<Point> newRoomArea;
			while((newRoomArea = FloodFillRoom()).Count > 0)
			{
				Region newRoom = new Region { Id = Rooms.RegionIdCounter++ };
				newRoom.Area = newRoomArea;
				Rooms.UpdateOpArea(newRoom.Area, newRoom.Id);
				newRoom.Color = new Color(rng.Next(0, 255), rng.Next(0, 255), rng.Next(0, 255));
				newRoom.Border = Rooms.CalculateRegionBorder(newRoom);

				foreach (var point in newRoom.Area)
				{
					map[point.X, point.Y].BaseColor = newRoom.Color;
				}

				Rooms.AddRegion(newRoom);	
			}

			// the biggest "room" is in fact not a room
			// so change its color to some default one
			Region biggestRoom = null;
			int biggestRoomArea = int.MinValue;
			foreach (var room in Rooms.RegionList)
			{
				if(room.Area.Count > biggestRoomArea)
				{
					biggestRoom = room;
					biggestRoomArea = room.Area.Count;
				}
			}
			var defaultColor = Helper.GetColorFromHex("#966c4b");
			foreach (var point in biggestRoom!.Area)
			{
				map[point.X, point.Y].BaseColor = defaultColor;
			}

			// remove rooms that are too small to be considered rooms
			List<Region> roomsToBeRemoved = new List<Region>();
			int notARoomThreshold = 10;
			foreach (var room in Rooms.RegionList)
			{
				if(room.Area.Count < notARoomThreshold)
				{
					roomsToBeRemoved.Add(room);
				}
			}
			foreach (var roomToBeRemoved in roomsToBeRemoved)
			{
				Rooms.RegionList.Remove(roomToBeRemoved);
			}

			// enlarge small rooms
			foreach (var room in Rooms.RegionList)
			{
				if(room == biggestRoom)
				{
					continue;
				}

				// room is already big enough
				if(room.Area.Count >= WorldOptions.MinimalRoomSize)
				{
					continue;
				}

				while (room.Area.Count < WorldOptions.MinimalRoomSize)
				{
					var addedArea = new List<Point>(room.Border);
					for (int i = addedArea.Count - 1; i >= 0; i--)
					{
						if (addedArea[i].X <= 1 || addedArea[i].X >= WorldOptions.Width - 1 || addedArea[i].Y <= 1 || addedArea[i].Y >= WorldOptions.Height - 1)
						{
							addedArea.RemoveAt(i);
						}
					}
					room.Area.AddRange(addedArea);
					Rooms.UpdateOpArea(addedArea, room.Id);

					foreach (var point in addedArea)
					{
						map[point.X, point.Y].BaseType = 0;
						map[point.X, point.Y].BaseTextureIndex = 0;
						map[point.X, point.Y].CollisionFlag = false;
						map[point.X, point.Y].BaseColor = room.Color;
					}

					room.Border = Rooms.CalculateRegionBorder(room);
				}

				room.ProcessInterior(map);

				foreach (var point in room.Border)
				{
					map[point.X, point.Y].BaseType = 1;
					map[point.X, point.Y].BaseTextureIndex = 1;
					map[point.X, point.Y].CollisionFlag = true;
					map[point.X, point.Y].BaseColor = Color.White;
				}
			}
		
			// place doors/room entries
			Pandorai.Utility.Range roomEntryCountRange = new Pandorai.Utility.Range(1, 3);
			foreach (var room in Rooms.RegionList)
			{
				int entryCount = roomEntryCountRange.GetRandom(_game.mainRng);
				for (int i = 0; i < entryCount; i++)
				{
					var randomBorderPoint = room.Border.GetRandomElement(_game.mainRng);
					map[randomBorderPoint.X, randomBorderPoint.Y].BaseType = 0;
					map[randomBorderPoint.X, randomBorderPoint.Y].BaseTextureIndex = 0;
					map[randomBorderPoint.X, randomBorderPoint.Y].CollisionFlag = false;
					map[randomBorderPoint.X, randomBorderPoint.Y].BaseColor = room.Color;
				}
			}
		}

		private List<Point> FloodFillRoom()
		{
			Stack<Point> openList = new Stack<Point>();
			List<Point> closedList = new List<Point>();

			for (int X = 0; X < map.GetLength(0); X++)
			{
				for (int Y = 0; Y < map.GetLength(1); Y++)
				{
					if (operatingArea[X, Y] == 0 && map[X, Y].BaseType == 0 && map[X, Y].BaseColor == Color.White)
					{
						openList.Push(new Point(X, Y));
						goto EndLoop;
					}
				}
			}

			EndLoop:

			while (openList.Count > 0)
			{
				var currentTile = openList.Pop();

				var neighbours = GenHelper.GetNeighbours(currentTile);

				foreach (var neighbour in neighbours)
				{
					if (map[neighbour.X, neighbour.Y].BaseType == 0 && operatingArea[neighbour.X, neighbour.Y] == 0 && map[neighbour.X, neighbour.Y].BaseColor == Color.White)
					{
						openList.Push(neighbour);
					}
					operatingArea[neighbour.X, neighbour.Y] = 1;
				}

				closedList.Add(currentTile);
				operatingArea[currentTile.X, currentTile.Y] = 1;
			}

			return closedList;
		}

        private void FillUnusedSpace(List<Point> usedTiles)
        {
            AreaDataType[,] constraint = new AreaDataType[WorldOptions.Width, WorldOptions.Height];
            constraint.Populate(AreaDataType.Free);
            foreach (var point in usedTiles)
            {
                constraint[point.X, point.Y] = AreaDataType.Locked;
            }
            for (int x = 0; x < WorldOptions.Width; x++)
            {
                for (int y = 0; y < WorldOptions.Height; y++)
                {
                    if (x == 0 || y == 0 || x == WorldOptions.Width - 1 || y == WorldOptions.Height - 1)
                        constraint[x, y] = AreaDataType.Locked;
                }
            }

            var sample = WFCSampleLoader.Samples["goldenSample"];
            var output = WFCGenerator.GetOutput(sample, sample.IsPeriodic, false, WorldOptions.Width, WorldOptions.Height, constraint);
            for (int x = 0; x < WorldOptions.Width; x++)
            {
                for (int y = 0; y < WorldOptions.Height; y++)
                {
                    if (map[x, y] != null && map[x, y].BaseColor != Color.White) continue;

                    var pixelColor = System.Drawing.Color.FromArgb(output[x, y]).ToXnaColor();
                    var tileType = sample.TileColorLegend[pixelColor];

                    if (tileType == TileType.Empty)
                    {
                        map[x, y] = new Tile(0, 0, false);
                    }
                    else if (tileType == TileType.Wall)
                    {
                        map[x, y] = new Tile(1, 9, true);
                    }
                    else if (tileType == TileType.Floor)
                    {
                        map[x, y] = new Tile(0, 0, false);
                    }
                    else if (tileType == TileType.Path)
                    {
                        map[x, y] = new Tile(0, 0, false);
                    }
                    else if (tileType == TileType.Mud)
                    {
                        map[x, y] = new Tile(0, 0, false);
                    }

                    if (x == 0 || y == 0 || x == WorldOptions.Width - 1 || y == WorldOptions.Height - 1)
                        map[x, y] = new Tile(1, 9, true);
                }
            }
        }

        private void PlaceRegions(List<Rectangle> filledSpace, List<Point> usedTiles, List<RegMapInfo> createdRegions)
        {
            foreach (var regInfo in createdRegions)
            {
                int randomLocationX;
                int randomLocationY;
                bool isGoodFit;

                int safetyCounter = 0;

                do
                {
                    isGoodFit = true;

                    randomLocationX = rng.Next(1, WorldOptions.Width - regInfo.TileInfo.GetLength(0) - 1);
                    randomLocationY = rng.Next(1, WorldOptions.Height - regInfo.TileInfo.GetLength(1) - 1);

                    foreach (var rectangle in filledSpace)
                    {
                        if (rectangle.Intersects(new Rectangle(randomLocationX, randomLocationY, regInfo.TileInfo.GetLength(0), regInfo.TileInfo.GetLength(1))))
                        {
                            isGoodFit = false;
                            break;
                        }
                    }
                    if (safetyCounter++ > 1000)
                    {
                        Console.WriteLine("Safety counter reached 1000");
                        _game.CreatureManager.Creatures.Clear(); // first clear everything
                        LightingManager.ClearLightSources();
                        ParticleSystemManager.Clear();
                        usedTiles.Clear();
                        filledSpace.Clear();
                        map.Populate(() => null);
                        PlaceRegions(filledSpace, usedTiles, createdRegions);
                        return;
                    }
                }
                while (!isGoodFit);
                Console.WriteLine("placed region");

                usedTiles.AddRange(regInfo.TakenSpace.Select(p => p + new Point(randomLocationX, randomLocationY)));

                filledSpace.Add(new Rectangle(randomLocationX, randomLocationY, regInfo.TileInfo.GetLength(0), regInfo.TileInfo.GetLength(1)));

                foreach (var tile in regInfo.TileInfo)
                {
                    // correct map indexes for structures
                    if (tile.HasStructure())
                    {
                        tile.MapObject.Structure.Tile.Index += new Point(randomLocationX, randomLocationY);
                        tile.MapObject.Structure.BindBehaviours();
                    }
                }

                foreach (var creature in regInfo.CreatureInfo)
                {
                    // adjust positions to the world coords
                    var creatureClone = creature.Clone();
                    creatureClone.MapIndex = creature.MapIndex;
                    creatureClone.MapIndex += new Point(randomLocationX, randomLocationY);
                    creatureClone.Position = creatureClone.MapIndex.ToVector2() * _game.Options.TileSize;
                    _game.CreatureManager.AddCreature(creatureClone);
                }

                regInfo.TileInfo.CopyTo(map, randomLocationX, randomLocationY);
            }
        }

        private RegMapInfo GenerateRegion(CustomRegion region)
		{
			var info = new RegMapInfo();

			List<DimensionAreaInfo> areas = new List<DimensionAreaInfo>();

			foreach (var dimension in region.DimensionSpecifiers)
			{
				var bounds = dimension.GetAreaData(rng);
				int width1 = bounds.GetLength(0);
				int height1 = bounds.GetLength(1);

				areas.Add(new DimensionAreaInfo
				{
					Spec = dimension,
					Width = width1,
					Height = height1,
					AreaData = bounds,
				});
			}

			var nextArea = areas.Find(i => i.Spec.Parent == null);
			recursiveSetPositions(nextArea);

			// fix positions to not go below 0
			var offsetX = areas.First(x => x.Position.X == areas.Min(i => i.Position.X)).Position.X;
			var offsetY = areas.First(x => x.Position.Y == areas.Min(i => i.Position.Y)).Position.Y;
			foreach (var area in areas)
			{
				area.Position.X += -offsetX;
				area.Position.Y += -offsetY;
			}

			var boundsWidth = areas.Max(i => i.Position.X + i.Width);
			var boundsHeight = areas.Max(i => i.Position.Y + i.Height);

			AreaDataType[,] areaData = new AreaDataType[boundsWidth, boundsHeight];
			areaData.Populate(AreaDataType.Locked);

			foreach (var area in areas)
			{
				area.AreaData.CopyTo(areaData, area.Position.X, area.Position.Y);
			}

			areaData.CreateBorder(AreaDataType.Locked, 1);

			// actually run the WFC
			var width = areaData.GetLength(0);
			var height = areaData.GetLength(1);
			var sample = WFCSampleLoader.Samples[region.SampleName];
			var output = WFCGenerator.GetOutput(sample, sample.IsPeriodic, false, width, height, areaData);

			var tileData = new Tile[width, height];

			int[,] availabilityMap = new int[width, height];

			List<Point> availableTiles = new List<Point>();
			List<Point> availableBorders = new List<Point>();

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var pixelColor = System.Drawing.Color.FromArgb(output[x, y]).ToXnaColor();
					var point = new Point(x, y);

					var tileType = sample.TileColorLegend[pixelColor];

					if (tileType == TileType.Empty)
					{
						tileData[x, y] = new Tile(0, 0, false);
					}
					else if (tileType == TileType.Wall)
					{
						tileData[x, y] = new Tile(1, 9, true)
						{
							BaseColor = region.BorderColor,
						};
						availabilityMap[x, y] = 2;
						availableBorders.Add(point);
						info.TakenSpace.Add(point);
					}
					else if (tileType == TileType.Floor)
					{
						tileData[x, y] = new Tile(0, 0, false)
						{
							BaseColor = region.FloorColor,
						};
						availableTiles.Add(point);
						availabilityMap[x, y] = 1;
						info.TakenSpace.Add(point);
					}
					else if (tileType == TileType.Path)
					{
						tileData[x, y] = new Tile(0, 0, false)
						{
							BaseColor = Color.Goldenrod,
						};
						availableTiles.Add(point);
						availabilityMap[x, y] = 1;
						info.TakenSpace.Add(point);
					}
					else if (tileType == TileType.Mud)
					{
						tileData[x, y] = new Tile(0, 0, false)
						{
							BaseColor = Color.SaddleBrown,
							IsSticky = true,
						};
						availableTiles.Add(point);
						availabilityMap[x, y] = 1;
						info.TakenSpace.Add(point);
					}
				}
			}

			//
			info.TakenSpace.AddRange(GenHelper.ExpandArea(availableBorders, p => tileData.IsPointInBounds(p) && tileData[p.X, p.Y].BaseColor == Color.White));

			// create region object
			Region regionObj = new Region();
			regionObj.Area = new List<Point>(availableTiles);
			regionObj.Border = new List<Point>(availableBorders);
			regionObj.Color = region.FloorColor;
			regionObj.ProcessInterior(tileData);

			// populate area with content
			List<ContentEntry> placedContent = new List<ContentEntry>();

			foreach (var element in region.Content)
			{
				recursiveSelectContent(element);
			}

			foreach (var entry in placedContent)
			{
				foreach (var loc in entry.ProximitySpecs)
				{
					if (loc.Type == LocationType.Proximity)
					{
						var proximityLoc = (ProximitySpec)loc;
						var linkedEntries = placedContent.FindAll(x => x.Spec.Name == proximityLoc.ProximityTo);
						foreach (var linkedEntry in linkedEntries)
						{
							entry.Parents.Add(linkedEntry, proximityLoc);
						}
					}
				}
			}

			foreach (var entry in placedContent)
			{
				// count how many dependency layers deep an entry has
				// i.e if entry has a parent and this parent has his own parent then the depth is 2
				// it's 0 if it has no parents at all
				entry.AncestryDepth = recursiveCountParents(entry);
			}

			var hypotenuse = Math.Sqrt(width * width + height * height);

			Dictionary<DistanceDescription, int> distMap = new Dictionary<DistanceDescription, int>()
			{
				{ DistanceDescription.VeryClose, 1 },
				{ DistanceDescription.Close, (int)(hypotenuse * 0.10) },
				{ DistanceDescription.Far, (int)(hypotenuse * 0.30) },
				{ DistanceDescription.VeryFar, (int)(hypotenuse * 0.45) },
				{ DistanceDescription.FarthestAway, (int)(hypotenuse * 0.90) }
			};

			for (int i = 1; i < 10; i++) // we can safely assume that no content entry goes lower than 10 layers deep
			{
				foreach (var entry in placedContent.Where(x => x.AncestryDepth == i))
				{
					positionContent(entry);
				}
			}

			//
			info.TileInfo = tileData;

			void recursiveSetPositions(DimensionAreaInfo nextSpec)
			{
				if (nextSpec.Spec.Parent != null)
				{
					var placement = nextSpec.Spec.PossiblePlacements.GetRandomElement(rng);
					var parent = areas.Find(i => i.Spec == nextSpec.Spec.Parent);
					var realOffsetX = nextSpec.Spec.OffsetX.GetRandom(rng);
					var realOffsetY = nextSpec.Spec.OffsetY.GetRandom(rng);

					switch (placement)
					{
						case Placement.Left:
							nextSpec.Position = parent.Position;
							nextSpec.Position.X -= nextSpec.Width;
							nextSpec.Position.X -= (int)realOffsetX;
							nextSpec.Position.Y += parent.Height / 2 - nextSpec.Height / 2;
							nextSpec.Position.Y += (int)realOffsetY;
							// TODO: add vertical allignment
							break;
						case Placement.Right:
							nextSpec.Position = parent.Position;
							nextSpec.Position.X += parent.Width;
							nextSpec.Position.X += (int)realOffsetX;
							nextSpec.Position.Y += parent.Height / 2 - nextSpec.Height / 2;
							nextSpec.Position.Y += (int)realOffsetY;
							break;
						case Placement.Top:
							nextSpec.Position = parent.Position;
							nextSpec.Position.Y -= nextSpec.Height;
							nextSpec.Position.Y -= (int)realOffsetY;
							nextSpec.Position.X += parent.Width / 2 - nextSpec.Width / 2;
							nextSpec.Position.X += (int)realOffsetX;
							break;
						case Placement.Bottom:
							nextSpec.Position = parent.Position;
							nextSpec.Position.Y += parent.Height;
							nextSpec.Position.Y += (int)realOffsetY;
							nextSpec.Position.X += parent.Width / 2 - nextSpec.Width / 2;
							nextSpec.Position.X += (int)realOffsetX;
							break;
						case Placement.Center:
							nextSpec.Position = parent.Position;
							nextSpec.Position.X += parent.Width / 2 - nextSpec.Width / 2;
							nextSpec.Position.X += (int)realOffsetX;
							nextSpec.Position.Y += parent.Height / 2 - nextSpec.Height / 2;
							nextSpec.Position.Y += (int)realOffsetY;
							break;
					}
				}

				foreach (var child in nextSpec.Spec.Children)
				{
					recursiveSetPositions(areas.Find(i => i.Spec == child));
				}
			}

			void recursiveSelectContent(ElementNode node, ContentEntry modifiedEntry = null)
			{
				float randomChance = (float)rng.NextDouble();
				if (randomChance >= node.Chance)
				{
					return;
				}

				if (node.Type == NodeType.Choice)
				{
					int rand = rng.Next(node.ChildNodes.Sum(x => x.Weight));
					int sum = 0;
					foreach (var option in node.ChildNodes)
					{
						if (rand < sum + option.Weight)
						{
							recursiveSelectContent(option, modifiedEntry);
							return;
						}
						else
						{
							sum += option.Weight;
						}
					}
				}

				else if (node.Type == NodeType.Creature || node.Type == NodeType.Item || node.Type == NodeType.Structure || node.Type == NodeType.Entrance || node.Type == NodeType.InventoryItem)
				{
					EntitySpecifier entitySpec = (EntitySpecifier)node.Object;
					var number = entitySpec.Number.GetRandom(rng);

					for (int i = 0; i < number; i++)
					{
						if(node.Type == NodeType.InventoryItem)
						{
							modifiedEntry.Inventory.Add(ItemLoader.GetItem(node.Object.Name));
							continue;
						}

						ContentEntry entry = new ContentEntry
						{
							Spec = node.Object,
							Node = node,
						};
						placedContent.Add(entry);

						foreach (var modifier in node.ChildNodes)
						{
							modifiedEntry = entry;
							recursiveSelectContent(modifier, modifiedEntry);
						}
					}
				}

				else if(node.Type == NodeType.Trigger)
				{
					ContentEntry entry = new ContentEntry
					{
						Spec = node.Object,
						Node = node,
					};
					placedContent.Add(entry);

					foreach (var modifier in node.ChildNodes)
					{
						modifiedEntry = entry;
						recursiveSelectContent(modifier, modifiedEntry);
					}
				}

				else if (node.Type == NodeType.ProximitySpec )
				{
					var modifier = (Modifier)node.Object;
					modifiedEntry.ProximitySpecs.Add((ProximitySpec)modifier.Spec);
				}
				else if(node.Type == NodeType.LayerSpec)
				{
					var modifier = (Modifier)node.Object;
					modifiedEntry.LayerSpecs.Add((LayerSpec)modifier.Spec);
				}
				else if(node.Type == NodeType.StripeSpec)
				{
					var modifier = (Modifier)node.Object;
					modifiedEntry.StripeSpecs.Add((StripeSpec)modifier.Spec);
				}

				else
				{
					foreach (var child in node.ChildNodes)
					{
						recursiveSelectContent(child, modifiedEntry);
					}
				}
			}

			int recursiveCountParents(ContentEntry entry)
			{
				var deepest = 0;
				foreach (var parent in entry.Parents.Keys)
				{
					var depth = recursiveCountParents(parent);
					if(depth > deepest)
					{
						deepest = depth;
					}
				}
				return deepest + 1;
			}

			void positionContent(ContentEntry entry)
			{
				Point finalPosition = availableTiles.GetRandomElement(rng);
				if (entry.Node.Type == NodeType.Entrance)
					finalPosition = availableBorders.GetRandomElement(rng);

				var farParents = entry.Parents.Where(x => x.Value.Distance == DistanceDescription.Far || x.Value.Distance == DistanceDescription.VeryFar || x.Value.Distance == DistanceDescription.FarthestAway);
				var closeParents = entry.Parents.Where(x => x.Value.Distance == DistanceDescription.Close || x.Value.Distance == DistanceDescription.VeryClose);

				var closeParentsNoDuplicates = closeParents.GroupBy(x => x.Key.Spec.Name).Select(x => x.ToList().GetRandomElement(rng));
				var selectedParents = farParents.Concat(closeParentsNoDuplicates);

				List<List<Point>> constraints = new List<List<Point>>();

				if(entry.Node.Type == NodeType.Entrance)
				{
					//constraints.Add(availableBorders);
				}

				List<Point> layerConstraint = new List<Point>();
				foreach (var layerSpec in entry.LayerSpecs)
				{
					int layerCount = regionObj.InteriorLayers.Count;
					int layerIndex;
					if (layerSpec.Location == LayerLocation.Border)
					{
						layerIndex = Math.Min(layerCount - 1, 0 + layerSpec.Offset);
					}
					else if (layerSpec.Location == LayerLocation.Center)
					{
						layerIndex = Math.Max(0, layerCount - 1 - layerSpec.Offset);
					}
					else
					{
						layerIndex = Math.Min(layerCount - 1, Math.Max(0, (int)Math.Floor(layerCount / 2f) + layerSpec.Offset));
					}

					for (int i = 0; i < layerSpec.Thickness; i++)
					{
						if (layerSpec.Location == LayerLocation.Border)
						{
							layerIndex += i;
						}
						else if (layerSpec.Location == LayerLocation.Center)
						{
							layerIndex += -i;
						}
						else
						{
							layerIndex += i % 2 == 0 ? (int)-Math.Ceiling(i / 2f) : (int)Math.Ceiling(i / 2f);
						}
						if(layerIndex < regionObj.InteriorLayers.Count && layerIndex >= 0)
							layerConstraint.AddRange(regionObj.InteriorLayers[layerIndex]);
					}
				}
				if(entry.LayerSpecs.Count > 0)
				{
					constraints.Add(layerConstraint);
				}

				List<Point> stripeConstraint = new List<Point>();
				foreach (var stripeSpec in entry.StripeSpecs)
				{
					for (int i = 0; i < stripeSpec.Thickness; i++)
					{
						int offset = i % 2 == 0 ? (int)-Math.Ceiling(i / 2f) : (int)Math.Ceiling(i / 2f);
						stripeConstraint.AddRange(regionObj.GetStripe(stripeSpec.Direction, stripeSpec.Position, offset));
					}

				}
				if (entry.StripeSpecs.Count > 0)
				{
					constraints.Add(stripeConstraint);
				}

				foreach (var parentPair in selectedParents)
				{
					var dist = distMap[parentPair.Value.Distance];
					var parent = parentPair.Key;
					List<Point> bound;
					var proxSpec = parentPair.Value;
					if (entry.Node.Type == NodeType.Entrance)
					{
						bound = GenHelper.GetRingAreaPoints(parent.Position.X, parent.Position.Y, dist, proxSpec.Tolerance, ExpansionDirection.OutIn, x => availabilityMap.IsPointInBounds(x) && availabilityMap[x.X, x.Y] == 2);
					}
					else if(entry.Node.Type == NodeType.Trigger)
					{
						bound = GenHelper.GetRingAreaPoints(parent.Position.X, parent.Position.Y, dist, proxSpec.Tolerance, ExpansionDirection.InOut, x => availabilityMap.IsPointInBounds(x));
					}
					else
					{
						bound = GenHelper.GetRingAreaPoints(parent.Position.X, parent.Position.Y, dist, proxSpec.Tolerance, ExpansionDirection.InOut, x => availabilityMap.IsPointInBounds(x) && availabilityMap[x.X, x.Y] == 1);
					}
					foreach (var point in bound)
					{
						//tileData[point.X, point.Y].BaseColor = tileData[point.X, point.Y].BaseColor.Brighten(-0.25f);
					}
					constraints.Add(bound);
				}

				if(entry.Node.Type != NodeType.Trigger)
					constraints.Add(new List<Point>(availableTiles)); // dont remove, it's needed

				if(constraints.Count > 1)
				{
					//Debugger.Break();
					List<Point> lastIntersection = constraints[0];

					for (int i = 1; i < constraints.Count; i++)
					{
						var intersection = lastIntersection.Intersect(constraints[i]).ToList();
						if (intersection.Count <= 0)
						{
							float smallestDist = float.MaxValue;
							Point point1 = Point.Zero;
							Point point2 = Point.Zero;
							for (int j1 = 0; j1 < lastIntersection.Count; j1++)
							{
								for (int j2 = 0; j2 < constraints[i].Count; j2++)
								{
									var dist = lastIntersection[j1].DistanceTo(constraints[i][j2]);
									if (dist < smallestDist)
									{
										smallestDist = dist;
										point1 = lastIntersection[j1];
										point2 = constraints[i][j2];
									}
								}
							}
							Point pointBetween = new Point((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);
							if(entry.Node.Type == NodeType.Entrance)
							{
								if (availabilityMap[pointBetween.X, pointBetween.Y] != 2)
								{
									pointBetween = GenHelper.FloodfillSearchFirst(availabilityMap, pointBetween, x => availabilityMap[x.X, x.Y] == 2);
								}
							}
							else if (entry.Node.Type == NodeType.Trigger)
							{
								pointBetween = GenHelper.FloodfillSearchFirst(availabilityMap, pointBetween, x => true);
							}
							else
							{
								if (availabilityMap[pointBetween.X, pointBetween.Y] != 1)
								{
									pointBetween = GenHelper.FloodfillSearchFirst(availabilityMap, pointBetween, x => availabilityMap[x.X, x.Y] == 1);
								}
							}

							lastIntersection = new List<Point>() { pointBetween };
						}
						else
						{
							lastIntersection = intersection;
						}

						var randomColor = new Color(rng.Next(256), rng.Next(256), rng.Next(256));
						foreach (var point in lastIntersection)
						{
							//tileData[point.X, point.Y].BaseColor = i == bounds.Count - 1 ? Color.Yellow : randomColor;
						}
					}

					finalPosition = lastIntersection.GetRandomElement(rng);
				}

				if (entry.Node.Type == NodeType.Trigger)
				{
					List<Point> placementPoints = constraints.SelectMany(x => x).Distinct().ToList();
					TriggerSpecifier triggerSpec = (TriggerSpecifier)entry.Spec;
					foreach (var point in placementPoints)
					{
						tileData[point.X, point.Y].CreatureCame += triggerSpec.Handler;
					}
					return;
				}

				if (entry.Node.Type == NodeType.Entrance)
				{
					bool isGoodCheck(Point point)
					{
						if (!availableBorders.Contains(point)) return false;

						int floorCount = 0;
						int wallCount = 0;
						var neighbours = GenHelper.GetNeighbours(point);
						foreach (var neighbour in neighbours)
						{
							if (!tileData.IsPointInBounds(neighbour))
							{
								break;
							}

							if (tileData[neighbour.X, neighbour.Y].BaseType == 0 && tileData[neighbour.X, neighbour.Y].BaseColor == region.FloorColor)
							{
								floorCount++;
							}
							else if (tileData[neighbour.X, neighbour.Y].BaseType == 1)
							{
								wallCount++;
							}
						}
						return floorCount == 1 && wallCount == 2;
					}

					if (!isGoodCheck(finalPosition))
					{
						finalPosition = GenHelper.FloodfillSearchFirst(availabilityMap, finalPosition, x => isGoodCheck(x));
					}
				}

				entry.Position = finalPosition;

				//
				var node = entry.Node;
				EntitySpecifier entitySpec = (EntitySpecifier)entry.Spec;
				var number = entitySpec.Number.GetRandom(rng);

				var tile = tileData[finalPosition.X, finalPosition.Y];
				var index = finalPosition;
				availableTiles.Remove(index);
				availabilityMap[index.X, index.Y] = 0;
				availableBorders.Remove(index);
				if (node.Type == NodeType.Creature)
				{
					Creature creatureInstance = CreatureLoader.GetCreature(entitySpec.Name);
					creatureInstance.MapIndex = index;
					if (entry.Inventory.Count > 0)
						creatureInstance.Inventory.AddElements(entry.Inventory);
					tile.CollisionFlag = true;
					info.CreatureInfo.Add(creatureInstance);
				}
				else if (node.Type == NodeType.Structure)
				{
					Structure structureInstance = StructureLoader.GetStructure(entitySpec.Name);
					structureInstance.Tile = new TileInfo(index, tile);
					if (entry.Inventory.Count > 0)
					{
						Container container = (Container)structureInstance.Behaviours.Find(x => x.GetType() == typeof(Container));
						container.Inventory.AddElements(entry.Inventory);
					}

					tile.MapObject = new MapObject(ObjectType.Interactive, 0)
					{
						Structure = structureInstance,
					};
					tile.CollisionFlag = true;
					if(tile.TooltipInfo == null)
					{
						tile.TooltipInfo = new ImageTooltip
						{
							Title = entitySpec.Name,
							Image = TilesheetManager.MapSpritesheetTexture.ExtractSubtexture(TilesheetManager.MapObjectSpritesheet[structureInstance.Texture].Rect, Game1.game.GraphicsDevice),
						};
					}
				}
				else if (node.Type == NodeType.Item)
				{
					Item itemInstance = ItemLoader.GetItem(entitySpec.Name);
					tile.MapObject = new MapObject(ObjectType.Collectible, 0)
					{
						Item = itemInstance,
					};
				}
				else if(node.Type == NodeType.Entrance)
				{
					tile.BaseType = 0;
					tile.BaseTextureIndex = 0;
					tile.CollisionFlag = false;
					tile.BaseColor = region.FloorColor;
				}
			}

			return info;
		}

		private class RegMapInfo
		{
			public Tile[,] TileInfo;
			public List<Creature> CreatureInfo = new List<Creature>();
			public List<Point> TakenSpace = new List<Point>();
		}

		private class DimensionAreaInfo
		{
			public DimensionSpecifier Spec;
			public int Width;
			public int Height;
			public AreaDataType[,] AreaData;
			public Point Position;
		}

		private class ContentEntry
		{
			public ObjectSpecifier Spec;
			public List<ProximitySpec> ProximitySpecs = new List<ProximitySpec>();
			public List<LayerSpec> LayerSpecs = new List<LayerSpec>();
			public List<StripeSpec> StripeSpecs = new List<StripeSpec>();
			public List<Item> Inventory = new List<Item>();
			public Dictionary<ContentEntry, ProximitySpec> Parents = new Dictionary<ContentEntry, ProximitySpec>();
			public Point Position;
			public ElementNode Node;
			public int AncestryDepth;
		}
	}
}

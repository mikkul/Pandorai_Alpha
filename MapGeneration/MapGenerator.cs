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
using System.Linq;
using Pandorai.MapGeneration.CustomRegions;
using Pandorai.ParticleSystems;
using Pandorai.Structures.Behaviours;
using Pandorai.Sounds;
using System.Threading.Tasks;

namespace Pandorai.MapGeneration
{
	public class MapGenerator
	{
		public Main Game;
		public Regions Rooms = new Regions();

		private Random _rng = new Random();

		private Tile[,] _map;

		private int[,] _operatingArea;

		private Color _freeSpaceFloorColor = Helper.GetColorFromHex("#966c4b");

		private List<Point> _freeSpaceTiles = new List<Point>();

		private Dictionary<string, int> _maxAllowedItemCount;

		private Creature _trapCreature;

		public MapGenerator()
		{
			_trapCreature = new Creature(Main.Game);
			_trapCreature.Id = "Trap";
			_trapCreature.Stats = new CreatureStats(_trapCreature);
			_trapCreature.Stats.Strength = 45;
		}

		public async Task<Tile[,]> GenerateMapAsync(Main game, string regionSpreadsheet)
		{
			var map = await Task.FromResult(GenerateMap(game, regionSpreadsheet));
			return map;
		}

		public Tile[,] GenerateMap(Main game, string regionSpreadsheet)
        {
			Game = game;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

			_maxAllowedItemCount = new();
			_maxAllowedItemCount["BlinkPotion"] = 2;

            _map = new Tile[WorldOptions.Width, WorldOptions.Height];
            _map.Populate(() => null);

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

            MakeRooms(usedTiles);

			ProcessRooms();

			FillSpaceBetween();

			//DoSomethingAboutUnreachableSpace(); nvm it makes things even worse

			PlaceTeleporters();

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            return _map;
        }

        private void FillSpaceBetween()
        {
            AreaDataType[,] constraint = new AreaDataType[WorldOptions.Width, WorldOptions.Height];
            constraint.Populate(AreaDataType.Locked);
            foreach (var point in _freeSpaceTiles)
			{
				constraint[point.X, point.Y] = AreaDataType.Free;
			}

            var sample = WFCSampleLoader.Samples["spaceBetweenSample"];
            var output = WFCGenerator.GetOutput(sample, sample.IsPeriodic, false, WorldOptions.Width, WorldOptions.Height, constraint);

            foreach (var point in _freeSpaceTiles)
			{
				Color pixelColor = System.Drawing.Color.FromArgb(output[point.X, point.Y]).ToXnaColor();

				if (pixelColor == Color.Black) // stones
				{
					_map[point.X, point.Y] = new Tile(1, 9, true)
					{
						BaseColor = Color.Gray,
					};
				}
				else if (pixelColor == new Color(0, 255, 0)) // trees
				{
					_map[point.X, point.Y] = new Tile(1, 9, true)
					{
						BaseColor = new Color(0, 190, 0),
					};
				}
			}
        }

        private void DoSomethingAboutUnreachableSpace()
        {
			bool isUncreachableSpace = true;
			while(isUncreachableSpace)
			{
				isUncreachableSpace = false;
				var localOpArea = new int[_map.GetLength(0), _map.GetLength(1)];
				FloodFill(localOpArea);

				var uncreachablePoints = new List<Point>();
				for (int x = 0; x < _map.GetLength(0); x++)
				{
					for (int y = 0; y < _map.GetLength(1); y++)
					{
						if(localOpArea[x, y] == 0 && _map[x, y].BaseType == 0 && _map[x, y].BaseColor == _freeSpaceFloorColor)
						{
							isUncreachableSpace = true;
							uncreachablePoints.Add(new Point(x, y));
						}
					}
				}

				foreach (var point in uncreachablePoints)
				{
					var neighbours = GenHelper.GetNeighbours(point);
					foreach (var neighbour in neighbours)
					{
						if(!_map.IsPointInBounds(neighbour))
						{
							continue;
						}
						_map[neighbour.X, neighbour.Y].CollisionFlag = false;
						_map[neighbour.X, neighbour.Y].BaseColor = _freeSpaceFloorColor;
						_map[neighbour.X, neighbour.Y].BaseType = 0;
						_map[neighbour.X, neighbour.Y].SetTexture(0);
					}
				}
			}

            void FloodFill(int[,] opArea)
            {
                Stack<Point> openList = new Stack<Point>();
                List<Point> closedList = new List<Point>();

                for (int x = 0; x < _map.GetLength(0); x++)
                {
                    for (int y = 0; y < _map.GetLength(1); y++)
                    {
                        if (opArea[x, y] == 0 && _map[x, y].BaseType == 0 && _map[x, y].BaseColor == _freeSpaceFloorColor)
                        {
                            openList.Push(new Point(x, y));
                            goto EndLoop;
                        }
                    }
                }

            	EndLoop:

                while (openList.Count > 0)
                {
                    var currentTile = openList.Pop();

                    var neighbours = GenHelper.Get8Neighbours(currentTile);

                    foreach (var neighbour in neighbours)
                    {
						if(!_map.IsPointInBounds(neighbour))
						{
							continue;
						}
                        if (_map[neighbour.X, neighbour.Y].BaseType == 0 && opArea[neighbour.X, neighbour.Y] == 0)
                        {
                            openList.Push(neighbour);
                        }
                        opArea[neighbour.X, neighbour.Y] = 1;
                    }

                    closedList.Add(currentTile);
                    opArea[currentTile.X, currentTile.Y] = 1;
                }
            }
        }

        private void PlaceTeleporters()
		{
			var freeSpace = new List<Point>();
			for (int x = 0; x < _map.GetLength(0); x++)
			{
				for (int y = 0; y < _map.GetLength(1); y++)
				{
					if(!_map[x, y].CollisionFlag && _map[x, y].BaseColor == _freeSpaceFloorColor)
					{
						freeSpace.Add(new Point(x, y));
					}
				}
			}

			Color[] brightColors = new Color[]
			{
				Color.Yellow,
				Color.Green,
				Color.Red,
				Color.Blue,
				Color.Purple,
				Color.Cyan,
				Color.Orange,
			};

			int noOfTeleporterPairs = 7;
			for (int i = 0; i < noOfTeleporterPairs; i++)
			{
				var color = brightColors[i];
				for (int j = 0; j < 1; j++)
				{
					var randomPoint = freeSpace.GetRandomElement(Main.Game.MainRng);
					var teleporterInstance = PlaceStructure("Teleporter", randomPoint);
					teleporterInstance.ColorTint = color;
					var teleporterBehaviour = teleporterInstance.GetBehaviour<Teleporter>();
					teleporterBehaviour.Id = i;

					var otherCorrespondingTeleporter = StructureManager.Structures.First(x =>
					{
						var otherTelBeh = x.GetBehaviour<Teleporter>();
						if(otherTelBeh == null)
						{
							return false;
						}
						return x != teleporterInstance && x.Id == "Teleporter" && otherTelBeh.Id == -1;
					});
					otherCorrespondingTeleporter.ColorTint = color;
					otherCorrespondingTeleporter.GetBehaviour<Teleporter>().Id = i;
				}
			}
		}

		private void ProcessRooms()
		{
			_operatingArea = new int[_map.GetLength(0), _map.GetLength(1)];
			Rooms.OperatingArea = new int[_map.GetLength(0), _map.GetLength(1)];

			List<Point> newRoomArea;
			while((newRoomArea = FloodFillRoom()).Count > 0)
			{
				Region newRoom = new Region { Id = Rooms.RegionIdCounter++ };
				newRoom.Area = newRoomArea;
				Rooms.UpdateOpArea(newRoom.Area, newRoom.Id);
				newRoom.Color = new Color(_rng.Next(0, 255), _rng.Next(0, 255), _rng.Next(0, 255));
				newRoom.Border = Rooms.CalculateRegionBorder(newRoom);

				foreach (var point in newRoom.Area)
				{
					_map[point.X, point.Y].BaseColor = newRoom.Color;
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
			foreach (var point in biggestRoom!.Area)
			{
				_map[point.X, point.Y].BaseColor = _freeSpaceFloorColor;
			}
			_freeSpaceTiles = biggestRoom.Area;

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
				foreach (var point in roomToBeRemoved.Area)
				{
					_map[point.X, point.Y].BaseColor = _freeSpaceFloorColor;
				}
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
						_map[point.X, point.Y].BaseType = 0;
						_map[point.X, point.Y].SetTexture(0);
						_map[point.X, point.Y].CollisionFlag = false;
						_map[point.X, point.Y].BaseColor = room.Color;
					}

					room.Border = Rooms.CalculateRegionBorder(room);
				}

				room.ProcessInterior(_map);

				foreach (var point in room.Border)
				{
					_map[point.X, point.Y].BaseType = 1;
					_map[point.X, point.Y].SetTexture(1);
					_map[point.X, point.Y].CollisionFlag = true;
					_map[point.X, point.Y].BaseColor = Color.White;
				}
			}
		
			// place doors/room entries
			int[] roomEntryCountWeights = new int[] { 1, 2, 2, 2, 2, 3, 3 };
			bool[] isRoomDooredWeights = new bool[] { false, true, true };
			foreach (var room in Rooms.RegionList)
			{
				bool isDoor = isRoomDooredWeights.GetRandomElement(Main.Game.MainRng);
				int entryCount = roomEntryCountWeights.GetRandomElement(Main.Game.MainRng);
				for (int i = 0; i < entryCount; i++)
				{
					int safetyCounter = 0;
					Point randomBorderPoint;
					do
					{
						randomBorderPoint = room.Border.GetRandomElement(Game.MainRng);
						safetyCounter++;
					}
					while(!isGoodEntrance(randomBorderPoint) && safetyCounter < 1000);

					bool isGoodEntrance(Point point)
					{
						int floorCount = 0;
						int wallCount = 0;
						var neighbours = GenHelper.GetNeighbours(point);
						foreach (var neighbour in neighbours)
						{
							if (!_map.IsPointInBounds(neighbour))
							{
								break;
							}

							if (_map[neighbour.X, neighbour.Y].BaseType == 0 && _map[neighbour.X, neighbour.Y].BaseColor == room.Color)
							{
								floorCount++;
							}
							else if (_map[neighbour.X, neighbour.Y].BaseType == 1)
							{
								wallCount++;
							}
						}
						return floorCount == 1 && wallCount == 2;
					}

					var tile = _map[randomBorderPoint.X, randomBorderPoint.Y];
					tile.BaseType = 0;
					tile.SetTexture(0);
					tile.CollisionFlag = false;
					tile.BaseColor = room.Color;
					if(isDoor)
					{
						PlaceStructure("YellowDoor", randomBorderPoint);
					}
				}
			}
		
			// fill rooms with content
			foreach (var room in Rooms.RegionList)
			{
				if(room == biggestRoom)
				{
					continue;
				}
				
				FillRoomWithContent(room);
			}
		}

		private void FillRoomWithContent(Region room)
		{
			Random rng = Main.Game.MainRng;
			room.ProcessInterior(_map);

			// place torches
			for (int i = 0; i < 2; i++)
			{
				var position = GetRandomUntakenPosition(room.InteriorLayers[0]);
				PlaceStructure("Torch", position);
			}

			// place bookstands
			for (int i = 0; i < 2; i++)
			{
				bool doPlace = rng.NextFloat() < 0.75f;
				var position = GetRandomUntakenPosition(room.InteriorLayers[0]);
				PlaceStructure("BookStand", position);
			}

			// place items
			{
				var itemSet1 = new[] { "", "YellowKey", "BlueKey", "SmallHealthPotion", "SmallManaPotion" };
				var weightsSet1 = new[] { 10, 9, 4, 9, 9 };
				var chosenItems1 = GetWeightedChoices(itemSet1, weightsSet1, 3);

				var itemSet2 = new[] { "", "FireArrowSpell" };
				var weightsSet2 = new[] { 14, 6 };
				var chosenItems2 = GetWeightedChoices(itemSet2, weightsSet2, 1);

				var itemSet3 = new[] { "", "BlinkPotion" };
				var weightsSet3 = new[] { 3, 3 };
				var chosenItems3 = GetWeightedChoices(itemSet3, weightsSet3, 1);

				var itemSet4 = new[] { "", "SmallStrengthPotion", "SmallEndurancePotion", "SmallEnergyPotion" };
				var weightsSet4 = new[] { 12, 3, 3, 3 };
				var chosenItems4 = GetWeightedChoices(itemSet4, weightsSet4, 1);				

				var chosenItems = chosenItems1.Concat(chosenItems2).Concat(chosenItems3).Concat(chosenItems4);

				foreach (var itemName in chosenItems)
				{
					if(itemName == "")
					{
						continue;
					}
					var position = GetRandomUntakenPosition(room.Area);
					var item = PlaceItem(itemName, position);
				}
			}

			// place chests
			for (int i = 0; i < 2; i++)
			{
				bool doPlace = rng.NextFloat() < 0.3f;
				if(!doPlace)
				{
					continue;
				}
				var position = GetRandomUntakenPosition(room.InteriorLayers[0]);
				var chest = PlaceStructure("Chest", position);
				var chestContainer = chest.GetBehaviour<Container>();

				var itemSet1 = new[] { "", "YellowKey", "BlueKey" };
				var weightsSet1 = new[] { 3, 10, 4 };
				var chosenItems1 = GetWeightedChoices(itemSet1, weightsSet1, 2);

				var itemSet2 = new[] { "", "HealthPotion", "FireArrowSpell", "IceArrowSpell", "SpeedPotion" };
				var weightsSet2 = new[] { 7, 3, 7, 3, 2 };
				var chosenItems2 = GetWeightedChoices(itemSet2, weightsSet2, 2);

				var chosenItems = chosenItems1.Concat(chosenItems2);

				foreach (var itemName in chosenItems)
				{
					if(itemName == "")
					{
						continue;
					}
					var itemInstance = ItemLoader.GetItem(itemName);
					chestContainer.Inventory.AddElement(itemInstance);
				}
			}

			// place barrels
			for (int i = 0; i < 3; i++)
			{
				bool doPlace = rng.NextFloat() < 0.6f;
				if(!doPlace)
				{
					continue;
				}
				var position = GetRandomUntakenPosition(room.Area);
				var barrel = PlaceStructure("Barrel", position);
				var barrelContainer = barrel.GetBehaviour<Container>();

				var itemSet1 = new[] { "", "HealthPotion", "ManaPotion" };
				var weightsSet1 = new[] { 9, 3, 1 };
				var chosenItems1 = GetWeightedChoices(itemSet1, weightsSet1, 1);

				foreach (var itemName in chosenItems1)
				{
					if(itemName == "")
					{
						continue;
					}
					var itemInstance = ItemLoader.GetItem(itemName);
					barrelContainer.Inventory.AddElement(itemInstance);
				}
			}

			// place monsters
			for (int i = 0; i < 4; i++)
			{
				bool doPlace = rng.NextFloat() < 0.5f;
				if(!doPlace)
				{
					continue;
				}

				var creatureSet1 = new[] { "Rat", "Spider" };
				var weightsSet1 = new[] { 3, 1 };
				var chosenCreatures1 = GetWeightedChoices(creatureSet1, weightsSet1, 1);

				foreach (var creatureName in chosenCreatures1)
				{
					if(creatureName == "")
					{
						continue;
					}
					var position = GetRandomUntakenPosition(room.Area);
					var creatureInstance = PlaceCreature(creatureName, position);
				}
			}

			// place traps
			{
				bool doPlace = rng.NextFloat() < 0.5f;
				if(doPlace)
				{
					var position = GetRandomUntakenPosition(room.Area);
					_map[position.X, position.Y].Modifier |= TileModifier.Trap;
					_map[position.X, position.Y].BaseColor = _map[position.X, position.Y].BaseColor.Brighten(-0.2f);
					_map[position.X, position.Y].AddTexture(8);
					_map[position.X, position.Y].CreatureCame += c =>
					{
						c.OnGotHit(_trapCreature);
					};
				}
			}				
		}

		private List<string> GetWeightedChoices(string[] itemSet, int[] weightsSet, int numberOfItems = 1)
		{
			int totalNumber = weightsSet.Sum();
			List<string> weightedChoiceSet = new();
			for (int i = 0; i < itemSet.Length; i++)
			{
				for (int n = 0; n < weightsSet[i]; n++)
				{
					weightedChoiceSet.Add(itemSet[i]);
				}
			}

			List<string> chosenItems = new();
			for (int i = 0; i < numberOfItems; i++)
			{
				chosenItems.Add(weightedChoiceSet.GetRandomElement(Main.Game.MainRng));
			}

			return chosenItems;
		}

		private Point GetRandomUntakenPosition(List<Point> possiblePoints)
		{
			Point position;
			do
			{
				position = possiblePoints.GetRandomElement(Main.Game.MainRng);
			}
			while(_map[position.X, position.Y].CollisionFlag);

			return position;
		}

		private Item PlaceItem(string itemName, Point point)
		{
			if(_maxAllowedItemCount.ContainsKey(itemName) && ItemManager.GetItemCount(itemName) >= _maxAllowedItemCount[itemName])
			{
				return null;
			}

			Item itemInstance = ItemLoader.GetItem(itemName);
			var tile = _map[point.X, point.Y];
			tile.MapObject = new MapObject(ObjectType.Collectible, 0)
			{
				Item = itemInstance,
			};
			ItemManager.AddItem(itemInstance);
			return itemInstance;
		}
		
		private Creature PlaceCreature(string creatureName, Point point)
		{
			Creature creatureInstance = CreatureLoader.GetCreature(creatureName);
			creatureInstance.MapIndex = point;
			creatureInstance.Position = creatureInstance.MapIndex.ToVector2() * Game.Options.TileSize;
			var tile = _map[point.X, point.Y];
			tile.CollisionFlag = true;
			Game.CreatureManager.AddCreature(creatureInstance);
			return creatureInstance;
		}

		private Structure PlaceStructure(string structureName, Point point)
		{
			var tile = _map[point.X, point.Y];
			Structure structureInstance = StructureLoader.GetStructure(structureName);
			structureInstance.Tile = new TileInfo(point, tile);
			tile.MapObject = new MapObject(ObjectType.Interactive, 0)
			{
				Structure = structureInstance,
			};
			tile.CollisionFlag = true;
			structureInstance.BindBehaviours();
			StructureManager.AddStructure(structureInstance);
			return structureInstance;
		}

		private List<Point> FloodFillRoom()
		{
			Stack<Point> openList = new Stack<Point>();
			List<Point> closedList = new List<Point>();

			for (int X = 0; X < _map.GetLength(0); X++)
			{
				for (int Y = 0; Y < _map.GetLength(1); Y++)
				{
					if (_operatingArea[X, Y] == 0 && _map[X, Y].BaseType == 0 && _map[X, Y].BaseColor == Color.White)
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
					if (_map[neighbour.X, neighbour.Y].BaseType == 0 && _operatingArea[neighbour.X, neighbour.Y] == 0 && _map[neighbour.X, neighbour.Y].BaseColor == Color.White)
					{
						openList.Push(neighbour);
					}
					_operatingArea[neighbour.X, neighbour.Y] = 1;
				}

				closedList.Add(currentTile);
				_operatingArea[currentTile.X, currentTile.Y] = 1;
			}

			return closedList;
		}

        private void MakeRooms(List<Point> usedTiles)
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
                    if (_map[x, y] != null && _map[x, y].BaseColor != Color.White) continue;

                    var pixelColor = System.Drawing.Color.FromArgb(output[x, y]).ToXnaColor();
                    var tileType = sample.TileColorLegend[pixelColor];

                    if (tileType == TileType.Empty)
                    {
                        _map[x, y] = new Tile(0, 0, false);
                    }
                    else if (tileType == TileType.Wall)
                    {
                        _map[x, y] = new Tile(1, 9, true);
                    }
                    else if (tileType == TileType.Floor)
                    {
                        _map[x, y] = new Tile(0, 0, false);
                    }
                    else if (tileType == TileType.Path)
                    {
                        _map[x, y] = new Tile(0, 0, false);
                    }
                    else if (tileType == TileType.Mud)
                    {
                        _map[x, y] = new Tile(0, 0, false);
                    }

                    if (x == 0 || y == 0 || x == WorldOptions.Width - 1 || y == WorldOptions.Height - 1)
                        _map[x, y] = new Tile(1, 9, true);

					// play main music theme
					_map[x, y].CreatureCame += incomingCreature =>
					{
						if(!incomingCreature.IsPossessedCreature())
						{
							return;
						}
						SoundManager.PlayMusic("Main_theme");
					};
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

                    randomLocationX = _rng.Next(1, WorldOptions.Width - regInfo.TileInfo.GetLength(0) - 1);
                    randomLocationY = _rng.Next(1, WorldOptions.Height - regInfo.TileInfo.GetLength(1) - 1);

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
                        Game.CreatureManager.Creatures.Clear(); // first clear everything
                        LightingManager.ClearLightSources();
                        ParticleSystemManager.Clear();
                        usedTiles.Clear();
                        filledSpace.Clear();
                        _map.Populate(() => null);
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
                    creatureClone.Position = creatureClone.MapIndex.ToVector2() * Game.Options.TileSize;
                    Game.CreatureManager.AddCreature(creatureClone);
                }

                regInfo.TileInfo.CopyTo(_map, randomLocationX, randomLocationY);
            }
        }

        private RegMapInfo GenerateRegion(CustomRegion region)
		{
			var info = new RegMapInfo();

			List<DimensionAreaInfo> areas = new List<DimensionAreaInfo>();

			foreach (var dimension in region.DimensionSpecifiers)
			{
				var bounds = dimension.GetAreaData(_rng);
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
							Modifier = TileModifier.Sticky,
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

			//
			List<Point> entrances = new();

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
			if(region.Name == "FinalRoom")
			{
				// place a corridor and 3 red doors blocking the entrance
				var entrancePoint = entrances[0];
				for (int i = 0; i < 3; i++)
				{
					var doorPoint = new Point(entrancePoint.X, entrancePoint.Y + i + 1);
					var leftWallPoint = new Point(doorPoint.X - 1, doorPoint.Y);
					var rightWallPoint = new Point(doorPoint.X + 1, doorPoint.Y);

					var doorInstance = StructureLoader.GetStructure("RedDoor");
					doorInstance.Tile = new TileInfo(doorPoint, tileData[doorPoint.X, doorPoint.Y]);
					StructureManager.AddStructure(doorInstance);
					tileData[doorPoint.X, doorPoint.Y].MapObject = new MapObject(ObjectType.Interactive, 0)
					{
						Structure = doorInstance,
					};
					tileData[doorPoint.X, doorPoint.Y].CollisionFlag = true;

					tileData[leftWallPoint.X, leftWallPoint.Y].BaseType = 1;
					tileData[leftWallPoint.X, leftWallPoint.Y].CollisionFlag = true;
					tileData[leftWallPoint.X, leftWallPoint.Y].BaseColor = region.BorderColor;

					tileData[rightWallPoint.X, rightWallPoint.Y].BaseType = 1;
					tileData[rightWallPoint.X, rightWallPoint.Y].CollisionFlag = true;
					tileData[rightWallPoint.X, rightWallPoint.Y].BaseColor = region.BorderColor;
				}
			}			

			//
			info.TileInfo = tileData;

			//
			if(!string.IsNullOrEmpty(region.MusicThemeName))
			{
				foreach (var tile in info.TileInfo)
				{
					tile.CreatureCame += incomingCreature =>
					{
						if(!incomingCreature.IsPossessedCreature())
						{
							return;
						}

						SoundManager.PlayMusic(region.MusicThemeName);
					};
				}
			}

			//

			void recursiveSetPositions(DimensionAreaInfo nextSpec)
			{
				if (nextSpec.Spec.Parent != null)
				{
					var placement = nextSpec.Spec.PossiblePlacements.GetRandomElement(_rng);
					var parent = areas.Find(i => i.Spec == nextSpec.Spec.Parent);
					var realOffsetX = nextSpec.Spec.OffsetX.GetRandom(_rng);
					var realOffsetY = nextSpec.Spec.OffsetY.GetRandom(_rng);

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
				float randomChance = (float)_rng.NextDouble();
				if (randomChance >= node.Chance)
				{
					return;
				}

				if (node.Type == NodeType.Choice)
				{
					int rand = _rng.Next(node.ChildNodes.Sum(x => x.Weight));
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
					var number = entitySpec.Number.GetRandom(_rng);

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
				Point finalPosition = availableTiles.GetRandomElement(_rng);
				if (entry.Node.Type == NodeType.Entrance)
					finalPosition = availableBorders.GetRandomElement(_rng);

				var farParents = entry.Parents.Where(x => x.Value.Distance == DistanceDescription.Far || x.Value.Distance == DistanceDescription.VeryFar || x.Value.Distance == DistanceDescription.FarthestAway);
				var closeParents = entry.Parents.Where(x => x.Value.Distance == DistanceDescription.Close || x.Value.Distance == DistanceDescription.VeryClose);

				var closeParentsNoDuplicates = closeParents.GroupBy(x => x.Key.Spec.Name).Select(x => x.ToList().GetRandomElement(_rng));
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

						var randomColor = new Color(_rng.Next(256), _rng.Next(256), _rng.Next(256));
						foreach (var point in lastIntersection)
						{
							//tileData[point.X, point.Y].BaseColor = i == bounds.Count - 1 ? Color.Yellow : randomColor;
						}
					}

					finalPosition = lastIntersection.GetRandomElement(_rng);
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
				var number = entitySpec.Number.GetRandom(_rng);

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
					StructureManager.AddStructure(structureInstance);
					if (entry.Inventory.Count > 0)
					{
						Container container = structureInstance.GetBehaviour<Container>();
						container.Inventory.AddElements(entry.Inventory);
					}

					tile.MapObject = new MapObject(ObjectType.Interactive, 0)
					{
						Structure = structureInstance,
					};
					tile.CollisionFlag = true;
					if(tile.TooltipInfo == null)
					{
						// tile.TooltipInfo = new ImageTooltip
						// {
						// 	Title = entitySpec.Name,
						// 	Image = TilesheetManager.MapSpritesheetTexture.ExtractSubtexture(TilesheetManager.MapObjectSpritesheet[structureInstance.Texture].Rect, Game1.game.GraphicsDevice),
						// };
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
					tile.SetTexture(0);
					tile.CollisionFlag = false;
					tile.BaseColor = region.FloorColor;
					entrances.Add(finalPosition);
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

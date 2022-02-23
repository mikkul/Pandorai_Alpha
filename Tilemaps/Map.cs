using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Pandorai.Sprites;
using System.Collections;
using System.Linq;
using Pandorai.Items;
using Myra.Graphics2D.UI.Properties;
using Pandorai.Utility;

namespace Pandorai.Tilemaps
{
	public delegate void TileEventHandler(TileInfo info);
	public delegate void ActiveMapSwitchHandler(ActiveMap level);
	public delegate void TileWithItemHandler(TileInfo tileInfo, Item item);

	public enum ActiveMap
	{
		Surface,
		Underground
	}

	public class Map
	{
		public event ActiveMapSwitchHandler ActiveMapSwitched;

		public Tile[,] Tiles;

		public Tile[,] SurfaceTiles;
		public Tile[,] UndergroundTiles;

		public ActiveMap ActiveMap = ActiveMap.Surface;

		public int[,] Tilemap;

		public Vector2 OffsetToMiddle;

		public Point CenterTile;

		public Vector2 TileOffset;

		public Color DefaultTileColor;

		public int TileSize;

		public bool AreTilesInteractive = false;

		RenderTarget2D renderTarget;

		int tilesRenderedHorizontally;
		int tilesRenderedVertically;

		SpriteBatch spriteBatch;

		Vector2 oldReferencePoint;

		Dictionary<Point, bool> tileCollisionFlagChangeQueue = new Dictionary<Point, bool>();
		Dictionary<Point, bool> tileIgnoreFlagChangeQueue = new Dictionary<Point, bool>();

		List<Point> highlightedTiles = new List<Point>();
		Point lastHoveredOverTile = Point.Zero;

		Color mouseHighlightColor = Color.White * 0.1f;
		Color mouseHiglightDecalColor = Color.Black * 0.1f;
		Color mouseInteractDecalColor = Color.Red * 0.5f;

		Game1 game;

		public Map(SpriteBatch batch, Game1 _game)
		{
			spriteBatch = batch;
			game = _game;
			TileOffset = Vector2.Zero;
			CenterTile = Point.Zero;
			DefaultTileColor = Color.White;
			renderTarget = new RenderTarget2D(game.GraphicsDevice, game.Camera.Viewport.Width, game.Camera.Viewport.Height);
		}

		public void RefreshRenderTarget()
		{
			renderTarget = new RenderTarget2D(game.GraphicsDevice, game.Camera.Viewport.Width, game.Camera.Viewport.Height);
		}

		public void UpdateMapRenderingOptions(int oldSize, int newSize)
		{
			TileOffset = Vector2.Zero;
			CenterTile = Point.Zero;
			TileSize = newSize;
			SetAmountTilesRendered(game.Camera.Viewport.Width / TileSize / 2 + 2, game.Camera.Viewport.Height / TileSize / 2 + 2);
			OffsetToMiddle = new Vector2(game.Camera.Viewport.X + game.Camera.Viewport.Width / 2 - TileSize / 2, game.Camera.Viewport.Y + game.Camera.Viewport.Height / 2 - TileSize / 2);
			oldReferencePoint = Vector2.Zero;
			//game.Options.ApplyChanges();
		}

		public Tile GetTile(Point index)
		{
			if (Tiles.IsPointInBounds(index))
				return Tiles[index.X, index.Y];
			else return null;
		}

		public Tile GetTile(int x, int y)
		{
			if (Tiles.IsPointInBounds(x, y))
				return Tiles[x, y];
			else return null;
		}

		public void SwitchActiveMap(ActiveMap level)
		{
			ActiveMap = level;
			if(ActiveMap == ActiveMap.Surface)
			{
				Tiles = SurfaceTiles;
			}
			else if(ActiveMap == ActiveMap.Underground)
			{
				Tiles = UndergroundTiles;
			}

			ActiveMapSwitched?.Invoke(ActiveMap);
		}

		public void InteractWithMapObjects(TileInfo info)
		{
			if (!game.Player.HoldingShift) return;

			float distToClickedTile = (game.Player.PossessedCreature.MapIndex - info.Index).ToVector2().LengthSquared();
			if (distToClickedTile <= 2)
			{
				info.Tile.MapObject?.Structure?.Interact(game.Player.PossessedCreature);
				game.CreatureManager.GetCreature(info.Index)?.Interact(game.Player.PossessedCreature);
				game.TurnManager.SkipHeroTurn();
			}
		}

		public void HandleItemRelease(TileInfo tile, Item usedItem)
		{
			tile.Tile.OnItemUsed(usedItem);
		}

		public void HighlightHoveredTile(TileInfo info)
		{
			if(!AreTilesInteractive)
			{
				Tiles[lastHoveredOverTile.X, lastHoveredOverTile.Y].IsHighlighted = false;
				Tiles[lastHoveredOverTile.X, lastHoveredOverTile.Y].IsDecal = false;
				info.Tile.IsHighlighted = true;
				info.Tile.IsDecal = true;
				info.Tile.HighlightColor = mouseHighlightColor;

				if (game.Player.HoldingShift && !game.Player.HoldingControl)
				{
					info.Tile.DecalColor = mouseInteractDecalColor;
				}
				else
				{
					info.Tile.DecalColor = mouseHiglightDecalColor;
				}

				lastHoveredOverTile = info.Index;
			}
			else
			{
				Tiles[lastHoveredOverTile.X, lastHoveredOverTile.Y].DecalColor = Color.LightGreen;

				if (highlightedTiles.Contains(info.Index))
				{
					info.Tile.DecalColor = Color.Yellow;
				}
			}
		}

		public void GenerateTilesFromTilemap()
		{
			Tiles = new Tile[Tilemap.GetLength(0), Tilemap.GetLength(1)];

			for (int x = 0; x < Tiles.GetLength(0); x++)
			{
				for (int y = 0; y < Tiles.GetLength(1); y++)
				{
					bool collisionFlag = Tilemap[x, y] == 1;

					int tileTex = 9;

					if(collisionFlag)
					{
						bool topLeftNeighbour     = x - 1 >= 0 && y - 1 >= 0 ? Tilemap[x - 1, y - 1] == 1 : false;
						bool topNeighbour         = y - 1 >= 0 ? Tilemap[x    , y - 1] == 1 : false;
						bool topRightNeighbour    = x + 1 < Tiles.GetLength(0) && y - 1 >= 0 ? Tilemap[x + 1, y - 1] == 1 : false;
						bool leftNeighbour        = x - 1 >= 0 ? Tilemap[x - 1, y    ] == 1 : false;
						bool rightNeighbour       = x + 1 < Tiles.GetLength(0) ? Tilemap[x + 1, y    ] ==  1 : false;
						bool bottomLeftNeighbour  = x - 1 >= 0 && y + 1 < Tiles.GetLength(1) ? Tilemap[x - 1, y + 1] == 1 : false;
						bool bottomNeighbour      = y + 1 < Tiles.GetLength(1) ? Tilemap[x    , y + 1] == 1 : false;
						bool bottomRightNeighbour = x + 1 < Tiles.GetLength(0) && y + 1 < Tiles.GetLength(1) ? Tilemap[x + 1, y + 1] == 1 : false;

						bool[] boolFlags = new bool[] { topLeftNeighbour, topNeighbour, topRightNeighbour, leftNeighbour, rightNeighbour, bottomLeftNeighbour, bottomNeighbour, bottomRightNeighbour };
						BitArray bitArray = new BitArray(boolFlags);
						BitArray reversedBitArray = new BitArray(bitArray.Cast<bool>().Reverse().ToArray());
						byte[] bytes = new byte[1];
						reversedBitArray.CopyTo(bytes, 0);

						byte flag = bytes[0];

						if ((flag & ~0b101_00_101) == 0b000_00_000) tileTex = 9;
						if ((flag & ~0b101_00_101) == 0b000_01_000) tileTex = 10;
						if ((flag & ~0b101_00_101) == 0b000_11_000) tileTex = 11;
						if ((flag & ~0b101_00_101) == 0b000_10_000) tileTex = 12;
						if ((flag & ~0b101_00_101) == 0b000_00_010) tileTex = 13;
						if ((flag & ~0b101_00_101) == 0b010_00_010) tileTex = 14;
						if ((flag & ~0b101_00_101) == 0b010_00_000) tileTex = 15;
						if ((flag & ~0b101_00_100) == 0b000_01_011) tileTex = 16;
						if ((flag & ~0b101_00_000) == 0b000_11_111) tileTex = 17;
						if ((flag & ~0b101_00_001) == 0b000_10_110) tileTex = 18;
						if ((flag & ~0b100_00_100) == 0b011_01_011) tileTex = 19;
						if ((flag & ~0b000_00_000) == 0b111_11_111) tileTex = 20;
						if ((flag & ~0b001_00_001) == 0b110_10_110) tileTex = 21;
						if ((flag & ~0b100_00_101) == 0b011_01_000) tileTex = 22;
						if ((flag & ~0b000_00_101) == 0b111_11_000) tileTex = 23;
						if ((flag & ~0b001_00_101) == 0b110_10_000) tileTex = 24;
						if ((flag & ~0b101_00_100) == 0b000_01_010) tileTex = 25;
						if ((flag & ~0b001_00_101) == 0b010_10_000) tileTex = 26;
						if ((flag & ~0b101_00_001) == 0b000_10_010) tileTex = 27;
						if ((flag & ~0b100_00_101) == 0b010_01_000) tileTex = 28;
						if ((flag & ~0b001_00_001) == 0b010_10_010) tileTex = 29;
						if ((flag & ~0b101_00_000) == 0b000_11_010) tileTex = 30;
						if ((flag & ~0b000_00_101) == 0b010_11_000) tileTex = 31;
						if ((flag & ~0b100_00_100) == 0b010_01_010) tileTex = 32;
						if ((flag & ~0b000_00_000) == 0b010_11_010) tileTex = 33;
						if ((flag & ~0b000_00_000) == 0b011_11_010) tileTex = 34;
						if ((flag & ~0b000_00_000) == 0b010_11_011) tileTex = 35;
						if ((flag & ~0b000_00_000) == 0b110_11_010) tileTex = 36;
						if ((flag & ~0b000_00_000) == 0b010_11_110) tileTex = 37;
						if ((flag & ~0b000_00_000) == 0b000_11_011) tileTex = 38;
						if ((flag & ~0b000_00_000) == 0b000_11_110) tileTex = 39;
						if ((flag & ~0b000_00_000) == 0b011_11_000) tileTex = 40;
						if ((flag & ~0b000_00_000) == 0b110_11_000) tileTex = 41;
						if ((flag & ~0b000_00_000) == 0b010_10_110) tileTex = 42;
						if ((flag & ~0b000_00_000) == 0b010_01_011) tileTex = 43;
						if ((flag & ~0b000_00_000) == 0b110_10_010) tileTex = 44;
						if ((flag & ~0b000_00_000) == 0b011_01_010) tileTex = 45;
						if ((flag & ~0b000_00_000) == 0b011_11_110) tileTex = 46;
						if ((flag & ~0b000_00_000) == 0b110_11_011) tileTex = 47;
						if ((flag & ~0b000_00_000) == 0b111_11_011) tileTex = 48;
						if ((flag & ~0b000_00_000) == 0b111_11_110) tileTex = 49;
						if ((flag & ~0b000_00_000) == 0b011_11_111) tileTex = 50;
						if ((flag & ~0b000_00_000) == 0b110_11_111) tileTex = 51;
						if ((flag & ~0b000_00_000) == 0b111_11_010) tileTex = 52;
						if ((flag & ~0b000_00_000) == 0b010_11_111) tileTex = 53;
						if ((flag & ~0b000_00_000) == 0b011_11_011) tileTex = 54;
						if ((flag & ~0b000_00_000) == 0b110_11_110) tileTex = 55;
					}
					else
					{
						tileTex = 0;
					}

					Tiles[x, y] = new Tile(Tilemap[x, y], tileTex, collisionFlag);
				}
			}
		}

		public void UpdateTileTextures()
		{
			for (int x = 0; x < Tiles.GetLength(0); x++)
			{
				for (int y = 0; y < Tiles.GetLength(1); y++)
				{
					bool isWall = Tiles[x, y].BaseType == 1;

					int tileTex = 9;

					if (isWall)
					{
						bool topLeftNeighbour = x - 1 >= 0 && y - 1 >= 0 ? Tiles[x - 1, y - 1].BaseType == 1 : false;
						bool topNeighbour = y - 1 >= 0 ? Tiles[x, y - 1].BaseType == 1 : false;
						bool topRightNeighbour = x + 1 < Tiles.GetLength(0) && y - 1 >= 0 ? Tiles[x + 1, y - 1].BaseType == 1 : false;
						bool leftNeighbour = x - 1 >= 0 ? Tiles[x - 1, y].BaseType == 1 : false;
						bool rightNeighbour = x + 1 < Tiles.GetLength(0) ? Tiles[x + 1, y].BaseType == 1 : false;
						bool bottomLeftNeighbour = x - 1 >= 0 && y + 1 < Tiles.GetLength(1) ? Tiles[x - 1, y + 1].BaseType == 1 : false;
						bool bottomNeighbour = y + 1 < Tiles.GetLength(1) ? Tiles[x, y + 1].BaseType == 1 : false;
						bool bottomRightNeighbour = x + 1 < Tiles.GetLength(0) && y + 1 < Tiles.GetLength(1) ? Tiles[x + 1, y + 1].BaseType == 1 : false;

						bool[] boolFlags = new bool[] { topLeftNeighbour, topNeighbour, topRightNeighbour, leftNeighbour, rightNeighbour, bottomLeftNeighbour, bottomNeighbour, bottomRightNeighbour };
						BitArray bitArray = new BitArray(boolFlags);
						BitArray reversedBitArray = new BitArray(bitArray.Cast<bool>().Reverse().ToArray());
						byte[] bytes = new byte[1];
						reversedBitArray.CopyTo(bytes, 0);

						byte flag = bytes[0];

						if ((flag & ~0b101_00_101) == 0b000_00_000) tileTex = 9;
						if ((flag & ~0b101_00_101) == 0b000_01_000) tileTex = 10;
						if ((flag & ~0b101_00_101) == 0b000_11_000) tileTex = 11;
						if ((flag & ~0b101_00_101) == 0b000_10_000) tileTex = 12;
						if ((flag & ~0b101_00_101) == 0b000_00_010) tileTex = 13;
						if ((flag & ~0b101_00_101) == 0b010_00_010) tileTex = 14;
						if ((flag & ~0b101_00_101) == 0b010_00_000) tileTex = 15;
						if ((flag & ~0b101_00_100) == 0b000_01_011) tileTex = 16;
						if ((flag & ~0b101_00_000) == 0b000_11_111) tileTex = 17;
						if ((flag & ~0b101_00_001) == 0b000_10_110) tileTex = 18;
						if ((flag & ~0b100_00_100) == 0b011_01_011) tileTex = 19;
						if ((flag & ~0b000_00_000) == 0b111_11_111) tileTex = 20;
						if ((flag & ~0b001_00_001) == 0b110_10_110) tileTex = 21;
						if ((flag & ~0b100_00_101) == 0b011_01_000) tileTex = 22;
						if ((flag & ~0b000_00_101) == 0b111_11_000) tileTex = 23;
						if ((flag & ~0b001_00_101) == 0b110_10_000) tileTex = 24;
						if ((flag & ~0b101_00_100) == 0b000_01_010) tileTex = 25;
						if ((flag & ~0b001_00_101) == 0b010_10_000) tileTex = 26;
						if ((flag & ~0b101_00_001) == 0b000_10_010) tileTex = 27;
						if ((flag & ~0b100_00_101) == 0b010_01_000) tileTex = 28;
						if ((flag & ~0b001_00_001) == 0b010_10_010) tileTex = 29;
						if ((flag & ~0b101_00_000) == 0b000_11_010) tileTex = 30;
						if ((flag & ~0b000_00_101) == 0b010_11_000) tileTex = 31;
						if ((flag & ~0b100_00_100) == 0b010_01_010) tileTex = 32;
						if ((flag & ~0b000_00_000) == 0b010_11_010) tileTex = 33;
						if ((flag & ~0b000_00_000) == 0b011_11_010) tileTex = 34;
						if ((flag & ~0b000_00_000) == 0b010_11_011) tileTex = 35;
						if ((flag & ~0b000_00_000) == 0b110_11_010) tileTex = 36;
						if ((flag & ~0b000_00_000) == 0b010_11_110) tileTex = 37;
						if ((flag & ~0b101_00_000) == 0b000_11_011) tileTex = 38;
						if ((flag & ~0b101_00_000) == 0b000_11_110) tileTex = 39;
						if ((flag & ~0b000_00_101) == 0b011_11_000) tileTex = 40;
						if ((flag & ~0b000_00_101) == 0b110_11_000) tileTex = 41;
						if ((flag & ~0b001_00_001) == 0b010_10_110) tileTex = 42;
						if ((flag & ~0b100_00_100) == 0b010_01_011) tileTex = 43;
						if ((flag & ~0b001_00_001) == 0b110_10_010) tileTex = 44;
						if ((flag & ~0b100_00_100) == 0b011_01_010) tileTex = 45;
						if ((flag & ~0b000_00_000) == 0b011_11_110) tileTex = 46;
						if ((flag & ~0b000_00_000) == 0b110_11_011) tileTex = 47;
						if ((flag & ~0b000_00_000) == 0b111_11_011) tileTex = 48;
						if ((flag & ~0b000_00_000) == 0b111_11_110) tileTex = 49;
						if ((flag & ~0b000_00_000) == 0b011_11_111) tileTex = 50;
						if ((flag & ~0b000_00_000) == 0b110_11_111) tileTex = 51;
						if ((flag & ~0b000_00_000) == 0b111_11_010) tileTex = 52;
						if ((flag & ~0b000_00_000) == 0b010_11_111) tileTex = 53;
						if ((flag & ~0b000_00_000) == 0b011_11_011) tileTex = 54;
						if ((flag & ~0b000_00_000) == 0b110_11_110) tileTex = 55;

						Tiles[x, y].SetTexture(tileTex);
					}
				}
			}
		}

		public void SetAmountTilesRendered(int horizontal, int vertical)
		{
			tilesRenderedHorizontally = horizontal;
			tilesRenderedVertically = vertical;
		}

		public void InitScrollPosition(Camera camera)
		{
			oldReferencePoint = camera.Position;
			CenterTile = new Point((int)(((camera.Position.X + camera.Viewport.Width) / 2) / TileSize), (int)(((camera.Position.Y + camera.Viewport.Height) / 2) / TileSize));
		}

		public void ClearTileChanges()
		{
			tileCollisionFlagChangeQueue.Clear();
			tileIgnoreFlagChangeQueue.Clear();
		}

		public void HighlightTiles(Rectangle area)
		{
			UnHighlightTiles();

			int highlightedAreaWidth = area.Right - area.Left;
			int highlightedAreaHeight = area.Bottom - area.Top;

			for (int x = 0; x < highlightedAreaWidth; x++)
			{
				for (int y = 0; y < highlightedAreaHeight; y++)
				{
					if (area.Left + x < 0 || area.Left + x > Tiles.GetLength(0) - 1 || area.Top + y < 0 || area.Top + y > Tiles.GetLength(1) - 1) continue;

					Point pos = new Point(area.Left + x, area.Top + y);
					Tiles[pos.X, pos.Y].IsDecal = true;
					Tiles[pos.X, pos.Y].DecalColor = Color.LightGreen;
					highlightedTiles.Add(pos);
				}
			}
		}

		public void HighlightTiles(IEnumerable<Point> points)
		{
			UnHighlightTiles();
			foreach (var point in points)
			{
				if (!Tiles.IsPointInBounds(point)) continue;

				Tiles[point.X, point.Y].IsDecal = true;
				Tiles[point.X, point.Y].DecalColor = Color.LightGreen;
				highlightedTiles.Add(point);
			}
		}

		public void UnHighlightTiles()
		{
			for (int i = 0; i < highlightedTiles.Count; i++)
			{
				Tiles[highlightedTiles[i].X, highlightedTiles[i].Y].IsDecal = false;
			}

			highlightedTiles.Clear();
		}

		public void EnableTileInteraction()
		{
			AreTilesInteractive = true;
		}

		public void DisableTileInteraction()
		{
			AreTilesInteractive = false;
			UnHighlightTiles();

			// unbind handlers
			//TileInteractionManager.TileClick -= TransformSpell.UseHandler;
		}

		public void RequestTileCollisionFlagChange(Point tile, bool flag)
		{
			Tiles[tile.X, tile.Y].CollisionFlag = flag;
		}

		public void RequestTileIgnoreFlagChange(Point tile, bool flag)
		{
			Tiles[tile.X, tile.Y].IgnoreCollisionFlagOnSearch = flag;
		}

		public void ClearCollisionFlags()
		{
			foreach (var tile in Tiles)
			{
				if(tile.BaseType == 0)
				{
					tile.CollisionFlag = false;
				}
				else if(tile.BaseType == 1)
				{
					tile.CollisionFlag = true;
				}
			}
		}

		public Point GetTileIndexByPosition(Vector2 position)
		{
			int indexX = (int)Math.Ceiling((position.X - TileSize / 2) / TileSize);
			int indexY = (int)Math.Ceiling((position.Y - TileSize / 2) / TileSize);

			if(indexX < 0 || indexY < 0 || indexX > Tiles.GetLength(0) - 1 || indexY > Tiles.GetLength(1) - 1)
			{
				Console.WriteLine($"Can't read tile index from this position: X: {indexX} Y: {indexY}");
				return Point.Zero;
			}

			return new Point(indexX, indexY);
		}

		public Vector2 SnapToGrid(Vector2 position)
		{
			return new Vector2((float)Math.Round(position.X / TileSize) * TileSize, (float)Math.Round(position.Y / TileSize) * TileSize);
		}

		public void UpdateScroll(Vector2 referencePoint)
		{
			TileOffset += oldReferencePoint - referencePoint;

			//Console.WriteLine((oldReferencePoint).ToString());

			while (TileOffset.X < 0)
			{
				TileOffset.X += TileSize;
				CenterTile.X++;
			}
			while (TileOffset.X > TileSize)
			{
				TileOffset.X -= TileSize;
				CenterTile.X--;
			}
			while (TileOffset.Y < 0)
			{
				TileOffset.Y += TileSize;
				CenterTile.Y++;
			}
			while (TileOffset.Y > TileSize)
			{
				TileOffset.Y -= TileSize;
				CenterTile.Y--;
			}

			oldReferencePoint = referencePoint;
		}

		public void ClearAllTiles()
		{
			foreach (Tile tile in Tiles)
			{
				tile.MapObject = null;
			}
		}

		public RenderTarget2D Render()
		{
			Vector2 finalTileOffset = TileOffset + OffsetToMiddle;
			int currentTileX;
			int currentTileY;
			Vector2 tilePosition;

			game.GraphicsDevice.SetRenderTarget(renderTarget);

			spriteBatch.Begin(blendState: BlendState.AlphaBlend);

			for (int y = -tilesRenderedVertically; y < tilesRenderedVertically; y++)
			{
				if(y == -tilesRenderedVertically)
				{
					//Console.WriteLine(CenterTile.ToString());
				}
				currentTileY = CenterTile.Y + y;
				if(currentTileY < 0 || currentTileY >= Tiles.GetLength(1))
				{
					continue;
				}

				for (int x = -tilesRenderedHorizontally; x < tilesRenderedHorizontally; x++)
				{
					currentTileX = CenterTile.X + x;
					if (currentTileX < 0 || currentTileX >= Tiles.GetLength(0))
					{
						continue;
					}

					Point tileIndex = new Point(currentTileX, currentTileY);
					Tile tile = GetTile(tileIndex);

					bool isVisible = false;
					if (game.Player.TilesInFOV.Contains(tileIndex))
					{
						isVisible = true;
					}
					bool isBlackout = true;
					if (tile.Visited)
					{
						isBlackout = false;
					}

					tilePosition = new Vector2(x * TileSize + finalTileOffset.X, y * TileSize + finalTileOffset.Y);

					if(!isBlackout)
					{
						for (int i = 0; i < tile.TextureIndices.Count; i++)
						{
							Color tileColor = i == 0 ? tile.BaseColor : Color.White;
							spriteBatch.Draw(TilesheetManager.MapSpritesheetTexture, new Rectangle(tilePosition.ToPoint(), new Point(TileSize, TileSize)), TilesheetManager.MapObjectSpritesheet[tile.TextureIndices[i]].Rect, tileColor);
						}

						if (tile.MapObject != null)
						{
							if (tile.MapObject.Item != null)
							{
								spriteBatch.Draw(TilesheetManager.MapSpritesheetTexture, new Rectangle(tilePosition.ToPoint(), new Point(TileSize, TileSize)), TilesheetManager.MapObjectSpritesheet[tile.MapObject.Item.Texture].Rect, tile.MapObject.Item.ColorTint);
							}
							else if (tile.MapObject.Structure != null)
							{
								spriteBatch.Draw(TilesheetManager.MapSpritesheetTexture, new Rectangle(tilePosition.ToPoint(), new Point(TileSize, TileSize)), TilesheetManager.MapObjectSpritesheet[tile.MapObject.Structure.Texture].Rect, tile.MapObject.Structure.ColorTint);
							}
						}

						if(!isVisible)
						{
							spriteBatch.Draw(game.squareTexture, new Rectangle(tilePosition.ToPoint(), new Point(TileSize)), Color.Black * game.Options.TilesOutsideFOVDarkening);
						}						
					}

					if(tile.IsDecal)
					{
						spriteBatch.Draw(TilesheetManager.MapSpritesheetTexture, new Rectangle(tilePosition.ToPoint(), new Point(TileSize, TileSize)), TilesheetManager.MapObjectSpritesheet[8].Rect, tile.DecalColor);
					}
					if(tile.IsHighlighted)
					{
						spriteBatch.Draw(game.squareTexture, new Rectangle(tilePosition.ToPoint() + new Point(1), new Point(TileSize - 2)), tile.HighlightColor);
					}
					// used to debug stuff
					// if(tile.HasStructure() && tile.MapObject.Structure.Id == "Teleporter")
					// {
					// 	spriteBatch.Draw(game.squareTexture, new Rectangle(tilePosition.ToPoint() + new Point(1), new Point(TileSize - 2)), Color.Yellow * 0.25f);
					// }
				}
			}

			spriteBatch.End();

			game.GraphicsDevice.SetRenderTarget(null);

			return renderTarget;
		}
	}
}

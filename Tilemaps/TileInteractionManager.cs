using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Utility;

namespace Pandorai.Tilemaps
{
	public static class TileInteractionManager
	{
		public static event TileEventHandler TileHover;
		public static event TileEventHandler TileClick;
		public static event TileEventHandler TileRightClick;
		public static event TileWithItemHandler TileItemReleased;

		public static event MovementRequestHandler AcceptedMovement;
		public static event CreatureOnTileHandler RunIntoCollision;

		public static void CheckMovementPosibility(Creature creature, Point desiredPoint)
		{
			if(!Main.Game.Map.Tiles.IsPointInBounds(desiredPoint))
			{
				return;
			}

			if(!creature.NoClip)
			{
				if(!Main.Game.Map.Tiles[desiredPoint.X, desiredPoint.Y].CollisionFlag)
				{
					bool isDiagonal = creature.MapIndex.X - desiredPoint.X != 0 && creature.MapIndex.Y - desiredPoint.Y != 0;

					if(!Main.Game.Map.GetTile(creature.MapIndex).Modifier.HasFlag(TileModifier.Sticky) || !isDiagonal)
					{
						AcceptedMovement?.Invoke(creature, desiredPoint);
					}
				}
				else
				{
					RunIntoCollision?.Invoke(creature, new TileInfo(desiredPoint, Main.Game.Map.Tiles[desiredPoint.X, desiredPoint.Y]));
				}
			}
			else
			{
				AcceptedMovement?.Invoke(creature, desiredPoint);
			}
		}

		public static void HandleMouseHover(Vector2 mousePos)
		{
			if (!Main.Game.IsGameStarted || Main.Game.IsGamePaused) return;

			TileHover?.Invoke(GetTileInfoByMousePos(mousePos));
		}

		public static void HandleLMBClick(Vector2 mousePos)
		{
			if (!Main.Game.IsGameStarted || Main.Game.IsGamePaused) return;

			TileClick?.Invoke(GetTileInfoByMousePos(mousePos));
		}

		public static void HandleRMBClick(Vector2 mousePos)
		{
			if (!Main.Game.IsGameStarted || Main.Game.IsGamePaused) return;

			TileRightClick?.Invoke(GetTileInfoByMousePos(mousePos));
		}
		
		public static void OnItemRelease(Items.Item releasedItem)
		{
			if (!Main.Game.IsGameStarted || Main.Game.IsGamePaused) return;

			if (releasedItem == null) return;

			TileItemReleased?.Invoke(GetTileInfoByMousePos(Main.Game.InputManager.MousePos), releasedItem);
		}

		public static void EmulateTileClick(Point index)
		{
			if (!Main.Game.IsGameStarted || Main.Game.IsGamePaused) return;

			TileClick?.Invoke(new TileInfo(index, Main.Game.Map.Tiles[index.X, index.Y]));
		}

		private static TileInfo GetTileInfoByMousePos(Vector2 pos)
		{
			Point index = Main.Game.Map.GetTileIndexByPosition(Main.Game.Camera.Position + pos);
			return new TileInfo(index, Main.Game.Map.Tiles[index.X, index.Y]);
		}
	}
}

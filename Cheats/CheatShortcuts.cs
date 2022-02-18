using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using Pandorai.Tilemaps;

namespace Pandorai.Cheats
{
	public static class CheatShortcuts
	{
		public static Game1 game;

		public static bool Activated = false;

		public static void HandleKeyInput(Keys key)
		{
			if (!Activated) return;

			if(key == Keys.T)
			{
				TeleportToNextRegion();
			}
		}

		public static void HandleTileHover(TileInfo info)
		{
			if (!Activated) return;

			if (game.Player.HoldingControl)
			{
				Label textLabel = (Label)game.desktop.Root.FindWidgetById("DialogueTextLabel");
				textLabel.Text = $"X: {info.Index.X}, Y: {info.Index.Y}";
			}
		}

		public static void HandleTileClick(TileInfo info)
		{
			if (!Activated) return;

			if (game.Player.HoldingControl)
			{
				if(game.Player.HoldingShift)
				{
					game.Map.Tiles[game.Player.PossessedCreature.MapIndex.X, game.Player.PossessedCreature.MapIndex.Y].CollisionFlag = false;
					game.Player.PossessedCreature.Position = info.Index.ToVector2() * game.Map.TileSize;
					game.Player.PossessedCreature.MapIndex = info.Index;
					info.Tile.CollisionFlag = true;
				}
				else
				{
					PropertyGrid propGrid = (PropertyGrid)game.desktop.Root.FindWidgetById("TilePropertyGrid");
					propGrid.Object = info.Tile;
				}
			}
		}

		public static void TeleportToNextRegion()
		{
			return;
		}
	}
}

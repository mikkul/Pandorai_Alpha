using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using Pandorai.Tilemaps;

namespace Pandorai.Cheats
{
	public static class CheatShortcuts
	{
		public static Main Game;

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

			if (Game.Player.HoldingControl)
			{
				Label textLabel = (Label)Game.desktop.Root.FindWidgetById("DialogueTextLabel");
				textLabel.Text = $"X: {info.Index.X}, Y: {info.Index.Y}";
			}
		}

		public static void HandleTileClick(TileInfo info)
		{
			if (!Activated) return;

			if (Game.Player.HoldingControl)
			{
				if(Game.Player.HoldingShift)
				{
					Game.Map.Tiles[Game.Player.PossessedCreature.MapIndex.X, Game.Player.PossessedCreature.MapIndex.Y].CollisionFlag = false;
					Game.Player.PossessedCreature.Position = info.Index.ToVector2() * Game.Map.TileSize;
					Game.Player.PossessedCreature.MapIndex = info.Index;
					info.Tile.CollisionFlag = true;
				}
				else
				{
					PropertyGrid propGrid = (PropertyGrid)Game.desktop.Root.FindWidgetById("TilePropertyGrid");
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

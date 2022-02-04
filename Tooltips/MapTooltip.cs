using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Pandorai.Utility;
using Pandorai.Tilemaps;
using System.Linq;

namespace Pandorai.Tooltips
{
	public static class MapTooltip
	{
		static Game1 game;
		static Panel currentTooltip = null;
		static Point lastTile;

		public static void Init(Game1 _game)
		{
			game = _game;
		}

		public static void DisplayMapTooltip(TileInfo info)
		{
			return;

			if (info.Index != lastTile)
			{
				if(currentTooltip != null)
					currentTooltip.RemoveFromDesktop();
				currentTooltip = null;
			}

			lastTile = info.Index;

			var playerNeighbouringTiles = GenHelper.Get8Neighbours(game.Player.PossessedCreature.MapIndex);
			if (currentTooltip != null || !playerNeighbouringTiles.Contains(info.Index)) return;

			currentTooltip = new Panel
			{
				Left = (int)game.InputManager.MousePos.X,
				Top = (int)game.InputManager.MousePos.Y,
				Background = new SolidBrush(Color.Black * 0.5f),
				Width = 150,
				Height = 150,
				Enabled = false,
				Border = new SolidBrush(Color.DarkGray * 0.5f),
				BorderThickness = new Thickness(5),
			};

			var stackPanel = new VerticalStackPanel
			{
			};

			stackPanel.Proportions.Add(new Proportion());

			if(info.Tile.TooltipInfo != null)
			{
				stackPanel.Widgets.Add(new Label
				{
					Text = info.Tile.TooltipInfo.Title,
					HorizontalAlignment = HorizontalAlignment.Center,
					Wrap = true,
				});

				if(info.Tile.TooltipInfo.GetType() == typeof(TextTooltip))
				{
					TextTooltip textTooltip = (TextTooltip)info.Tile.TooltipInfo;
					stackPanel.Widgets.Add(new Label
					{
						Text = textTooltip.Text,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center,
						Wrap = true,
					});
				}
				else if(info.Tile.TooltipInfo.GetType() == typeof(ImageTooltip))
				{
					ImageTooltip imageTooltip = (ImageTooltip)info.Tile.TooltipInfo;
					stackPanel.Widgets.Add(new Image
					{
						Renderable = new TextureRegion(imageTooltip.Image, new Rectangle(0, 0, 100, 100)),
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center,
					});
				}
			}

			currentTooltip.Widgets.Add(stackPanel);
			game.desktop.Widgets.Add(currentTooltip);
		}
	}
}

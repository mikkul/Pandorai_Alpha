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
		private static Main _game;
		private static Panel _currentTooltip = null;
		private static Point _lastTile;

		public static void Init(Main _game)
		{
            MapTooltip._game = _game;
		}

		public static void DisplayMapTooltip(TileInfo info)
		{
			if (info.Index != _lastTile)
			{
				if(_currentTooltip != null)
					_currentTooltip.RemoveFromDesktop();
				_currentTooltip = null;
			}

			_lastTile = info.Index;

			var playerNeighbouringTiles = GenHelper.Get8Neighbours(_game.Player.PossessedCreature.MapIndex);
			if (_currentTooltip != null || !playerNeighbouringTiles.Contains(info.Index)) return;

			var width = 200;
			var height = 200;

			_currentTooltip = new Panel
			{
				Left = (int)_game.InputManager.MousePos.X,
				Top = (int)_game.InputManager.MousePos.Y,
				Background = new SolidBrush(Color.Black * 0.5f),
				Width = width,
				Height = height,
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

			_currentTooltip.Widgets.Add(stackPanel);
			_game.desktop.Widgets.Add(_currentTooltip);
		}
	}
}

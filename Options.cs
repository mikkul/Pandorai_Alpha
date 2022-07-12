using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using Pandorai.Rendering;
using System;
using System.Collections.Generic;

namespace Pandorai
{
	public delegate void ValueChangeHandler<T>(T oldValue, T newValue);

	public class Options
	{
		public event ValueChangeHandler<int> TileSizeChanged;
		public event EmptyEventHandler SettingsApplied;

		public const int DefaultUnitSize = 64;

		public Color ClearColor = new Color(0.0f, 0.0f, 0.0f, 1f);

		public float TilesOutsideFOVDarkening = 0.25f;

		public static Point OldResolution;
		public bool OldIsFullScreen;

		public bool EnableFPSCounter = false;

		public PropertyGrid TilePropGrid;
		public ScrollViewer TilePropGridScroll;

		public List<Point> ResolutionList = new List<Point>();

		private int tileSize = 64;
		public int TileSize
		{
			get => tileSize;
			set
			{
				value = value < 1 ? 1 : value;
				TileSizeChanged?.Invoke(tileSize, value);
				tileSize = value;
				_game.Map.TileSize = value;
			}
		}

		public float UnitMultiplier => (float)TileSize / (float)DefaultUnitSize;

		private Main _game;

		private Point _newResolution;
		private bool _newIsFullscreen;

		public Options(Main thisGame)
		{
			_game = thisGame;

			OldResolution = new Point(_game.Graphics.PreferredBackBufferWidth, _game.Graphics.PreferredBackBufferHeight);
			OldIsFullScreen = _game.Graphics.IsFullScreen;

			_newResolution = OldResolution;
			_newIsFullscreen = OldIsFullScreen;

			ResolutionList.Add(new Point(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height));
			ResolutionList.Add(new Point(1200, 800));
			ResolutionList.Add(new Point(800, 600));
		}

		public void ApplyChanges()
		{
			OldResolution = _newResolution;
			OldIsFullScreen = _newIsFullscreen;

			_game.Camera.UpdateViewport(this);
			_game.Map.SetAmountTilesRendered(_game.Camera.Viewport.Width / _game.Map.TileSize / 2 + 2, _game.Camera.Viewport.Height / _game.Map.TileSize / 2 + 2);
			_game.ViewportTarget = new RenderTarget2D(_game.GraphicsDevice, _game.Camera.Viewport.Width, _game.Camera.Viewport.Height);
			_game.Map.OffsetToMiddle = new Vector2(_game.Camera.Viewport.X + _game.Camera.Viewport.Width / 2 - _game.Map.TileSize / 2, _game.Camera.Viewport.Y + _game.Camera.Viewport.Height / 2 - _game.Map.TileSize / 2);

			AdjustGUI();
			SettingsApplied?.Invoke();
		}

		public void RevertChanges()
		{
			if(OldResolution != _newResolution)
			{
				ChangeResolution(OldResolution, null);
			}

			if(OldIsFullScreen != _newIsFullscreen)
			{
				ToggleFullscreen();
			}
		}

		public void AdjustGUI()
		{
			string[] inventoryIds = new string[]
			{
				"inventory",
				"sidekickInventory",
				"chestInventory",
			};
			foreach (var id in inventoryIds)
			{
				try
				{
					Grid inventoryGrid = (Grid)_game.desktop.Root.FindWidgetById(id);
					if (inventoryGrid != null)
					{
						foreach (var proportion in inventoryGrid.RowsProportions)
						{
							proportion.Value = _game.Graphics.PreferredBackBufferWidth / 16;
						}
						foreach (var button in inventoryGrid.Widgets)
						{
							button.Width = _game.Graphics.PreferredBackBufferWidth / 16;
							button.Height = _game.Graphics.PreferredBackBufferWidth / 16;
						}
					}
				}
				catch (Exception) {}
			}
		}

		public void ChangeResolution(Point newResolutionArg, Dialog optionsWindow)
		{
			_newResolution = newResolutionArg;

			_game.Graphics.PreferredBackBufferWidth = _newResolution.X;
			_game.Graphics.PreferredBackBufferHeight = _newResolution.Y;
			_game.Graphics.ApplyChanges();

			CenterGameWindow();

			// center the options window
			optionsWindow?.OnKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter);
			optionsWindow?.ShowModal(_game.desktop, new Point(OldResolution.X / 2 - (int)optionsWindow.Width / 2, OldResolution.Y / 2 - (int)optionsWindow.Height / 2));

			LightingManager.RefreshRenderTarget(_game.GraphicsDevice, _game.Camera);
		}

		public void ToggleFullscreen()
		{
			_game.Graphics.ToggleFullScreen();

			CenterGameWindow();

			_newIsFullscreen = _game.Graphics.IsFullScreen;
		}

		public void CenterGameWindow()
		{
			int windowWidth = _game.Graphics.PreferredBackBufferWidth;
			int windowHeight = _game.Graphics.PreferredBackBufferHeight;
			int screenWidth = _game.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
			int screenHeight = _game.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;

			int x = screenWidth / 2 - windowWidth / 2;
			int y = screenHeight / 2 - windowHeight / 2;

			_game.Window.Position = new Point(x, y);
		}
	}
}

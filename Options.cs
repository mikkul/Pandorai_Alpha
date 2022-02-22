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

		public float TilesOutsideFOVDarkening = -0.25f;

		public static Point oldResolution;
		public bool oldIsFullScreen;

		public bool enableFPSCounter = false;

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
				game.Map.TileSize = value;
			}
		}

		public float UnitMultiplier {
			get => (float)TileSize / (float)DefaultUnitSize;
		}

		Game1 game;

		Point newResolution;
		bool newIsFullscreen;

		public Options(Game1 thisGame)
		{
			game = thisGame;

			oldResolution = new Point(game.graphics.PreferredBackBufferWidth, game.graphics.PreferredBackBufferHeight);
			oldIsFullScreen = game.graphics.IsFullScreen;

			newResolution = oldResolution;
			newIsFullscreen = oldIsFullScreen;

			ResolutionList.Add(new Point(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height));
			ResolutionList.Add(new Point(1200, 800));
			ResolutionList.Add(new Point(800, 600));
		}

		public void ApplyChanges()
		{
			oldResolution = newResolution;
			oldIsFullScreen = newIsFullscreen;

			game.Camera.UpdateViewport(this);
			game.Map.SetAmountTilesRendered(game.Camera.Viewport.Width / game.Map.TileSize / 2 + 2, game.Camera.Viewport.Height / game.Map.TileSize / 2 + 2);
			game.ViewportTarget = new RenderTarget2D(game.GraphicsDevice, game.Camera.Viewport.Width, game.Camera.Viewport.Height);
			game.Map.OffsetToMiddle = new Vector2(game.Camera.Viewport.X + game.Camera.Viewport.Width / 2 - game.Map.TileSize / 2, game.Camera.Viewport.Y + game.Camera.Viewport.Height / 2 - game.Map.TileSize / 2);

			AdjustGUI();
			SettingsApplied?.Invoke();
		}

		public void RevertChanges()
		{
			if(oldResolution != newResolution)
			{
				ChangeResolution(oldResolution, null);
			}

			if(oldIsFullScreen != newIsFullscreen)
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
				Grid inventoryGrid = (Grid)game.desktop.Root.FindWidgetById(id);
				if (inventoryGrid != null)
				{
					foreach (var proportion in inventoryGrid.RowsProportions)
					{
						proportion.Value = game.graphics.PreferredBackBufferWidth / 16;
					}
					foreach (var button in inventoryGrid.Widgets)
					{
						button.Width = game.graphics.PreferredBackBufferWidth / 16;
						button.Height = game.graphics.PreferredBackBufferWidth / 16;
					}
				}
			}
		}

		public void ChangeResolution(Point newResolutionArg, Dialog optionsWindow)
		{
			newResolution = newResolutionArg;

			game.graphics.PreferredBackBufferWidth = newResolution.X;
			game.graphics.PreferredBackBufferHeight = newResolution.Y;
			game.graphics.ApplyChanges();

			CenterGameWindow();

			// center the options window
			optionsWindow?.OnKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter);
			optionsWindow?.ShowModal(game.desktop, new Point(oldResolution.X / 2 - (int)optionsWindow.Width / 2, oldResolution.Y / 2 - (int)optionsWindow.Height / 2));

			LightingManager.RefreshRenderTarget(game.GraphicsDevice, game.Camera);
		}

		public void ToggleFullscreen()
		{
			game.graphics.ToggleFullScreen();

			CenterGameWindow();

			newIsFullscreen = game.graphics.IsFullScreen;
		}

		public void CenterGameWindow()
		{
			int windowWidth = game.graphics.PreferredBackBufferWidth;
			int windowHeight = game.graphics.PreferredBackBufferHeight;
			int screenWidth = game.graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
			int screenHeight = game.graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;

			int x = screenWidth / 2 - windowWidth / 2;
			int y = screenHeight / 2 - windowHeight / 2;

			game.Window.Position = new Point(x, y);
		}
	}
}

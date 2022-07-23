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
				Main.Game.Map.TileSize = value;
			}
		}

		public float UnitMultiplier => (float)TileSize / (float)DefaultUnitSize;

		private Point _newResolution;
		private bool _newIsFullscreen;

		public Options()
		{
			OldResolution = new Point(Main.Game.Graphics.PreferredBackBufferWidth, Main.Game.Graphics.PreferredBackBufferHeight);
			OldIsFullScreen = Main.Game.Graphics.IsFullScreen;

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

			Main.Game.Camera.UpdateViewport(this);
			Main.Game.Map.SetAmountTilesRendered(Main.Game.Camera.Viewport.Width / Main.Game.Map.TileSize / 2 + 2, Main.Game.Camera.Viewport.Height / Main.Game.Map.TileSize / 2 + 2);
			Main.Game.ViewportTarget = new RenderTarget2D(Main.Game.GraphicsDevice, Main.Game.Camera.Viewport.Width, Main.Game.Camera.Viewport.Height);
			Main.Game.Map.OffsetToMiddle = new Vector2(Main.Game.Camera.Viewport.X + Main.Game.Camera.Viewport.Width / 2 - Main.Game.Map.TileSize / 2, Main.Game.Camera.Viewport.Y + Main.Game.Camera.Viewport.Height / 2 - Main.Game.Map.TileSize / 2);

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
					Grid inventoryGrid = (Grid)Main.Game.desktop.Root.FindWidgetById(id);
					if (inventoryGrid != null)
					{
						foreach (var proportion in inventoryGrid.RowsProportions)
						{
							proportion.Value = Main.Game.Graphics.PreferredBackBufferWidth / 16;
						}
						foreach (var button in inventoryGrid.Widgets)
						{
							button.Width = Main.Game.Graphics.PreferredBackBufferWidth / 16;
							button.Height = Main.Game.Graphics.PreferredBackBufferWidth / 16;
						}
					}
				}
				catch (Exception) {}
			}
		}

		public void ChangeResolution(Point newResolutionArg, Dialog optionsWindow)
		{
			_newResolution = newResolutionArg;

			Main.Game.Graphics.PreferredBackBufferWidth = _newResolution.X;
			Main.Game.Graphics.PreferredBackBufferHeight = _newResolution.Y;
			Main.Game.Graphics.ApplyChanges();

			CenterGameWindow();

			// center the options window
			optionsWindow?.OnKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter);
			optionsWindow?.ShowModal(Main.Game.desktop, new Point(OldResolution.X / 2 - (int)optionsWindow.Width / 2, OldResolution.Y / 2 - (int)optionsWindow.Height / 2));

			LightingManager.RefreshRenderTarget(Main.Game.GraphicsDevice, Main.Game.Camera);
		}

		public void ToggleFullscreen()
		{
			Main.Game.Graphics.ToggleFullScreen();

			CenterGameWindow();

			_newIsFullscreen = Main.Game.Graphics.IsFullScreen;
		}

		public void CenterGameWindow()
		{
			int windowWidth = Main.Game.Graphics.PreferredBackBufferWidth;
			int windowHeight = Main.Game.Graphics.PreferredBackBufferHeight;
			int screenWidth = Main.Game.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
			int screenHeight = Main.Game.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;

			int x = screenWidth / 2 - windowWidth / 2;
			int y = screenHeight / 2 - windowHeight / 2;

			Main.Game.Window.Position = new Point(x, y);
		}
	}
}

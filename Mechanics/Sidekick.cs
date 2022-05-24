using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using Pandorai.Creatures;
using Pandorai.Items;
using Microsoft.Xna.Framework;
using System;
using Pandorai.ParticleSystems;
using Pandorai.Utility;
using Pandorai.UI;
using Pandorai.Tilemaps;

namespace Pandorai.Mechanics
{
	public static class Sidekick
	{
		public static Creature SlotsSpirit;
		public static Texture2D Sprite;

		public static Item SelectedItem;
		public static Widget DraggedItem;

		public static bool WasItemDragged = false;

		public static Vector2 Position;

		private static Main _game;

		private static Widget _previousGUI = null;

		private static Creature _selectedItemOwner;
		private static bool _isDraggedItemDetached = false;
		private static ImageTextButton _selectedItemButton;

		private static PSSparkles _sparkles;

		private static OpenSimplexNoise _noise;

		private static Random _rng = new Random();

		private static float _thinkingTimer = 0;
		private static float _movementChangeTime = 1000;
		private static float _newMovementChangeTime = 1000;
		private static double _choiceOffset = 0;
		private static Action _currentAction = Action.None;
		private static float _lerpValue = 0;
		private static Vector2 _desiredPos;

		private static float _safeDistance = 3;
		private static float _maxDistance = 12;
		private static float _strafeDistance = 2;

		private static SidekickTips _tips;

		public static void Init()
		{
			_game = Main.Game;

			SlotsSpirit = new Creature(_game);
			SlotsSpirit.Inventory = new Inventory(new Creature(_game), 4)
			{
				Collumns = 4,
				Rows = 1,
				Id = "sidekickInventory"
			};

			_sparkles = new PSSparkles(Vector2.Zero, 100, _game.squareTexture, 1500, 35, 5, 1000, Color.Violet, true, _game);

			_noise = new OpenSimplexNoise();
			
			_tips = new SidekickTips();
		}

		public static void InitLate()
		{
			Position = _game.Player.PossessedCreature.Position - new Vector2(100, 100);
			ParticleSystemManager.AddSystem(_sparkles, false);
			_thinkingTimer = 0;
			_newMovementChangeTime = 2500;
		}

		public static void ConsiderTips()
		{
			_tips.Update();
		}

		public static void AdjustToTileSize(int oldSize, int newSize)
		{
			Position *= (float)newSize / (float)oldSize;
		}

		public static void DeselectItem(Vector2 pos)
		{
			if (DraggedItem != null)
			{
				DraggedItem.Visible = false;
				DraggedItem.RemoveFromDesktop();
				TileInteractionManager.OnItemRelease(SelectedItem);
			}
			_isDraggedItemDetached = false;
			DraggedItem = null;

			_game.InputManager.MouseMove -= HandleItemDrag;
		}

		public static void HandleItemSelection(Item item, Creature user, ImageTextButton button)
		{
			SelectedItem = item;
			_selectedItemOwner = user;
			_selectedItemButton = button;
			_game.InputManager.MouseMove += HandleItemDrag;
			WasItemDragged = false;
		}

		public static void HandleItemDrag(Vector2 pos)
		{
			WasItemDragged = true;
			if(!_isDraggedItemDetached)
			{
				DraggedItem = new Panel
				{
					Left = (int)_game.InputManager.MousePos.X,
					Top = (int)_game.InputManager.MousePos.Y,
					Width = _selectedItemButton.Width,
					Height = _selectedItemButton.Height,
					Background = _selectedItemButton.Image,
				};
				var root = (Panel)_game.desktop.Root;
				root.Widgets.Add(DraggedItem);
				_isDraggedItemDetached = true;
			}
			else
			{
				DraggedItem.Left = (int)pos.X + 2;
				DraggedItem.Top = (int)pos.Y + 2;
			}
		}

		public static void HandleItemRelease(Item item, Creature user, ImageTextButton button)
		{
			Inventory inv1 = _selectedItemOwner.Inventory;
			Inventory inv2 = user.Inventory;

			int amountFromSelectedItem;
			int amountFromReplacedItem;
			int selectedSlotIndex;
			int replacedSlotIndex;

			try
			{
				amountFromSelectedItem = inv1.FindItem(SelectedItem).Amount;
				amountFromReplacedItem = inv2.FindItem(item).Amount;
				selectedSlotIndex = inv1.Items.IndexOf(inv1.FindItem(SelectedItem));
				replacedSlotIndex = inv2.Items.IndexOf(inv2.FindItem(item));
			}
			catch(NullReferenceException)
			{
				DeselectItem(Vector2.Zero);
				return;
			}

			if (item.Id == SelectedItem.Id)
			{
				inv1.ReplaceSlot(new EmptyItem(), 1, selectedSlotIndex);
				inv2.AddElement(SelectedItem, amountFromSelectedItem);
			}
			else
			{
				inv1.ReplaceSlot(item, amountFromReplacedItem, selectedSlotIndex);
				inv2.AddElement(SelectedItem, amountFromSelectedItem, replacedSlotIndex);
			}

			DeselectItem(Vector2.Zero);
		}

		public static void DisplaySlots()
		{
			bool showSlots = false;
			if(showSlots)
			{
				return;
			}
			Proportion columnProp = new Proportion
			{
				Type = ProportionType.Part,
			};
			Proportion rowProp = new Proportion
			{
				Type = ProportionType.Pixels,
				Value = Options.OldResolution.X / 16,
			};
			var gui = SlotsSpirit.Inventory.ConstructGUI(columnProp, rowProp, (i, c, b) =>
			{
				SlotsSpirit.Inventory.OnItemSelected(i, b);
			}, (i, c, b) =>
			{
				SlotsSpirit.Inventory.OnItemRelease(i, b);
				if (!WasItemDragged) i.Use(c);
			});
			gui.GridColumn = 0;
			gui.GridRow = 0;
			if(_previousGUI != null)
			{
				GUI.InventorySlotsGrid.Widgets.Remove(_previousGUI);
			}
			GUI.InventorySlotsGrid.Widgets.Add(gui);
			_previousGUI = gui;
		}

		public static void Update(GameTime dt)
		{
			SlotsSpirit.Position = _game.Player.PossessedCreature.Position;
			SlotsSpirit.MapIndex = _game.Player.PossessedCreature.MapIndex;

			Move(dt);

			_sparkles.CentralPosition = Position;
		}

		public static void Draw(SpriteBatch batch)
		{
			var destRect = new Rectangle(_game.Camera.GetViewportPosition(Position - new Vector2(_game.Map.TileSize / 2, _game.Map.TileSize / 2)).ToPoint(), new Point(_game.Map.TileSize));
			batch.Draw(Sprite, destRect, Color.White);
		}

		private static void Move(GameTime dt)
		{
			_thinkingTimer += (float)dt.ElapsedGameTime.TotalMilliseconds;

			var playerPos = _game.Player.PossessedCreature.Position;

			if (_thinkingTimer >= _movementChangeTime) // changing movement
			{
				_movementChangeTime = _newMovementChangeTime;
				_thinkingTimer = 0;
				_lerpValue = 0;
				_choiceOffset += 1;
				var choiceNum = _noise.Evaluate(_choiceOffset, 0);
				//var choiceNum = rng.NextDouble();
				if(choiceNum < 0.25 || Vector2.Distance(playerPos, Position) > _maxDistance * _game.Map.TileSize)
				{
					_currentAction = Action.Following;
					_desiredPos = playerPos + Vector2.Normalize(Position - playerPos) * _safeDistance * _game.Map.TileSize;
				}
				else if(choiceNum < 0.50)
				{
					_currentAction = Action.SwitchingSide;
					_desiredPos = playerPos - Vector2.Normalize(Position - playerPos) * _safeDistance * _game.Map.TileSize;
				}
				else if(choiceNum < 0.80)
				{
					_currentAction = Action.Strafing;

					float randomX = (float)_noise.Evaluate(Position.X, Position.Y);
					float randomY = (float)_noise.Evaluate(Position.Y, Position.X);

					_desiredPos = Position + Vector2.Normalize(new Vector2(randomX, randomY)) * _strafeDistance * _game.Map.TileSize;
				}
				else
				{
					_currentAction = Action.None;
				}

				_newMovementChangeTime = _rng.Next(500, 1500);
			}

			if(_currentAction == Action.Following)
			{
				_lerpValue += (float)dt.ElapsedGameTime.TotalMilliseconds / _movementChangeTime;
				_desiredPos = playerPos + Vector2.Normalize(Position - playerPos) * _safeDistance * _game.Map.TileSize;
				Position = Vector2.Lerp(Position, _desiredPos, _lerpValue);
			}
			else if(_currentAction == Action.SwitchingSide)
			{
				_lerpValue += (float)dt.ElapsedGameTime.TotalMilliseconds / _movementChangeTime;
				Position = Vector2.Lerp(Position, _desiredPos, _lerpValue);
			}
			else if (_currentAction == Action.Strafing)
			{
				_lerpValue += (float)dt.ElapsedGameTime.TotalMilliseconds / _movementChangeTime;
				Position = Vector2.Lerp(Position, _desiredPos, _lerpValue);
			}
		}

		private enum Action
		{
			Following,
			SwitchingSide,
			Strafing,
			None,
		}
	}
}

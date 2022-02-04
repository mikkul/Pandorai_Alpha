using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using Pandorai.Creatures;
using Pandorai.Items;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
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

		static Game1 game;

		static Widget previousGUI = null;

		public static Item SelectedItem;
		static Creature selectedItemOwner;
		public static Widget DraggedItem;
		static bool isDraggedItemDetached = false;
		static ImageTextButton selectedItemButton;
		public static bool WasItemDragged = false;

		static Vector2 position;
		static float speed = 1;

		static PSSparkles sparkles;

		static OpenSimplexNoise noise;

		static Random rng = new Random();

		static float thinkingTimer = 0;
		static float movementChangeTime = 1000;
		static float newMovementChangeTime = 1000;
		static double choiceOffset = 0;
		static Action currentAction = Action.None;
		static float lerpValue = 0;
		static Vector2 desiredPos;

		static float safeDistance = 3;
		static float maxDistance = 12;
		static float strafeDistance = 2;

		public static void Init()
		{
			game = Game1.game;

			SlotsSpirit = new Creature(game);
			SlotsSpirit.Inventory = new Inventory(new Creature(game), 4)
			{
				Collumns = 4,
				Rows = 1,
				Id = "sidekickInventory"
			};

			sparkles = new PSSparkles(Vector2.Zero, 100, game.squareTexture, 1500, 35, 5, 1000, Color.Violet, true, game);

			noise = new OpenSimplexNoise();
		}

		public static void InitLate()
		{
			position = game.Player.PossessedCreature.Position - new Vector2(100, 100);
			ParticleSystemManager.AddSystem(sparkles, false);
			thinkingTimer = 0;
			newMovementChangeTime = 2500;
		}

		public static void AdjustToTileSize(int oldSize, int newSize)
		{
			position *= (float)newSize / (float)oldSize;
		}

		public static void DeselectItem(Vector2 pos)
		{
			if (DraggedItem != null)
			{
				DraggedItem.Visible = false;
				DraggedItem.RemoveFromDesktop();
				TileInteractionManager.OnItemRelease(SelectedItem);
			}
			isDraggedItemDetached = false;
			DraggedItem = null;

			game.InputManager.MouseMove -= HandleItemDrag;
		}

		public static void HandleItemSelection(Item item, Creature user, ImageTextButton button)
		{
			SelectedItem = item;
			selectedItemOwner = user;
			selectedItemButton = button;
			game.InputManager.MouseMove += HandleItemDrag;
			WasItemDragged = false;
		}

		public static void HandleItemDrag(Vector2 pos)
		{
			WasItemDragged = true;
			if(!isDraggedItemDetached)
			{
				DraggedItem = new Panel
				{
					Left = (int)game.InputManager.MousePos.X,
					Top = (int)game.InputManager.MousePos.Y,
					Width = selectedItemButton.Width,
					Height = selectedItemButton.Height,
					Background = selectedItemButton.Image,
				};
				var root = (Panel)game.desktop.Root;
				root.Widgets.Add(DraggedItem);
				isDraggedItemDetached = true;
			}
			else
			{
				DraggedItem.Left = (int)pos.X + 2;
				DraggedItem.Top = (int)pos.Y + 2;
			}
		}

		public static void HandleItemRelease(Item item, Creature user, ImageTextButton button)
		{
			Inventory inv1 = selectedItemOwner.Inventory;
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
			Proportion columnProp = new Proportion
			{
				Type = ProportionType.Part,
			};
			Proportion rowProp = new Proportion
			{
				Type = ProportionType.Pixels,
				Value = Options.oldResolution.X / 16,
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
			if(previousGUI != null)
			{
				GUI.inventorySlotsGrid.Widgets.Remove(previousGUI);
			}
			GUI.inventorySlotsGrid.Widgets.Add(gui);
			previousGUI = gui;
		}

		public static void Update(GameTime dt)
		{
			SlotsSpirit.Position = game.Player.PossessedCreature.Position;
			SlotsSpirit.MapIndex = game.Player.PossessedCreature.MapIndex;

			Move(dt);

			sparkles.CentralPosition = position;
		}

		public static void Draw(SpriteBatch batch)
		{
			var destRect = new Rectangle(game.Camera.GetViewportPosition(position - new Vector2(game.Map.TileSize / 2, game.Map.TileSize / 2)).ToPoint(), new Point(game.Map.TileSize));
			batch.Draw(Sprite, destRect, Color.White);
		}

		private static void Move(GameTime dt)
		{
			thinkingTimer += (float)dt.ElapsedGameTime.TotalMilliseconds;

			var playerPos = game.Player.PossessedCreature.Position;

			if (thinkingTimer >= movementChangeTime) // changing movement
			{
				movementChangeTime = newMovementChangeTime;
				thinkingTimer = 0;
				lerpValue = 0;
				choiceOffset += 1;
				var choiceNum = noise.Evaluate(choiceOffset, 0);
				//var choiceNum = rng.NextDouble();
				if(choiceNum < 0.25 || Vector2.Distance(playerPos, position) > maxDistance * game.Map.TileSize)
				{
					currentAction = Action.Following;
					desiredPos = playerPos + Vector2.Normalize(position - playerPos) * safeDistance * game.Map.TileSize;
				}
				else if(choiceNum < 0.50)
				{
					currentAction = Action.SwitchingSide;
					desiredPos = playerPos - Vector2.Normalize(position - playerPos) * safeDistance * game.Map.TileSize;
				}
				else if(choiceNum < 0.80)
				{
					currentAction = Action.Strafing;

					float randomX = (float)noise.Evaluate(position.X, position.Y);
					float randomY = (float)noise.Evaluate(position.Y, position.X);

					desiredPos = position + Vector2.Normalize(new Vector2(randomX, randomY)) * strafeDistance * game.Map.TileSize;
				}
				else
				{
					currentAction = Action.None;
				}

				newMovementChangeTime = rng.Next(500, 1500);
			}

			if(currentAction == Action.Following)
			{
				lerpValue += (float)dt.ElapsedGameTime.TotalMilliseconds / movementChangeTime;
				desiredPos = playerPos + Vector2.Normalize(position - playerPos) * safeDistance * game.Map.TileSize;
				position = Vector2.Lerp(position, desiredPos, lerpValue);
			}
			else if(currentAction == Action.SwitchingSide)
			{
				lerpValue += (float)dt.ElapsedGameTime.TotalMilliseconds / movementChangeTime;
				position = Vector2.Lerp(position, desiredPos, lerpValue);
			}
			else if (currentAction == Action.Strafing)
			{
				lerpValue += (float)dt.ElapsedGameTime.TotalMilliseconds / movementChangeTime;
				position = Vector2.Lerp(position, desiredPos, lerpValue);
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

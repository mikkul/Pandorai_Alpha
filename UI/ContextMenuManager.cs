using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Pandorai.Creatures;
using Pandorai.Structures;
using Pandorai.Tilemaps;
using System;
using System.Collections.Generic;

namespace Pandorai.UI
{
	public static class ContextMenuManager
	{
		private static Widget contextMenu = null;

		public static void ShowContextMenu(TileInfo	info)
		{
			if (Game1.game.Player.IsInteractingWithSomeone) return;

			if(contextMenu != null)
				HideContextMenu();

			Game1.game.Player.IsInteractingWithSomeone = true;

			List<ContextMenuOption> options = CheckAvailableOptions(info);

			contextMenu = ContextMenuGUI(options);
			contextMenu.Left = (int)Game1.game.InputManager.MousePos.X;
			contextMenu.Top = (int)Game1.game.InputManager.MousePos.Y;
			Game1.game.desktop.Widgets.Add(contextMenu);
			contextMenu.MouseLeft += (e, a) =>
			{
				HideContextMenu();
			};
		}

		public static void HideContextMenu()
		{
			contextMenu.RemoveFromDesktop();
			contextMenu = null;
			Game1.game.Player.IsInteractingWithSomeone = false;
		}

		private static List<ContextMenuOption> CheckAvailableOptions(TileInfo info)
		{
			List<ContextMenuOption> options = new List<ContextMenuOption>();

			var distToPlayer = (info.Index - Game1.game.Player.PossessedCreature.MapIndex).ToVector2().LengthSquared();
			if(distToPlayer <= 2.0f) // if player is on an adjacent tile
			{
				string objectName;
				var tryCreature = Game1.game.CreatureManager.GetCreature(info.Index);
				if (tryCreature != null)
				{
					objectName = tryCreature.Id;

					options.Add(new ContextMenuOption
					{
						Text = "Attack",
						Action = () =>
						{
							tryCreature.Hit(Game1.game.Player.PossessedCreature);
							Game1.game.TurnManager.PlayerIsReady();
						},
					});
					options.Add(new ContextMenuOption
					{
						Text = "Interact",
						Action = () =>
						{
							tryCreature.Interact(Game1.game.Player.PossessedCreature);
							Game1.game.TurnManager.PlayerIsReady();
						},
					});
				}

				else if(info.Tile.HasStructure())
				{
					objectName = info.Tile.MapObject.Structure.Id;
					options.Add(new ContextMenuOption
					{
						Text = "Interact",
						Action = () =>
						{
							StructureManager.CheckStructureInteraction(Game1.game.Player.PossessedCreature, info);
							Game1.game.TurnManager.PlayerIsReady();
						},
					});
					options.Add(new ContextMenuOption
					{
						Text = "Kick",
						Action = () =>
						{
							info.Tile.MapObject.Structure.UseForce(ForceType.Physical);
							Game1.game.TurnManager.PlayerIsReady();
						},
					});
				}

				else
				{
					objectName = "seemingly empty space";
				}	

				options.Add(new ContextMenuOption
				{
					Text = "Inspect",
					Action = () =>
					{
						MessageLog.DisplayMessage($"You inspect the {objectName}");
					},
				});
			}

			return options;
		}

		private static Widget ContextMenuGUI(List<ContextMenuOption> options)
		{
			VerticalStackPanel panel = new VerticalStackPanel
			{
				Spacing = 4,
				Background = new SolidBrush(Color.Navy),
				Padding = new Thickness(8),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
			};

			foreach (var option in options)
			{
				TextButton button = new TextButton
				{
					Text = option.Text,
					Width = 130,
					Height = 30,
				};

				button.Click += (e, a) =>
				{
					HideContextMenu();
					option.Action.Invoke();
				};

				panel.Widgets.Add(button);
			}

			return panel;
		}
	}
}

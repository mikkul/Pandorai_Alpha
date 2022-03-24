using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Pandorai.Structures;
using Pandorai.Tilemaps;
using System.Collections.Generic;

namespace Pandorai.UI
{
	public static class ContextMenuManager
	{
		private static Widget _contextMenu = null;

		public static void ShowContextMenu(TileInfo	info)
		{
			if (Main.Game.Player.IsInteractingWithSomeone) return;

			if(_contextMenu != null)
				HideContextMenu();

			Main.Game.Player.IsInteractingWithSomeone = true;

			List<ContextMenuOption> options = CheckAvailableOptions(info);

			_contextMenu = ContextMenuGUI(options);
			_contextMenu.Left = (int)Main.Game.InputManager.MousePos.X;
			_contextMenu.Top = (int)Main.Game.InputManager.MousePos.Y;
			Main.Game.desktop.Widgets.Add(_contextMenu);
			_contextMenu.MouseLeft += (e, a) =>
			{
				HideContextMenu();
			};
		}

		public static void HideContextMenu()
		{
			_contextMenu.RemoveFromDesktop();
			_contextMenu = null;
			Main.Game.Player.IsInteractingWithSomeone = false;
		}

		private static List<ContextMenuOption> CheckAvailableOptions(TileInfo info)
		{
			List<ContextMenuOption> options = new List<ContextMenuOption>();

			var distToPlayer = (info.Index - Main.Game.Player.PossessedCreature.MapIndex).ToVector2().LengthSquared();
			if(distToPlayer <= 2.0f) // if player is on an adjacent tile
			{
				string objectName;
				var tryCreature = Main.Game.CreatureManager.GetCreature(info.Index);
				if (tryCreature != null)
				{
					objectName = tryCreature.Id;

					options.Add(new ContextMenuOption
					{
						Text = "Attack",
						Action = () =>
						{
							tryCreature.Hit(Main.Game.Player.PossessedCreature);
							Main.Game.TurnManager.PlayerIsReady();
						},
					});
					options.Add(new ContextMenuOption
					{
						Text = "Interact",
						Action = () =>
						{
							tryCreature.Interact(Main.Game.Player.PossessedCreature);
							Main.Game.TurnManager.PlayerIsReady();
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
							StructureManager.CheckStructureInteraction(Main.Game.Player.PossessedCreature, info);
							Main.Game.TurnManager.PlayerIsReady();
						},
					});
					options.Add(new ContextMenuOption
					{
						Text = "Kick",
						Action = () =>
						{
							info.Tile.MapObject.Structure.UseForce(ForceType.Physical);
							Main.Game.TurnManager.PlayerIsReady();
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

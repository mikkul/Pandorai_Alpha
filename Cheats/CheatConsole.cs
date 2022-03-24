using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework;
using System.Linq;
using Pandorai.Creatures;
using Pandorai.Items;
using Pandorai.Creatures.Behaviours;

namespace Pandorai.Cheats
{
	public static class CheatConsole
	{
		public static Keys ActivationKey = Keys.OemTilde;

		public static Main Game;

		public static List<string> CommandParameters;

		public static Dictionary<string, Action> Commands = new Dictionary<string, Action>();

		public static bool IsActive = false;
		
        private static Vision _originalPlayerVision;

        public static void InitCommands()
		{
			Commands.Add("godmode", () =>
			{
				Game.Player.PossessedCreature.Stats.MaxHealth = int.MaxValue;
				Game.Player.PossessedCreature.Stats.Health = int.MaxValue;
			});

			Commands.Add("activate", () =>
			{
				CheatShortcuts.Activated ^= true;
			});

			Commands.Add("allvision", () =>
			{
				var possesedCreature = Main.Game.Player.PossessedCreature;
				if(_originalPlayerVision == null)
				{
					var allSeeingVision = new AllSeeingVision();
					allSeeingVision.Owner = possesedCreature;
					var originalVision = possesedCreature.GetBehaviour<Vision>();
					possesedCreature.Behaviours.Remove(originalVision);
					possesedCreature.Behaviours.Add(allSeeingVision);
					_originalPlayerVision = originalVision;
				}
				else
				{
					var allSeeingVision = possesedCreature.GetBehaviour<AllSeeingVision>();
					possesedCreature.Behaviours.Remove(allSeeingVision);
					possesedCreature.Behaviours.Add(_originalPlayerVision);
					_originalPlayerVision = null;
				}
			});			

			Commands.Add("modifystats", () =>
			{
				UI.GUI.ShowCheatsModifyStatsWindow();
			});					

			Commands.Add("giveitem", () =>
			{
				var itemName = CommandParameters[0];
				Game.Player.PossessedCreature.Inventory.AddElement(ItemLoader.GetItem(itemName));
			});			

			Commands.Add("noclip", () =>
			{
				Game.Player.PossessedCreature.NoClip ^= true;
			});

			Commands.Add("speed", () =>
			{
				Game.TurnManager.heroTurnTime = int.Parse(CommandParameters[0]);
			});

			Commands.Add("teleport", () =>
			{
			});

			Commands.Add("move", () =>
			{
				Game.Player.PossessedCreature.Position.X += int.Parse(CommandParameters[0]) * Game.Map.TileSize;
				Game.Player.PossessedCreature.Position.Y += int.Parse(CommandParameters[1]) * Game.Map.TileSize;
				Game.Player.PossessedCreature.MapIndex = Game.Map.GetTileIndexByPosition(Game.Player.PossessedCreature.Position);
			});

			Commands.Add("kill", () =>
			{
				if(CommandParameters.Count > 0)
				{
					if (CommandParameters[0] == "all")
					{
						if (CommandParameters[1] == "spiders")
						{
							//List<Creature> creaturesToBeKilled = game.CreatureManager.Creatures.FindAll(item => item.GetType() == typeof(Spider));
							//foreach (var creature in creaturesToBeKilled)
							//{
							//	creature.GetHit(9999);
							//}
						}
					}
					else
					{
						int x, y;
						int.TryParse(CommandParameters[0], out x);
						int.TryParse(CommandParameters[1], out y);

						if (x != 0 && y != 0)
						{
							Game.CreatureManager.GetCreature(new Point(x, y))?.GetHit(9999, Main.Game.Player.PossessedCreature);
						}
					}
				}
			});

			Commands.Add("class", () =>
			{
				if (CommandParameters.Count > 0)
				{
					if (CommandParameters[0] == "monster")
					{
						Game.Player.PossessedCreature.Class = CreatureClass.Monster;
					}
					else if (CommandParameters[0] == "human")
					{
						Game.Player.PossessedCreature.Class = CreatureClass.Human;
					}
					else if (CommandParameters[0] == "neutral")
					{
						Game.Player.PossessedCreature.Class = CreatureClass.Neutral;
					}
					Console.WriteLine(CommandParameters[0]);
				}
			});
		}

		public static void UseCommand(string text)
		{
			string[] textArr = text.Split(' ');

			string commandName = textArr[0];

			if (Commands.ContainsKey(commandName))
			{
				CommandParameters = textArr.ToList();
				CommandParameters.RemoveAt(0);
				Commands[commandName]();
			}
		}

		public static void ActivationHandler(Keys key)
		{
			if (IsActive) return;

			if(key.Equals(ActivationKey))
			{
				var console = GUI();
				console.Show(Game.desktop, Point.Zero);
				console.FindWidgetById("CheatConsoleInput").SetKeyboardFocus();
				IsActive = true;
			}
		}

		public static Dialog GUI()
		{
			Dialog console = new Dialog
			{
				Id = "CheatConsole",
				AcceptsKeyboardFocus = true,
				Width = (int)(Options.OldResolution.X * 0.75f),
				Opacity = 0.8f
			};

			TextBox commandPrompt = new TextBox
			{
				Id = "CheatConsoleInput",
				AcceptsKeyboardFocus = true
			};

			console.Closed += (s, a) =>
			{
				if(console.Result)
				{
					if(commandPrompt.Text != null)
					{
						UseCommand(commandPrompt.Text);
					}
				}

				IsActive = false;
				Game.desktop.FocusedKeyboardWidget = null;
			};

			console.Content = commandPrompt;

			return console;
		}
	}
}

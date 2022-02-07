using Myra;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework;
using System.Linq;
using Pandorai.Creatures;

namespace Pandorai.Cheats
{
	public static class CheatConsole
	{
		public static Keys ActivationKey = Keys.OemTilde;

		public static Game1 game;

		public static List<string> CommandParameters;

		public static Dictionary<string, Action> Commands = new Dictionary<string, Action>();

		public static bool IsActive = false;

		public static void InitCommands()
		{
			Commands.Add("godmode", () =>
			{
				game.Player.PossessedCreature.MaxHealth = int.MaxValue;
				game.Player.PossessedCreature.Health = int.MaxValue;
			});

			Commands.Add("activate", () =>
			{
				CheatShortcuts.Activated ^= true;
			});

			Commands.Add("noclip", () =>
			{
				game.Player.PossessedCreature.NoClip ^= true;
			});

			Commands.Add("speed", () =>
			{
				game.TurnManager.heroTurnTime = int.Parse(CommandParameters[0]);
			});

			Commands.Add("teleport", () =>
			{
			});

			Commands.Add("move", () =>
			{
				game.Player.PossessedCreature.Position.X += int.Parse(CommandParameters[0]) * game.Map.TileSize;
				game.Player.PossessedCreature.Position.Y += int.Parse(CommandParameters[1]) * game.Map.TileSize;
				game.Player.PossessedCreature.MapIndex = game.Map.GetTileIndexByPosition(game.Player.PossessedCreature.Position);
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
							game.CreatureManager.GetCreature(new Point(x, y))?.GetHit(9999, Game1.game.Player.PossessedCreature);
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
						game.Player.PossessedCreature.Class = CreatureClass.Monster;
					}
					else if (CommandParameters[0] == "human")
					{
						game.Player.PossessedCreature.Class = CreatureClass.Human;
					}
					else if (CommandParameters[0] == "neutral")
					{
						game.Player.PossessedCreature.Class = CreatureClass.Neutral;
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
				console.Show(game.desktop, Point.Zero);
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
				Width = (int)(Options.oldResolution.X * 0.75f),
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
				game.desktop.FocusedKeyboardWidget = null;
			};

			console.Content = commandPrompt;

			return console;
		}
	}
}

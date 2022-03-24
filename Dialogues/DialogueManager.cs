using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Myra.Graphics2D.UI;
using Myra.Graphics2D;

namespace Pandorai.Dialogues
{
	public class DialogueManager
	{
		public static Main game;

		public static Label NameLabel;
		public static Label TextLabel;
		public static VerticalStackPanel OptionsStack;

		public static List<DialogueNode> ReadDialogueFile(string fileName)
		{
			List<DialogueNode> nodes = new List<DialogueNode>();

			string filePath = Path.Combine(game.Content.RootDirectory, "DialogueFiles", fileName);

			string fileContent = File.ReadAllText(filePath);

			// delete unnecesary spaces
			// eg. <type   > - delete spaces between 'type' and '>'
			string edit1 = Regex.Replace(fileContent, @"\s\B", "");

			// divide entire text string into chunks representing each dialogue node
			string[] dividedNodeStrings = Regex.Matches(edit1, @"(?!#)[^\n][\s\S]+?(?=#|$)").Cast<Match>().Select(m => m.Value).ToArray();

			List<string> dividedNodesEdit1 = new List<string>();

			foreach (string nodeString in dividedNodeStrings)
			{
				// split into multiple lines
				string editedNode = Regex.Replace(nodeString, @"<|{|}", "\n$0");
				dividedNodesEdit1.Add(editedNode);

				// find the index value, type value, content value and store them
				Match indexMatch = Regex.Match(editedNode, @"<!(.+)>(.*)");
				string index = indexMatch.Groups[2].Value;

				Match typeMatch = indexMatch.NextMatch();
				string type = typeMatch.Groups[2].Value;

				Match contentMatch = typeMatch.NextMatch();
				string content = null;
				if (contentMatch.Success)
				{
					content = contentMatch.Groups[2].Value;
				}

				// set node's index and type
				var dialNode = new DialogueNode();
				dialNode.Index = int.Parse(index);
				dialNode.Type = type;
				dialNode.Content = content;

				// delete unnecesary fields (index and type)
				editedNode = Regex.Replace(editedNode, @"\n?<!.+>.*\n", "");

				// 
				MatchCollection actionMatches = Regex.Matches(editedNode, @"^.+", RegexOptions.Multiline);
				foreach (Match actionMatch in actionMatches)
				{
					string actionName = Regex.Match(actionMatch.Value, @"<(.+)>").Groups[1].Value;
					string actionValue = null;
					if (actionName == "action" || actionName == "if" || actionName == "method" || actionName == "condition" || actionName == "effect")
					{
						actionValue = Regex.Match(actionMatch.Value, @">(.+)").Groups[1].Value;
					}
					dialNode.Actions.Add(new DialogueAction(actionName, actionValue));
				}

				// add prepared node to our list of nodes
				nodes.Add(dialNode);
			}

			return nodes;
		}

		public static void SetNameLabel(string value)
		{
			NameLabel.Text = value;
		}

		public static void SetNameLabel()
		{
			NameLabel.Text = "";
		}

		public static void SetTextContent(string value)
		{
			TextLabel.Text = value;
		}

		public static void SetTextContent()
		{
			TextLabel.Text = "";
		}

		public static void HideOptions()
		{
			OptionsStack.Widgets.Clear();
		}

		public static void ShowOptions(List<DialogueOption> options, Dialogue dialogueInstance)
		{
			HideOptions();
			foreach (var option in options)
			{
				OptionsStack.Widgets.Add(OptionButton(option, dialogueInstance));
			}
		}

		private static TextButton OptionButton(DialogueOption option, Dialogue dialogueInstance)
		{
			TextButton button = new TextButton
			{
				Text = option.Content,
				Width = (int)(Options.OldResolution.X * 0.75f),
				Padding = new Thickness(3),
			};

			foreach (var condition in option.Conditions)
			{
				if(!condition.Check())
				{
					button.Enabled = false;
					break;
				}
			}

			button.Click += (s, a) =>
			{
				dialogueInstance.ReadNode(option.NodeIndex);
			};

			return button;
		}
	}
}

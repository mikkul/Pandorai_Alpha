using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using Pandorai.Mechanics;
using Pandorai.Items;
using Pandorai.Effects;
using Pandorai.Conditions;
using Pandorai.UI;

namespace Pandorai.Dialogues
{
	public class Dialogue
	{
		public string NPCName;
		public string PlayerName;

		private List<DialogueNode> _dialogueNodes = new List<DialogueNode>();
		private List<DialogueOption> _dialogueOptions = new List<DialogueOption>();
		private List<CustomBool> _customBools = new List<CustomBool>();

		private Main _game;
		private string _name;

		public Dialogue(string dialogueName, Main game)
		{
			_name = dialogueName;
			string fullName = dialogueName + ".txt";
			_dialogueNodes = DialogueManager.ReadDialogueFile(fullName);
			_game = game;
		}

		public Dialogue Clone()
		{
			return new Dialogue(_name, _game);
		}

		public DialogueNode GetNode(int index)
		{
			return _dialogueNodes.Find(node => node.Index == index);
		}

		public void ReadNode(int index)
		{
			MessageLog.Hide();

			if (index == -1)
			{
				return;
			}

			DialogueNode ourNode = GetNode(index);

			// used to check whether actions in <else> brackets should be executed
			// gets resetted after ending the else statement so that it can be used again by next if statements
			bool failedIf = false;
			// execute all the actions of the node in order
			for (int i = 0; i < ourNode.Actions.Count; i++)
			{
				DialogueAction action = ourNode.Actions[i];

				if (action.Type == "if")
				{
					CustomBool check = _customBools.Find(cBool => cBool.Name == action.Value);

					if (check != null && !check.Value)
					{
						i = ourNode.Actions.FindIndex(item => item.Type == "/if");
						failedIf = true;
						continue;
					}
				}
				else if (action.Type == "else" && !failedIf)
				{
					failedIf = false;
					i = ourNode.Actions.FindIndex(item => item.Type == "/else");
					continue;
				}
				else if (action.Type == "method")
				{
					// get function name and its parameters
					Match splitFunc = Regex.Match(action.Value, @"(.+)\((.+)\)");
					string funcName = splitFunc.Groups[1].Value;

					object[] funcParams;
					if (funcName == "SetLead")
					{
						funcParams = splitFunc.Groups[2].Value.Split(',').ToList<object>().Concat(new[] { ourNode }).ToArray();
					}
					else
					{
						funcParams = splitFunc.Groups[2].Value.Split(',').ToList<object>().ToArray();
					}

					// call the given function/method
					MethodInfo method = GetType().GetMethod(funcName);
					if (method != null)
					{
						method.Invoke(this, funcParams);
					}
					else
					{
						string warningMsg = string.Format(
							$@"Action method doesn't exist.
						 At node with index:  {ourNode.Index}
						 The incorrect method name is {funcName}
						 Check if there aren't any misspellings"
						);

						warningMsg = Regex.Replace(warningMsg, @"^\s+", string.Empty, RegexOptions.Multiline);

						Console.WriteLine(warningMsg);
					}
				}
				else if(action.Type == "effect")
				{
					Match nameParamMatch = Regex.Match(action.Value, @"(.+)\((.+)\)");
					var effectName = nameParamMatch.Groups[1].Value;

					var effectParams = nameParamMatch.Groups[2].Value;
					var effectAttributes = effectParams.Split(';');

					Type effectType = TypeLegends.Effects[effectName];
					Effect effectInstance = (Effect)Activator.CreateInstance(effectType);

					foreach (var attribute in effectAttributes)
					{
						var split = attribute.Split('=');
						var attributeName = split[0];
						var attributeValue = split[1];
						effectInstance.SetAttribute(attributeName, attributeValue);
					}

					effectInstance.Use(Main.Game.Player.PossessedCreature);
				}
			}
			// check for type and do something about it
			if (ourNode.Type == "Exit")
			{
				DialogueManager.SetNameLabel();
				DialogueManager.SetTextContent();
				DialogueManager.HideOptions();
			}
			else if (ourNode.Type == "Start")
			{
				DialogueManager.SetNameLabel(NPCName);
				DialogueManager.SetTextContent();
				DialogueManager.ShowOptions(_dialogueOptions, this);

				ReadNode(ourNode.Lead);
			}
			else if (ourNode.Type == "HeroSay")
			{
				DialogueManager.SetNameLabel(PlayerName);
				DialogueManager.SetTextContent(ourNode.Content);
				DialogueManager.HideOptions();

				// wait for user input to continue to the leading node
				WaitForUserConfirm(ourNode.Lead);
			}
			else if (ourNode.Type == "NPCSay")
			{
				DialogueManager.SetNameLabel(NPCName);
				DialogueManager.SetTextContent(ourNode.Content);
				DialogueManager.HideOptions();

				// wait for user input to continue to the leading node
				WaitForUserConfirm(ourNode.Lead);
			}
			else if (ourNode.Type == "HeroOption")
			{
				RemoveOption(ourNode.Index.ToString());
				ReadNode(ourNode.Lead);
			}
			else if (ourNode.Type == "ForeverHeroOption")
			{
				ReadNode(ourNode.Lead);
			}
		}

		public void WaitForUserConfirm(int index)
		{
			KeyPressedHandler handler = null;
			handler	= key =>
			{
				if (key == Keys.Space)
				{
					ContinueNode(index);
					_game.InputManager.SingleKeyPress -= handler;
				}
			};

			_game.InputManager.SingleKeyPress += handler;
		}

		public void ContinueNode(int index)
		{
			if (index == -1)
			{
				// this is the default node
				// it will take you back to hero options
				// nothing special happens
				DialogueManager.HideOptions();
				DialogueManager.SetNameLabel(NPCName);
				DialogueManager.SetTextContent();
				DialogueManager.ShowOptions(_dialogueOptions, this);
			}
			else
			{
				// the rest - "normal" nodes
				ReadNode(index);
			}

			if(index == -2)
			{
				_game.Player.IsInteractingWithSomeone = false;
				MessageLog.Show();
			}
		}

		// Methods callable from dialogue nodes
		public void SetHeroName(string name)
		{
			PlayerName = name;
		}

		public void SetNPCName(string name)
		{
			NPCName = name;
		}

		public void SetBool(string name, string value)
		{
			bool boolValue = value == "true";

			if (_customBools.Find(cBool => cBool.Name == name) == null)
			{
				_customBools.Add(new CustomBool(name, boolValue));
			}
			else
			{
				_customBools.Find(cBool => cBool.Name == name).Value = boolValue;
			}
		}

		public void SetLead(string index, DialogueNode callerNode)
		{
			callerNode.Lead = int.Parse(index);
		}

		public void GiveHeroItem(string itemName)
		{
			_game.Player.PossessedCreature.Inventory.AddElement(ItemLoader.GetItem(itemName));
		}

		public void AddOption(string index)
		{
			int intIndex = int.Parse(index);

			if (_dialogueOptions.Any(item => item.NodeIndex == intIndex))
			{
				Console.WriteLine(string.Format("Option nr {0} already exists", intIndex));
				return;
			}

			DialogueNode optionNode = _dialogueNodes.Find(item => item.Index == intIndex);
			bool isForever = optionNode.Type == "ForeverHeroOption";
			string optContent = optionNode.Content;

			var dialogueOption = new DialogueOption(optContent, intIndex, isForever);

			var conditions = optionNode.Actions.Where(x => x.Type == "condition");
			foreach (var condition in conditions)
			{
				Match nameParamMatch = Regex.Match(condition.Value, @"(.+)\((.+)\)");
				var conditionName = nameParamMatch.Groups[1].Value;

				var conditionParams = nameParamMatch.Groups[2].Value;
				var conditionAttributes = conditionParams.Split(';');

				Type conditionType = TypeLegends.Conditions[conditionName];
				Condition conditionInstance = (Condition)Activator.CreateInstance(conditionType);

				foreach (var attribute in conditionAttributes)
				{
					var split = attribute.Split('=');
					var attributeName = split[0];
					var attributeValue = split[1];
					conditionInstance.SetAttribute(attributeName, attributeValue);
				}

				dialogueOption.Conditions.Add(conditionInstance);
			}

			_dialogueOptions.Add(dialogueOption);
		}

		public void RemoveOption(string index)
		{
			int intIndex = int.Parse(index);
			_dialogueOptions.Remove(_dialogueOptions.Find(item => item.NodeIndex == intIndex));
		}
	}
}

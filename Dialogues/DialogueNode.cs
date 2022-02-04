using System.Collections.Generic;

namespace Pandorai.Dialogues
{
	public class DialogueNode
	{
		public int Index = -3;
		public string Type;
		public string Content = null;
		public int Lead = -1;
		public int OptionIndex = 0;

		public List<DialogueAction> Actions = new List<DialogueAction>();

		public DialogueAction GetAction(string id)
		{
			return Actions.Find(a => a.Value == id);
		}
	}
}
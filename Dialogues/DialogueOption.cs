using System.Collections.Generic;
using Pandorai.Conditions;

namespace Pandorai.Dialogues
{
	public class DialogueOption
	{
		public string Content;
		public int NodeIndex;
		public bool IsForever;
		public List<Condition> Conditions = new();

		public DialogueOption(string content, int nodeIndex, bool isForever)
		{
			Content = content;
			NodeIndex = nodeIndex;
			IsForever = isForever;
		}
	}
}
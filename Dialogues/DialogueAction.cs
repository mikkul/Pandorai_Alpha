﻿using System;

namespace Pandorai.Dialogues
{
	public class DialogueAction
	{
		public string Type = null;
		public string Value = null;
		public Action Action = null;

		public DialogueAction(string type, string value)
		{
			Type = type;
			Value = value;
		}
	}
}


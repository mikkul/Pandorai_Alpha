using Pandorai.Dialogues;

namespace Pandorai.Creatures.Behaviours
{
	public class Talkative : Behaviour
	{
		private Dialogue dialogue;

		public override void SetAttribute(string name, string value)
		{
			if (name == "DialogueName")
			{
				dialogue = new Dialogue(value);
			}
		}

		public override Behaviour Clone()
		{
			return new Talkative
			{
				dialogue = dialogue.Clone(),
			};
		}

		public override void Bind()
		{
			Owner.Interacted += Talk;
		}

		private void Talk(Creature incomingCreature)
		{
			if(!incomingCreature.IsPossessedCreature())
			{
				return;
			}

			Main.Game.Player.IsInteractingWithSomeone = true;
			dialogue.ReadNode(0);
		}
	}
}

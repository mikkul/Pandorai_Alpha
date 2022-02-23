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
				dialogue = new Dialogue(value, Game1.game);
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

			Game1.game.Player.IsInteractingWithSomeone = true;
			dialogue.ReadNode(0);
		}
	}
}

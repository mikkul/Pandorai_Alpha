using Pandorai.Dialogues;

namespace Pandorai.Creatures.Behaviours
{
	public class Talkative : Behaviour
	{
		public Dialogue Dialogue;

		public override void SetAttribute(string name, string value)
		{
			if (name == "DialogueName")
			{
				Dialogue = new Dialogue(value);
				Dialogue.Init();
			}
		}

		public override Behaviour Clone()
		{
			return new Talkative
			{
				Dialogue = Dialogue.Clone(),
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
			Dialogue.ReadNode(0);
		}
	}
}

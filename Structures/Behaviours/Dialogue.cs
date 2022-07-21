using Pandorai.Creatures;

namespace Pandorai.Structures.Behaviours
{
    public class Dialogue : Behaviour
    {
        private Pandorai.Dialogues.Dialogue _dialogue;

        public override void SetAttribute(string name, string value)
        {
            if (name == "DialogueName")
			{
				_dialogue = new Pandorai.Dialogues.Dialogue(value);
			}
        }

        public override Behaviour Clone()
        {
            return new Dialogue
            {
                _dialogue = _dialogue.Clone(),
            };
        }

		public override void Bind()
		{
			Structure.Interacted += Interact;
		}

		public override void Unbind()
		{
			Structure.Interacted -= Interact;
		}

        public override void Interact(Creature creature)
        {
            if(creature != Main.Game.Player.PossessedCreature)
            {
                return;
            }

            Main.Game.Player.IsInteractingWithSomeone = true;
            _dialogue.ReadNode(0);
        }

        public override void ForceHandler(ForceType force)
        {
            
        }
    }
}
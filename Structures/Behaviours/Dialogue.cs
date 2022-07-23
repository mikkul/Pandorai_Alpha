using Pandorai.Creatures;

namespace Pandorai.Structures.Behaviours
{
    public class Dialogue : Behaviour
    {
        public Pandorai.Dialogues.Dialogue DialogueObject;

        public override void SetAttribute(string name, string value)
        {
            if (name == "DialogueName")
			{
				DialogueObject = new Pandorai.Dialogues.Dialogue(value);
                DialogueObject.Init();
			}
        }

        public override Behaviour Clone()
        {
            return new Dialogue
            {
                DialogueObject = DialogueObject.Clone(),
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
            DialogueObject.ReadNode(0);
        }

        public override void ForceHandler(ForceType force)
        {
            
        }
    }
}
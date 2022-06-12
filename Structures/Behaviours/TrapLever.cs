using Pandorai.Creatures;

namespace Pandorai.Structures.Behaviours
{
    public class TrapLever : Behaviour
    {
        public int ActivatedTexture;
        public bool Activated;
        public int Id = -1;

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
            if(Activated)
            {
                return;
            }
            Activated = true;

            Structure.Texture = ActivatedTexture;

            var correspondingTrap = StructureManager.Structures.Find(x => 
            {
                var trapBehaviour = x.GetBehaviour<Trap>();
                return trapBehaviour != null && trapBehaviour.Id == Id;
            });
            if(correspondingTrap == null)
            {
                return;
            }

            correspondingTrap.GetBehaviour<Trap>().Activate();
        }

        public override Behaviour Clone()
        {
            return new TrapLever
            {
                Id = Id,
                ActivatedTexture = ActivatedTexture,
            };
        }

        public override void ForceHandler(ForceType force)
        {
        }

        public override void SetAttribute(string name, string value)
        {
            if(name == "ActivatedTexture")
            {
                ActivatedTexture = int.Parse(value);
            }
        }
    }
}
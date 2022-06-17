using Pandorai.Creatures;

namespace Pandorai.Structures.Behaviours
{
    public class CreatureSpawnPoint : Behaviour
    {
        public string CreatureName { get; set; }

        public override void SetAttribute(string name, string value)
        {
            if(name == "CreatureName")
            {
                CreatureName = value;
            }
        }

        public override Behaviour Clone()
        {
            return new CreatureSpawnPoint
            {
                CreatureName = CreatureName,
            };
        }

		public override void Bind()
		{
		}

		public override void Unbind()
		{
		}

        public override void Interact(Creature creature)
        {
        }

        public override void ForceHandler(ForceType force)
        {
        }
    }
}
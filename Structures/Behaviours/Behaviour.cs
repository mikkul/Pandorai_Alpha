using Pandorai.Creatures;

namespace Pandorai.Structures.Behaviours
{
	public abstract class Behaviour
	{
		public Structure Structure { get; set; }

		public abstract void Bind();

		public abstract void Unbind();

		public abstract Behaviour Clone();

		public abstract void SetAttribute(string name, string value);

		public abstract void Interact(Creature creature);

		public abstract void ForceHandler(ForceType force);
	}
}

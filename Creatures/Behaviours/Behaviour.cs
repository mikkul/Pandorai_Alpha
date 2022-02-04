namespace Pandorai.Creatures.Behaviours
{
	public abstract class Behaviour
	{
		protected Creature owner;
		public Creature Owner { 
			get => owner; 
			set
			{
				owner = value;
				Bind();
			}
		}

		public abstract Behaviour Clone();

		public abstract void Bind();

		public abstract void SetAttribute(string name, string value);
	}
}

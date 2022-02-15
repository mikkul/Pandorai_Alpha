using Pandorai.Creatures;

namespace Pandorai.Effects
{
	public abstract class Effect
	{
		public abstract void Use(Creature usingCreature);

		public abstract void SetAttribute(string name, string value);
	}
}

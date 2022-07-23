using Pandorai.Creatures;

namespace Pandorai.Triggers
{
	public static partial class Trigger
	{
        private static Creature _spikesCreature = new Creature
		{
			TemplateName = "Spikes",
			Stats = new CreatureStats(_spikesCreature)
			{
				Strength = 45,
			},
		};

		public static void SpikesTrigger(Creature incomingCreature)
		{
			incomingCreature.OnGotHit(_spikesCreature);
		}
	}
}

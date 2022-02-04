using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Rendering;

namespace Pandorai.Triggers
{
	public static partial class Trigger
	{
		//private static StoneGuardian dummyStoneGuardian = new StoneGuardian(null);

		public static void StoneGuardianAwake(Creature incomingCreature)
		{
			//if (!dummyStoneGuardian.EnemyClasses.Contains(incomingCreature.Class)) return;

			//float range = 15f;

			//foreach (var creature in incomingCreature.game.CreatureManager.Creatures)
			//{
			//	if (creature.GetType() != typeof(StoneGuardian)) continue;

			//	float dist = Vector2.DistanceSquared(creature.Position, incomingCreature.Position);
			//	if (dist < (range * incomingCreature.game.Map.TileSize) * (range * incomingCreature.game.Map.TileSize))
			//	{
			//		StoneGuardian otherGuardian = (StoneGuardian)creature;
			//		if (!otherGuardian.IsAwaken)
			//		{
			//			otherGuardian.Awake();
			//		}
			//	}
			//}
		}
	}
}

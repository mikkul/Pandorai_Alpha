using System.Linq;
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Creatures.Behaviours;

namespace Pandorai.Triggers
{
	public static partial class Trigger
	{
		private static Creature _dummyStoneGuardian;

		public static void StoneGuardianAwake(Creature incomingCreature)
		{
			if(_dummyStoneGuardian == null)
			{
				_dummyStoneGuardian = Main.Game.CreatureManager.Creatures.FirstOrDefault(x => x.TemplateName == "StoneGuardian");
			}

			if (_dummyStoneGuardian == null || !_dummyStoneGuardian.EnemyClasses.Contains(incomingCreature.Class)) return;

			float range = 15f;

			foreach (var creature in Main.Game.CreatureManager.Creatures)
			{
				if (creature.TemplateName != "StoneGuardian") continue;

				float dist = Vector2.DistanceSquared(creature.Position, incomingCreature.Position);
				if (dist < (range * Main.Game.Map.TileSize) * (range * Main.Game.Map.TileSize))
				{
					var awakeningBehaviour = creature.GetBehaviour<Awakening>() as Awakening;
					if(awakeningBehaviour != null)
					{
						awakeningBehaviour.Awake(incomingCreature);
					}
				}
			}
		}
	}
}

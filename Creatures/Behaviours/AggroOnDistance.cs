using Microsoft.Xna.Framework;

namespace Pandorai.Creatures.Behaviours
{
	public class AggroOnDistance : Behaviour
	{
		public int Range;

		public override Behaviour Clone()
		{
			return new AggroOnDistance
			{
				Range = Range,
			};
		}

		public override void SetAttribute(string name, string value)
		{
			if (name == "Range")
			{
				Range = int.Parse(value);
			}
		}

		public override void Bind()
		{
			Owner.TurnCame += Work;
		}

		private void Work()
		{
			float lowestDist = int.MaxValue;

			foreach (var creature in Owner.game.CreatureManager.Creatures)
			{
				if (Owner.EnemyClasses.Contains(creature.Class))
				{
					float dist = Vector2.DistanceSquared(creature.Position, Owner.Position);
					int realAggroRange = Range - creature.Stealth;
					if (dist > (realAggroRange * Owner.game.Map.TileSize) * (realAggroRange * Owner.game.Map.TileSize))
					{
						continue;
					}
					else
					{
						if (dist < lowestDist)
						{
							lowestDist = dist;
							Owner.Target = creature.MapIndex;
						}
					}
				}
			}
		}
	}
}
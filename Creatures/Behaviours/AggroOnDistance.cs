using Microsoft.Xna.Framework;
using Pandorai.Sounds;

namespace Pandorai.Creatures.Behaviours
{
	public class AggroOnDistance : Behaviour
	{
		private Creature _lastTargetCreature;
		private int _targetResetCounter;
		private int _targetResetThreshold = 10;

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

			Creature targetCreature = null;

			foreach (var creature in Owner.game.CreatureManager.Creatures)
			{
				if (Owner.EnemyClasses.Contains(creature.Class))
				{
					float dist = Vector2.DistanceSquared(creature.Position, Owner.Position);
					int realAggroRange = Range - creature.Stats.Stealth;
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
							targetCreature = creature;
						}
					}
				}
			}
			if(Owner.Target != Owner.MapIndex)
			{
				if(targetCreature != _lastTargetCreature)
				{
					SoundManager.PlaySound(Owner.Sounds.Aggro);
				}

				_lastTargetCreature = targetCreature;
			}

			if(targetCreature == null)
			{
				_targetResetCounter++;
				if(_targetResetCounter >= _targetResetThreshold)
				{
					_targetResetCounter = 0;
					_lastTargetCreature = null;
				}
			}
		}
	}
}
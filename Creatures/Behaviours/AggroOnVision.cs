using Pandorai.ParticleSystems;
using Microsoft.Xna.Framework;
using Pandorai.Sounds;

namespace Pandorai.Creatures.Behaviours
{
	public class AggroOnVision : Behaviour
	{
		private Creature _lastTargetCreature;
		private int _targetResetCounter;
		private int _targetResetThreshold = 10;

		public override void Bind()
		{
			Owner.TurnCame += CheckAggro;
		}

		void CheckAggro()
		{
			Vision vision = Owner.GetBehaviour<Vision>() as Vision;
			if (vision == null) return;

			bool hasTarget = false;

			foreach (var tile in vision.VisibleTiles)
			{
				var tryCreature = Owner.game.CreatureManager.GetCreature(tile);
				if (tryCreature == null) continue;

				float dist = Vector2.DistanceSquared(tryCreature.Position, Owner.Position);
				var normalVision = Owner.GetBehaviour<NormalVision>() as NormalVision;
				if(normalVision != null)
				{
					int realAggroRange = normalVision.RangeLimit - tryCreature.Stats.Stealth;
					if (dist > (realAggroRange * Owner.game.Map.TileSize) * (realAggroRange * Owner.game.Map.TileSize))
					{
						continue;
					}
				}

				if(Owner.EnemyClasses.Contains(tryCreature.Class))
				{
					if(_lastTargetCreature != tryCreature)
					{
						SoundManager.PlaySound(Owner.Sounds.Aggro);
						var aggroFlash = new PSImplosion(Owner.Position, 25, Main.Game.fireParticleTexture, 1000, Main.Game.Map.TileSize / 2, 20, Color.Purple, true, Main.Game);
						ParticleSystemManager.AddSystem(aggroFlash, true);
					}

					Owner.Target = tryCreature.MapIndex;

					_lastTargetCreature = tryCreature;
					hasTarget = true;
					break;
				}
			}

			if(!hasTarget)
			{
				_targetResetCounter++;
				if(_targetResetCounter >= _targetResetThreshold)
				{
					_targetResetCounter = 0;
					_lastTargetCreature = null;
				}
			}
		}

		public override Behaviour Clone()
		{
			return new AggroOnVision()
			{

			};
		}

		public override void SetAttribute(string name, string value)
		{
			
		}
	}
}

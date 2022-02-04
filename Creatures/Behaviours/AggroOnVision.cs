using Pandorai.ParticleSystems;
using Microsoft.Xna.Framework;
using System;

namespace Pandorai.Creatures.Behaviours
{
	public class AggroOnVision : Behaviour
	{
		public override void Bind()
		{
			Owner.TurnCame += CheckAggro;
		}

		void CheckAggro()
		{
			Vision vision = Owner.GetBehaviour<Vision>() as Vision;
			if (vision == null) return;

			foreach (var tile in vision.VisibleTiles)
			{
				var tryCreature = Owner.game.CreatureManager.GetCreature(tile);
				if (tryCreature == null) continue;

				if(Owner.EnemyClasses.Contains(tryCreature.Class))
				{
					Owner.Target = tryCreature.MapIndex;
					var aggroFlash = new PSImplosion(Owner.Position, 25, Game1.game.fireParticleTexture, 1000, Game1.game.Map.TileSize / 2, 20, Color.Purple, true, Game1.game);
					ParticleSystemManager.AddSystem(aggroFlash, true);
					break;
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

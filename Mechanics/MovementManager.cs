using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Sounds;

namespace Pandorai.Mechanics
{
	public static class MovementManager
	{
		public static void BeginCreatureMovement(Creature creature, Point desiredPoint)
		{
			if (!creature.IsAlive) return;

			creature.StartPosition = creature.Position;
			creature.TargetPosition = desiredPoint.ToVector2() * creature.Game.Map.TileSize;

			creature.IsMoving = true;
			creature.SetMovementTexture(desiredPoint);
			SoundManager.PlaySound(creature.Sounds.Footstep, 0.35f);

			creature.Game.Map.RequestTileCollisionFlagChange(creature.MapIndex, false);
			creature.Game.Map.RequestTileCollisionFlagChange(desiredPoint, true);

			if(creature == creature.Game.Player.PossessedCreature)
			{
				creature.Game.TurnManager.PlayerIsReady();
			}
			/*else
			{
				creature.game.TurnManager.EnemyIsReady();
			}*/
		}
	}
}

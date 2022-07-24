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
			creature.TargetPosition = desiredPoint.ToVector2() * Main.Game.Map.TileSize;

			creature.IsMoving = true;
			creature.SetMovementTexture(desiredPoint);
			SoundManager.PlaySound(creature.Sounds.Footstep, creature.Position, 0.35f);

			Main.Game.Map.RequestTileCollisionFlagChange(creature.MapIndex, false);
			Main.Game.Map.RequestTileCollisionFlagChange(desiredPoint, true);

			if(creature == Main.Game.Player.PossessedCreature)
			{
				Main.Game.TurnManager.PlayerIsReady();
			}
			/*else
			{
				creature.game.TurnManager.EnemyIsReady();
			}*/
		}
	}
}

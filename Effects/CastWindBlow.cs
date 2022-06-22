using Pandorai.Creatures;
using System;
using System.Linq;
using Pandorai.Tilemaps;
using Microsoft.Xna.Framework;
using Pandorai.ParticleSystems;
using System.Timers;
using Pandorai.Structures;
using Pandorai.Utility;
using Pandorai.Sounds;

namespace Pandorai.Effects
{
	public class CastWindBlow : Effect
	{
		public int Damage;
		public int Range;

		public override void SetAttribute(string name, string value)
		{
			if (name == "Damage")
			{
				Damage = int.Parse(value);
			}
			else if (name == "Range")
			{
				Range = int.Parse(value);
			}
		}

		public override void Use(Creature user)
		{
			if (user == user.Game.Player.PossessedCreature && user.Game.Player.IsInteractingWithSomeone)
            {
                return;
            }

			var interactableTiles = GenHelper.GetNeighbours(user.MapIndex).ToList();

			user.Game.Map.HighlightTiles(interactableTiles);

			user.Game.Map.EnableTileInteraction();

			TileEventHandler UseHandler = null;

			UseHandler = (TileInfo tile) =>
			{
				if (!interactableTiles.Contains(tile.Index))
				{
					user.Game.Map.DisableTileInteraction();
					TileInteractionManager.TileClick -= UseHandler;
					return;
				}

				Point dir = Point.Zero;
				switch (interactableTiles.IndexOf(tile.Index))
				{
					case 0:
						dir = new Point(-1, 0);
						break;
					case 1:
						dir = new Point(1, 0);
						break;
					case 2:
						dir = new Point(0, -1);
						break;
					case 3:
						dir = new Point(0, 1);
						break;
				}
				Vector2 velocity = dir.ToVector2() * 64 * Range;

				user.Game.Player.IsInteractingWithSomeone = true;
				user.Game.Map.DisableTileInteraction();
				TileInteractionManager.TileClick -= UseHandler;

				float time = user.Game.TurnManager.enemyTurnTime * 2;

				var fireballPS = new PSFireball(user.Position, 35, user.Game.smokeParticleTexture, time, velocity, 40, 30, Color.Azure, true, user.Game);

				ParticleSystemManager.AddSystem(fireballPS, true);

				Timer effectTimer = new Timer(time);
				effectTimer.Elapsed += (s, a) =>
				{
					if (user == user.Game.Player.PossessedCreature)
					{
						user.Game.TurnManager.PlayerIsReady();
					}

					user.Game.Player.IsInteractingWithSomeone = false;

					effectTimer.Stop();
					effectTimer.Dispose();
				};

				Point fireballIndex = user.MapIndex;
				double timePassed = 0;
				int pierceLeft = int.MaxValue;
				Timer damageTimer = new Timer(time / Range);
				damageTimer.Elapsed += (s, a) =>
				{
					timePassed += damageTimer.Interval;

					if (timePassed > time)
					{
						damageTimer.Stop();
						damageTimer.Dispose();
						return;
					}

					fireballIndex += dir;

					if (user.Game.Map.GetTile(fireballIndex) == null) return;

					var tryCreature = user.Game.CreatureManager.GetCreature(fireballIndex);
					if (tryCreature != null)
					{
						if (pierceLeft > 0)
						{
                            // push back
                            tryCreature.Move(fireballIndex + dir);

                            // deal damage
							float actualDamage = Damage - Damage * (float)tryCreature.Stats.IceResistance / 100f;
							user.Game.GameStateManager.AddSynchronizedAction(() => tryCreature.GetHit(actualDamage, user));
							pierceLeft--;
						}
					}
					else if (user.Game.Map.GetTile(fireballIndex).CollisionFlag)
					{
						pierceLeft = 0;
						fireballPS.Disintegrate();
					}
				};

				effectTimer.Start();
				damageTimer.Start();
				SoundManager.PlaySound("fireball0");
				DisplayMessage(user);
			};

			TileInteractionManager.TileClick += UseHandler;
		}

        protected override string GetMessage()
        {
            return "You cast an ice projectile";
        }
    }
}

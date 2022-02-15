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
	public class CastIceProjectile : Effect
	{
		public int Damage;
		public int Pierce;
		public int Range;
		public ForceType Force = ForceType.Ice;

		public override void SetAttribute(string name, string value)
		{
			if (name == "Damage")
			{
				Damage = int.Parse(value);
			}
			else if (name == "Pierce")
			{
				Pierce = int.Parse(value);
			}
			else if (name == "Range")
			{
				Range = int.Parse(value);
			}
			else if (name == "Force")
			{
				Force |= (ForceType)Enum.Parse(typeof(ForceType), value);
			}
		}

		public override void Use(Creature user)
		{
			if (user == user.game.Player.PossessedCreature && user.game.Player.IsInteractingWithSomeone) return;

			var interactableTiles = GenHelper.GetNeighbours(user.MapIndex).ToList();

			user.game.Map.HighlightTiles(interactableTiles);

			user.game.Map.EnableTileInteraction();

			TileEventHandler UseHandler = null;

			UseHandler = (TileInfo tile) =>
			{
				if (!interactableTiles.Contains(tile.Index))
				{
					user.game.Map.DisableTileInteraction();
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

				user.game.Player.IsInteractingWithSomeone = true;
				user.game.Map.DisableTileInteraction();
				TileInteractionManager.TileClick -= UseHandler;

				float time = user.game.TurnManager.enemyTurnTime * 2;

				var fireballPS = new PSFireball(user.Position, 35, user.game.smokeParticleTexture, time, velocity, 40, 30, Color.Cyan, true, user.game);

				ParticleSystemManager.AddSystem(fireballPS, true);

				Timer effectTimer = new Timer(time);
				effectTimer.Elapsed += (s, a) =>
				{
					if (user == user.game.Player.PossessedCreature)
					{
						user.game.TurnManager.PlayerIsReady();
					}

					user.game.Player.IsInteractingWithSomeone = false;

					effectTimer.Stop();
					effectTimer.Dispose();
				};

				Point fireballIndex = user.MapIndex;
				double timePassed = 0;
				int pierceLeft = Pierce;
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

					if (user.game.Map.GetTile(fireballIndex) == null) return;

					var tryCreature = user.game.CreatureManager.GetCreature(fireballIndex);
					if (tryCreature != null)
					{
						if (pierceLeft > 0)
						{
							float actualDamage = Damage;
							user.game.GameStateManager.AddSynchronizedAction(() => tryCreature.GetHit(actualDamage, user));
							pierceLeft--;
						}
					}
					else if (user.game.Map.GetTile(fireballIndex).MapObject != null && user.game.Map.GetTile(fireballIndex).MapObject.Structure != null)
					{
						if (pierceLeft > 0)
						{
							user.game.GameStateManager.AddSynchronizedAction(() =>
							{
								if (user.game.Map.GetTile(fireballIndex).MapObject == null) return;

								if (user.game.Map.GetTile(fireballIndex).MapObject.Structure.UseForce(Force) == ForceResult.None)
								{
									pierceLeft = 0;
									fireballPS.Disintegrate();
								}
							});
							pierceLeft--;
						}
					}
					else if (user.game.Map.GetTile(fireballIndex).CollisionFlag)
					{
						pierceLeft = 0;
						fireballPS.Disintegrate();
					}
				};

				effectTimer.Start();
				damageTimer.Start();
				SoundManager.PlaySound("fireball0");
			};

			TileInteractionManager.TileClick += UseHandler;
		}
	}
}

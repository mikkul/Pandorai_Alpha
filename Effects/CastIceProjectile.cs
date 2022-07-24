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
			if (user == Main.Game.Player.PossessedCreature && Main.Game.Player.IsInteractingWithSomeone) return;

			var interactableTiles = GenHelper.GetNeighbours(user.MapIndex).ToList();

			Main.Game.Map.HighlightTiles(interactableTiles);

			Main.Game.Map.EnableTileInteraction();

			TileEventHandler UseHandler = null;

			UseHandler = (TileInfo tile) =>
			{
				if (!interactableTiles.Contains(tile.Index))
				{
					Main.Game.Map.DisableTileInteraction();
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

				Main.Game.Player.IsInteractingWithSomeone = true;
				Main.Game.Map.DisableTileInteraction();
				TileInteractionManager.TileClick -= UseHandler;

				float time = Main.Game.TurnManager.enemyTurnTime * 2;

				var fireballPS = new PSFireball(user.Position, 35, "SmokeParticleTexture", time, velocity, 40, 30, Color.Cyan, true);

				ParticleSystemManager.AddSystem(fireballPS, true);

				Timer effectTimer = new Timer(time);
				effectTimer.Elapsed += (s, a) =>
				{
					if (user == Main.Game.Player.PossessedCreature)
					{
						Main.Game.TurnManager.PlayerIsReady();
					}

					Main.Game.Player.IsInteractingWithSomeone = false;

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

					if (Main.Game.Map.GetTile(fireballIndex) == null) return;

					var tryCreature = Main.Game.CreatureManager.GetCreature(fireballIndex);
					if (tryCreature != null)
					{
						if (pierceLeft > 0)
						{
							float actualDamage = Damage - Damage * (float)tryCreature.Stats.IceResistance / 100f;
							Main.Game.GameStateManager.AddSynchronizedAction(() => tryCreature.GetHit(actualDamage, user));
							pierceLeft--;
						}
					}
					else if (Main.Game.Map.GetTile(fireballIndex).MapObject != null && Main.Game.Map.GetTile(fireballIndex).MapObject.Structure != null)
					{
						if (pierceLeft > 0)
						{
							Main.Game.GameStateManager.AddSynchronizedAction(() =>
							{
								if (Main.Game.Map.GetTile(fireballIndex).MapObject == null) return;

								if (Main.Game.Map.GetTile(fireballIndex).MapObject.Structure.UseForce(Force) == ForceResult.None)
								{
									pierceLeft = 0;
									fireballPS.Disintegrate();
								}
							});
							pierceLeft--;
						}
					}
					else if (Main.Game.Map.GetTile(fireballIndex).CollisionFlag)
					{
						pierceLeft = 0;
						fireballPS.Disintegrate();
					}
				};

				effectTimer.Start();
				damageTimer.Start();
				SoundManager.PlaySound("fireball0", user.Position);
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

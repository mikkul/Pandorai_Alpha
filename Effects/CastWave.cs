using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.ParticleSystems;
using Pandorai.Sounds;
using Pandorai.Utility;
using System.Timers;

namespace Pandorai.Effects
{
	public class CastWave : Effect
	{
		public int Damage;
		public int Range;

		public override void SetAttribute(string name, string value)
		{
			if(name == "Damage")
			{
				Damage = int.Parse(value);
			}
			else if(name == "Range")
			{
				Range = int.Parse(value);
			}
		}

		public override void Use(Creature user)
		{
			if (user == user.game.Player.PossessedCreature && user.game.Player.IsInteractingWithSomeone) return;

			user.game.Player.IsInteractingWithSomeone = true;

			float time = 1000;

			Game1.game.Camera.CameraShake = new Rendering.Shake((int)time + 250, 60, 30, Game1.game.mainRng);
			Game1.game.Camera.ShakeCamera();
			SoundManager.PlaySound("explosion_low");

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

			int rangeCovered = 0;
			Timer damageTimer = new Timer(time / Range);
			damageTimer.Elapsed += (s, a) =>
			{
				PSExplosion wavePS;
				if(rangeCovered % 2 == 0)
				{
					wavePS = new PSExplosion(user.Position, 150, user.game.smokeParticleTexture, time * 1.4f, 64 * Range + 128, 100, Helper.GetColorFromHex("#8610e0"), true, user.game);
				}
				else
				{
					wavePS = new PSExplosion(user.Position, 150, user.game.smokeParticleTexture, time * 1.4f, 64 * Range + 128, 85, Helper.GetColorFromHex("#5f00b3"), true, user.game);
				}

				ParticleSystemManager.AddSystem(wavePS, true);

				if (++rangeCovered > Range)
				{
					damageTimer.Stop();
					damageTimer.Dispose();
					return;
				}

				var damagedTiles = GenHelper.GetCircleBorderPoints(user.MapIndex.X, user.MapIndex.Y, rangeCovered);
				foreach (var tile in damagedTiles)
				{
					Creature tryCreature;
					if((tryCreature = Game1.game.CreatureManager.GetCreature(tile)) != null)
					{
						Game1.game.GameStateManager.AddSynchronizedAction(() => tryCreature.GetHit(Damage, user));
					}
				}
			};

			effectTimer.Start();
			damageTimer.Start();

			SoundManager.PlaySound("Spell_02");
			DisplayMessage(user);
		}

        protected override string GetMessage()
        {
            return "You cast a deadly wave";
        }
    }
}

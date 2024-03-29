﻿using Microsoft.Xna.Framework;
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
			if (user == Main.Game.Player.PossessedCreature && Main.Game.Player.IsInteractingWithSomeone) return;

			Main.Game.Player.IsInteractingWithSomeone = true;

			float time = 1000;

			Main.Game.Camera.CameraShake = new Rendering.Shake((int)time + 250, 60, 30, Main.Game.MainRng);
			Main.Game.Camera.ShakeCamera();
			SoundManager.PlaySound("explosion_low", user.Position);

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

			int rangeCovered = 0;
			Timer damageTimer = new Timer(time / Range);
			damageTimer.Elapsed += (s, a) =>
			{
				PSExplosion wavePS;
				if(rangeCovered % 2 == 0)
				{
					wavePS = new PSExplosion(user.Position, 150, "SmokeParticleTexture", time * 1.4f, 64 * Range + 128, 100, Helper.GetColorFromHex("#8610e0"), true);
				}
				else
				{
					wavePS = new PSExplosion(user.Position, 150, "SmokeParticleTexture", time * 1.4f, 64 * Range + 128, 85, Helper.GetColorFromHex("#5f00b3"), true);
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
					if((tryCreature = Main.Game.CreatureManager.GetCreature(tile)) != null)
					{
						Main.Game.GameStateManager.AddSynchronizedAction(() => tryCreature.GetHit(Damage, user));
					}
				}
			};

			effectTimer.Start();
			damageTimer.Start();

			SoundManager.PlaySound("Spell_02", user.Position);
			DisplayMessage(user);
		}

        protected override string GetMessage()
        {
            return "You cast a deadly wave";
        }
    }
}

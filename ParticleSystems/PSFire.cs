using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;

namespace Pandorai.ParticleSystems
{
	public class PSFire : ParticleSystem
	{
		private float particleSpeed;
		private int regenSpan;
		private float displacementOffsetX;
		private Random rng = new Random();

		private float regenCounter = 0;

		public PSFire(Vector2 position, int noOfParticles, Texture2D particleTexture, float particleLifeMs, float particleSpd, int partSize, int regenSpanMs, float fireWidth, Color color, bool isWorldCoords, Game1 _game)
		{
			centralPosition = position;
			numberOfParticles = noOfParticles;
			baseTexture = particleTexture;
			maxParticleLife = particleLifeMs;
			particleSpeed = particleSpd;
			particleSize = partSize;
			regenSpan = regenSpanMs;
			baseColor = color;
			isWorldCoordinates = isWorldCoords;
			displacementOffsetX = fireWidth;
			game = _game;
		}

		protected override Particle GenerateParticle()
		{
			var velX = rng.NextFloat(-displacementOffsetX * 0.2f, displacementOffsetX * 0.2f);
			var velY = rng.NextFloat(-particleSpeed * 0.1f, -particleSpeed);
			Vector2 velocity = new Vector2(velX, velY);
			float randomLife = rng.Next(0, 50);
			Vector2 pos = new Vector2(centralPosition.X + rng.NextFloat(-displacementOffsetX, displacementOffsetX), centralPosition.Y);
			return new Particle(pos, velocity, randomLife);
		}

		public override bool Update(GameTime dt)
		{
			Particle part;

			for (int i = particles.Count - 1; i >= 0; i--)
			{
				part = particles[i];

				part.Update(dt, Vector2.Zero, game.Options.UnitMultiplier);
				if (part.LifeTime >= maxParticleLife)
				{
					particles.RemoveAt(i);
				}
			}

			if (particles.Count < numberOfParticles)
			{
				int particlesToAdd = numberOfParticles - particles.Count;
				float timePerParticle = regenSpan / particlesToAdd;
				regenCounter += (float)dt.ElapsedGameTime.TotalMilliseconds;
				if (regenCounter >= timePerParticle)
				{
					regenCounter -= (float)Math.Floor(regenCounter);
					particles.Add(GenerateParticle());
				}
			}

			return true;
		}
	}
}

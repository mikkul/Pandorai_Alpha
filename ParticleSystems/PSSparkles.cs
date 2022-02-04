using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;
using System.Diagnostics;

namespace Pandorai.ParticleSystems
{
	public class PSSparkles : ParticleSystem
	{
		private float particleSpeed;
		private int regenSpan;
		private Random rng = new Random();

		private float regenCounter = 0;

		public PSSparkles(Vector2 position, int noOfParticles, Texture2D particleTexture, float particleLifeMs, float particleSpd, int partSize, int regenSpanMs, Color color, bool isWorldCoords, Game1 _game)
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
			game = _game;

			/*for (int i = 0; i < numberOfParticles; i++)
			{
				particles.Add(GenerateParticle());
			}*/
		}

		protected override Particle GenerateParticle()
		{
			double range = 20;
			float velX = (float)rng.NextDouble(-particleSpeed, particleSpeed);
			velX += (float)Math.Max(Math.Sign(velX) * range, velX);
			float velY = (float)rng.NextDouble(-particleSpeed, particleSpeed);
			velY += (float)Math.Max(Math.Sign(velY) * range, velY);
			Vector2 velocity = new Vector2(velX, velY);
			float randomLife = rng.Next(0, 50);
			return new Particle(centralPosition, velocity, randomLife);
		}

		public override bool Update(GameTime dt)
		{
			Particle part;

			for (int i = particles.Count - 1; i >= 0; i--)
			{
				part = particles[i];

				part.Update(dt, Vector2.Zero, game.Options.UnitMultiplier);
				if(part.LifeTime >= maxParticleLife)
				{
					particles.RemoveAt(i);
				}
			}

			if(particles.Count < numberOfParticles)
			{
				int particlesToAdd = numberOfParticles - particles.Count;
				float timePerParticle = regenSpan / particlesToAdd;
				regenCounter += (float)dt.ElapsedGameTime.TotalMilliseconds;
				if(regenCounter >= timePerParticle)
				{
					regenCounter -= (float)Math.Floor(regenCounter);
					particles.Add(GenerateParticle());
				}
			}

			return true;
		}
	}
}

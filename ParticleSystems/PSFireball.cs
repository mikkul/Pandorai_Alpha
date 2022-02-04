using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;

namespace Pandorai.ParticleSystems
{
	public class PSFireball : ParticleSystem
	{
		private Vector2 velocity;
		private bool isCentralParticle;
		private float radius;
		private Random rng = new Random();

		PSFireball centralParticle;

		public PSFireball(Vector2 position, int noOfParticles, Texture2D particleTexture, float particleLifeMs, Vector2 particleVelocity, int partSize, float fireballRadius, Color color, bool isWorldCoords, Game1 _game, bool isCenter = false)
		{
			centralPosition = position;
			numberOfParticles = noOfParticles;
			baseTexture = particleTexture;
			maxParticleLife = particleLifeMs * 1.2f;
			velocity = particleVelocity * 1000 / particleLifeMs;
			particleSize = partSize;
			radius = fireballRadius;
			baseColor = color;
			isWorldCoordinates = isWorldCoords;
			game = _game;
			isCentralParticle = isCenter;

			// add one big particle in the middle
			if(!isCenter)
			{
				centralParticle = new PSFireball(position, 1, particleTexture, particleLifeMs, particleVelocity, (int)(particleSize * 3), 0, color, isWorldCoords, _game, true);
				ParticleSystemManager.AddSystem(centralParticle, true);
			}

			for (int i = 0; i < numberOfParticles; i++)
			{
				particles.Add(GenerateParticle());
			}
		}

		public void Disintegrate()
		{
			for (int i = particles.Count - 1; i >= 0; i--)
			{
				particles[i].LifeTime = maxParticleLife - 100;
			}
			if(!isCentralParticle)
			{
				centralParticle.Disintegrate();
			}
		}

		public override bool Update(GameTime dt)
		{
			Particle part;

			for (int i = particles.Count - 1; i >= 0; i--)
			{
				part = particles[i];

				part.Update(dt, Vector2.Zero, game.Options.UnitMultiplier);
				if (part.LifeTime >= maxParticleLife * 0.8f)
				{
					particles.RemoveAt(i);
				}
			}

			if (particles.Count <= 0)
				return false;
			else
				return true;
		}

		protected override Particle GenerateParticle()
		{
			double angle = rng.NextDouble(0, 2 * Math.PI);
			float x = (float)Math.Cos(angle) * radius;
			float y = (float)Math.Sin(angle) * radius;
			Vector2 position = isCentralParticle ? centralPosition : new Vector2(centralPosition.X + x, centralPosition.Y + y);
			return new Particle(position, velocity, 0);
		}
	}
}

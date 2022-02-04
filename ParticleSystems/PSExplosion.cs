using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;

namespace Pandorai.ParticleSystems
{
	public class PSExplosion : ParticleSystem
	{
		private float particleSpeed;
		private Random rng = new Random();
		private const float frictionValue = 1f;

		public PSExplosion(Vector2 position, int noOfParticles, Texture2D particleTexture, float particleLifeMs, float particleSpd, int partSize, Color color, bool isWorldCoords, Game1 _game)
		{
			centralPosition = position;
			numberOfParticles = noOfParticles;
			baseTexture = particleTexture;
			maxParticleLife = particleLifeMs;
			particleSpeed = particleSpd;
			particleSize = partSize;
			baseColor = color;
			isWorldCoordinates = isWorldCoords;
			game = _game;

			for (int i = 0; i < numberOfParticles; i++)
			{
				particles.Add(GenerateParticle());
			}
		}

		public override bool Update(GameTime dt)
		{
			Particle part;

			for (int i = particles.Count - 1; i >= 0; i--)
			{
				part = particles[i];

				Vector2 friction = -part.Velocity * frictionValue;
				part.Update(dt, friction, game.Options.UnitMultiplier);
				if (part.LifeTime >= maxParticleLife)
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
			float randomnessRange = particleSpeed * 0.2f;
			double randAngle = rng.NextDouble(0, 2 * Math.PI);
			float velX = (float)Math.Cos(randAngle) * particleSpeed + rng.NextFloat(0, randomnessRange);
			float velY = (float)Math.Sin(randAngle) * particleSpeed + rng.NextFloat(0, randomnessRange);
			Vector2 velocity = new Vector2(velX, velY);
			float randomLife = rng.Next(0, 50);
			return new Particle(centralPosition, velocity, randomLife);
		}
	}
}

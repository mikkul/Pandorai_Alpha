using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandorai.ParticleSystems
{
	public class PSBloodSplat : ParticleSystem
	{
		private float verticalSpeed;
		private float horizontalScatter;
		private Vector2 gravity;
		private Random rng = new Random();

		public PSBloodSplat(Vector2 position, int noOfParticles, Texture2D particleTexture, float particleLifeMs, float verticalSpd, float horizontalRange, int partSize, Color color, float gravityValue, bool isWorldCoords, Game1 _game)
		{
			centralPosition = position;
			numberOfParticles = noOfParticles;
			baseTexture = particleTexture;
			maxParticleLife = particleLifeMs;
			verticalSpeed = verticalSpd;
			horizontalScatter = horizontalRange;
			particleSize = partSize;
			baseColor = color;
			gravity = new Vector2(0, gravityValue);
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

				part.Update(dt, gravity, game.Options.UnitMultiplier);
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
			double range = 10;
			float velX = (float)rng.NextDouble(-horizontalScatter, horizontalScatter);
			velX += (float)Math.Max(Math.Sign(velX) * range, velX);
			float velY = -(float)rng.NextDouble(verticalSpeed * 0.5, verticalSpeed);
			Vector2 velocity = new Vector2(velX, velY);
			float randomLife = rng.Next(0, 50);
			return new Particle(centralPosition, velocity, randomLife);
		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;
using System.Xml.Serialization;

namespace Pandorai.ParticleSystems
{
	public class PSImplosion : ParticleSystem
	{
		private float radius;
		private Random rng = new Random();
		private float acceleration;

		public PSImplosion(Vector2 position, int noOfParticles, Texture2D particleTexture, float implosionTime, float implosionRadius, int partSize, Color color, bool isWorldCoords, Game1 _game)
		{
			centralPosition = position;
			numberOfParticles = noOfParticles;
			baseTexture = particleTexture;
			maxParticleLife = implosionTime;
			particleSize = partSize;
			radius = implosionRadius;
			baseColor = color;
			isWorldCoordinates = isWorldCoords;
			game = _game;
			acceleration = 2 * implosionRadius / (float)Math.Pow(implosionTime / 1000, 2);

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

				Vector2 accelTowards = Vector2.Normalize(part.Velocity) * acceleration;
				part.Update(dt, accelTowards, game.Options.UnitMultiplier);
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

		public override void Draw(SpriteBatch batch)
		{
			batch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			foreach (var particle in particles)
			{
				float alpha = 1;
				if (!isWorldCoordinates)
				{
					batch.Draw(baseTexture, GetParticleDisplayRectangle(particle.Position), new Color(baseColor, alpha));
				}
				else
				{
					batch.Draw(baseTexture, GetParticleDisplayRectangle(game.Camera.GetViewportPosition(particle.Position)), new Color(baseColor, alpha));
				}
			}
			batch.End();
		}

		protected override Particle GenerateParticle()
		{
			double randAngle = rng.NextDouble(0, 2 * Math.PI);

			float x = (float)Math.Cos(randAngle) * radius;
			float y = (float)Math.Sin(randAngle) * radius;

			Vector2 velocity = Vector2.Normalize(new Vector2(-x, -y)) * 0.001f;
			return new Particle(centralPosition + new Vector2(x, y), velocity, 0);
		}
	}
}

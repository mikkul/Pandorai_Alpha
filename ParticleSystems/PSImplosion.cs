using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;

namespace Pandorai.ParticleSystems
{
	public class PSImplosion : ParticleSystem
	{
		private float _radius;
		private Random _rng = new Random();
		private float _acceleration;

		public PSImplosion(Vector2 position, int noOfParticles, Texture2D particleTexture, float implosionTime, float implosionRadius, int partSize, Color color, bool isWorldCoords, Main game)
		{
			_centralPosition = position;
			_numberOfParticles = noOfParticles;
			_baseTexture = particleTexture;
			_maxParticleLife = implosionTime;
			_particleSize = partSize;
			_radius = implosionRadius;
			_baseColor = color;
			_isWorldCoordinates = isWorldCoords;
			_game = game;
			_acceleration = 2 * implosionRadius / (float)Math.Pow(implosionTime / 1000, 2);

			for (int i = 0; i < _numberOfParticles; i++)
			{
				_particles.Add(GenerateParticle());
			}
		}

		public override bool Update(GameTime dt)
		{
			Particle part;

			for (int i = _particles.Count - 1; i >= 0; i--)
			{
				part = _particles[i];

				Vector2 accelTowards = Vector2.Normalize(part.Velocity) * _acceleration;
				part.Update(dt, accelTowards, _game.Options.UnitMultiplier);
				if (part.LifeTime >= _maxParticleLife)
				{
					_particles.RemoveAt(i);
				}
			}

			if (_particles.Count <= 0)
				return false;
			else
				return true;
		}

		public override void Draw(SpriteBatch batch)
		{
			batch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			foreach (var particle in _particles)
			{
				float alpha = 1;
				if (!_isWorldCoordinates)
				{
					batch.Draw(_baseTexture, GetParticleDisplayRectangle(particle.Position), new Color(_baseColor, alpha));
				}
				else
				{
					batch.Draw(_baseTexture, GetParticleDisplayRectangle(_game.Camera.GetViewportPosition(particle.Position)), new Color(_baseColor, alpha));
				}
			}
			batch.End();
		}

		protected override Particle GenerateParticle()
		{
			double randAngle = _rng.NextDouble(0, 2 * Math.PI);

			float x = (float)Math.Cos(randAngle) * _radius;
			float y = (float)Math.Sin(randAngle) * _radius;

			Vector2 velocity = Vector2.Normalize(new Vector2(-x, -y)) * 0.001f;
			return new Particle(_centralPosition + new Vector2(x, y), velocity, 0);
		}
	}
}

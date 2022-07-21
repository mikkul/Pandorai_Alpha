using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;

namespace Pandorai.ParticleSystems
{
	public class PSBloodSplat : ParticleSystem
	{
		private float _verticalSpeed;
		private float _horizontalScatter;
		private Vector2 _gravity;
		private Random _rng = new Random();

		public PSBloodSplat(Vector2 position, int noOfParticles, Texture2D particleTexture, float particleLifeMs, float verticalSpd, float horizontalRange, int partSize, Color color, float gravityValue, bool isWorldCoords)
		{
			_centralPosition = position;
			_numberOfParticles = noOfParticles;
			_baseTexture = particleTexture;
			_maxParticleLife = particleLifeMs;
			_verticalSpeed = verticalSpd;
			_horizontalScatter = horizontalRange;
			_particleSize = partSize;
			_baseColor = color;
			_gravity = new Vector2(0, gravityValue);
			_isWorldCoordinates = isWorldCoords;

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

				part.Update(dt, _gravity, Main.Game.Options.UnitMultiplier);
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

		protected override Particle GenerateParticle()
		{
			double range = 10;
			float velX = (float)_rng.NextDouble(-_horizontalScatter, _horizontalScatter);
			velX += (float)Math.Max(Math.Sign(velX) * range, velX);
			float velY = -(float)_rng.NextDouble(_verticalSpeed * 0.5, _verticalSpeed);
			Vector2 velocity = new Vector2(velX, velY);
			float randomLife = _rng.Next(0, 50);
			return new Particle(_centralPosition, velocity, randomLife);
		}
	}
}

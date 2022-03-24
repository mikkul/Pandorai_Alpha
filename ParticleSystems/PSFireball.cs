using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;

namespace Pandorai.ParticleSystems
{
	public class PSFireball : ParticleSystem
	{
		private Vector2 _velocity;
		private bool _isCentralParticle;
		private float _radius;
		private Random _rng = new Random();

		private PSFireball _centralParticle;

		public PSFireball(Vector2 position, int noOfParticles, Texture2D particleTexture, float particleLifeMs, Vector2 particleVelocity, int partSize, float fireballRadius, Color color, bool isWorldCoords, Main game, bool isCenter = false)
		{
			_centralPosition = position;
			_numberOfParticles = noOfParticles;
			_baseTexture = particleTexture;
			_maxParticleLife = particleLifeMs * 1.2f;
			_velocity = particleVelocity * 1000 / particleLifeMs;
			_particleSize = partSize;
			_radius = fireballRadius;
			_baseColor = color;
			_isWorldCoordinates = isWorldCoords;
			_game = game;
			_isCentralParticle = isCenter;

			// add one big particle in the middle
			if(!isCenter)
			{
				_centralParticle = new PSFireball(position, 1, particleTexture, particleLifeMs, particleVelocity, (int)(_particleSize * 3), 0, color, isWorldCoords, _game, true);
				ParticleSystemManager.AddSystem(_centralParticle, true);
			}

			for (int i = 0; i < _numberOfParticles; i++)
			{
				_particles.Add(GenerateParticle());
			}
		}

		public void Disintegrate()
		{
			for (int i = _particles.Count - 1; i >= 0; i--)
			{
				_particles[i].LifeTime = _maxParticleLife - 100;
			}
			if(!_isCentralParticle)
			{
				_centralParticle.Disintegrate();
			}
		}

		public override bool Update(GameTime dt)
		{
			Particle part;

			for (int i = _particles.Count - 1; i >= 0; i--)
			{
				part = _particles[i];

				part.Update(dt, Vector2.Zero, _game.Options.UnitMultiplier);
				if (part.LifeTime >= _maxParticleLife * 0.8f)
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
			double angle = _rng.NextDouble(0, 2 * Math.PI);
			float x = (float)Math.Cos(angle) * _radius;
			float y = (float)Math.Sin(angle) * _radius;
			Vector2 position = _isCentralParticle ? _centralPosition : new Vector2(_centralPosition.X + x, _centralPosition.Y + y);
			return new Particle(position, _velocity, 0);
		}
	}
}

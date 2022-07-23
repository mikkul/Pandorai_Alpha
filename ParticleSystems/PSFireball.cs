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

		public PSFireball(Vector2 position, int noOfParticles, string particleTextureName, float particleLifeMs, Vector2 particleVelocity, int partSize, float fireballRadius, Color color, bool isWorldCoords, bool isCenter = false)
		{
			CentralPosition = position;
			NumberOfParticles = noOfParticles;
			BaseTextureName = particleTextureName;
			MaxParticleLife = particleLifeMs * 1.2f;
			_velocity = particleVelocity * 1000 / particleLifeMs;
			ParticleSize = partSize;
			_radius = fireballRadius;
			BaseColor = color;
			IsWorldCoordinates = isWorldCoords;
			_isCentralParticle = isCenter;

			// add one big particle in the middle
			if(!isCenter)
			{
				_centralParticle = new PSFireball(position, 1, particleTextureName, particleLifeMs, particleVelocity, (int)(ParticleSize * 3), 0, color, isWorldCoords, true);
				ParticleSystemManager.AddSystem(_centralParticle, true);
			}

			for (int i = 0; i < NumberOfParticles; i++)
			{
				_particles.Add(GenerateParticle());
			}
		}

		public void Disintegrate()
		{
			for (int i = _particles.Count - 1; i >= 0; i--)
			{
				_particles[i].LifeTime = MaxParticleLife - 100;
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

				part.Update(dt, Vector2.Zero, Main.Game.Options.UnitMultiplier);
				if (part.LifeTime >= MaxParticleLife * 0.8f)
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
			Vector2 position = _isCentralParticle ? CentralPosition : new Vector2(CentralPosition.X + x, CentralPosition.Y + y);
			return new Particle(position, _velocity, 0);
		}
	}
}

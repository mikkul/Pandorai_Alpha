using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;

namespace Pandorai.ParticleSystems
{
	public class PSExplosion : ParticleSystem
	{
		private const float FrictionValue = 1f;

		private float _particleSpeed;
		private Random _rng = new Random();

		public PSExplosion(Vector2 position, int noOfParticles, string particleTextureName, float particleLifeMs, float particleSpd, int partSize, Color color, bool isWorldCoords)
		{
			CentralPosition = position;
			NumberOfParticles = noOfParticles;
			BaseTextureName = particleTextureName;
			MaxParticleLife = particleLifeMs;
			_particleSpeed = particleSpd;
			ParticleSize = partSize;
			BaseColor = color;
			IsWorldCoordinates = isWorldCoords;

			for (int i = 0; i < NumberOfParticles; i++)
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

				Vector2 friction = -part.Velocity * FrictionValue;
				part.Update(dt, friction, Main.Game.Options.UnitMultiplier);
				if (part.LifeTime >= MaxParticleLife)
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
			float randomnessRange = _particleSpeed * 0.2f;
			double randAngle = _rng.NextDouble(0, 2 * Math.PI);
			float velX = (float)Math.Cos(randAngle) * _particleSpeed + _rng.NextFloat(0, randomnessRange);
			float velY = (float)Math.Sin(randAngle) * _particleSpeed + _rng.NextFloat(0, randomnessRange);
			Vector2 velocity = new Vector2(velX, velY);
			float randomLife = _rng.Next(0, 50);
			return new Particle(CentralPosition, velocity, randomLife);
		}
	}
}

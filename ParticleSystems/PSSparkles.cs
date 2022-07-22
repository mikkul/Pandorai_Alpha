using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;

namespace Pandorai.ParticleSystems
{
	public class PSSparkles : ParticleSystem
	{
		private float _particleSpeed;
		private int _regenSpan;
		private Random _rng = new Random();

		private float _regenCounter = 0;

		public PSSparkles(Vector2 position, int noOfParticles, Texture2D particleTexture, float particleLifeMs, float particleSpd, int partSize, int regenSpanMs, Color color, bool isWorldCoords)
		{
			CentralPosition = position;
			NumberOfParticles = noOfParticles;
			BaseTexture = particleTexture;
			MaxParticleLife = particleLifeMs;
			_particleSpeed = particleSpd;
			ParticleSize = partSize;
			_regenSpan = regenSpanMs;
			BaseColor = color;
			IsWorldCoordinates = isWorldCoords;
		}

		protected override Particle GenerateParticle()
		{
			double range = 20;
			float velX = (float)_rng.NextDouble(-_particleSpeed, _particleSpeed);
			velX += (float)Math.Max(Math.Sign(velX) * range, velX);
			float velY = (float)_rng.NextDouble(-_particleSpeed, _particleSpeed);
			velY += (float)Math.Max(Math.Sign(velY) * range, velY);
			Vector2 velocity = new Vector2(velX, velY);
			float randomLife = _rng.Next(0, 50);
			return new Particle(CentralPosition, velocity, randomLife);
		}

		public override bool Update(GameTime dt)
		{
			Particle part;

			for (int i = _particles.Count - 1; i >= 0; i--)
			{
				part = _particles[i];

				part.Update(dt, Vector2.Zero, Main.Game.Options.UnitMultiplier);
				if(part.LifeTime >= MaxParticleLife)
				{
					_particles.RemoveAt(i);
				}
			}

			if(_particles.Count < NumberOfParticles)
			{
				int particlesToAdd = NumberOfParticles - _particles.Count;
				float timePerParticle = _regenSpan / particlesToAdd;
				_regenCounter += (float)dt.ElapsedGameTime.TotalMilliseconds;
				if(_regenCounter >= timePerParticle)
				{
					_regenCounter -= (float)Math.Floor(_regenCounter);
					_particles.Add(GenerateParticle());
				}
			}

			return true;
		}
	}
}

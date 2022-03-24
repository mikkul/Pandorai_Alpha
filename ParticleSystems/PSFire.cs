﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System;

namespace Pandorai.ParticleSystems
{
	public class PSFire : ParticleSystem
	{
		private float _particleSpeed;
		private int _regenSpan;
		private float _displacementOffsetX;
		private Random _rng = new Random();

		private float _regenCounter = 0;

		public PSFire(Vector2 position, int noOfParticles, Texture2D particleTexture, float particleLifeMs, float particleSpd, int partSize, int regenSpanMs, float fireWidth, Color color, bool isWorldCoords, Main game)
		{
			_centralPosition = position;
			_numberOfParticles = noOfParticles;
			_baseTexture = particleTexture;
			_maxParticleLife = particleLifeMs;
			_particleSpeed = particleSpd;
			_particleSize = partSize;
			_regenSpan = regenSpanMs;
			_baseColor = color;
			_isWorldCoordinates = isWorldCoords;
			_displacementOffsetX = fireWidth;
			_game = game;
		}

		protected override Particle GenerateParticle()
		{
			var velX = _rng.NextFloat(-_displacementOffsetX * 0.2f, _displacementOffsetX * 0.2f);
			var velY = _rng.NextFloat(-_particleSpeed * 0.1f, -_particleSpeed);
			Vector2 velocity = new Vector2(velX, velY);
			float randomLife = _rng.Next(0, 50);
			Vector2 pos = new Vector2(_centralPosition.X + _rng.NextFloat(-_displacementOffsetX, _displacementOffsetX), _centralPosition.Y);
			return new Particle(pos, velocity, randomLife);
		}

		public override bool Update(GameTime dt)
		{
			Particle part;

			for (int i = _particles.Count - 1; i >= 0; i--)
			{
				part = _particles[i];

				part.Update(dt, Vector2.Zero, _game.Options.UnitMultiplier);
				if (part.LifeTime >= _maxParticleLife)
				{
					_particles.RemoveAt(i);
				}
			}

			if (_particles.Count < _numberOfParticles)
			{
				int particlesToAdd = _numberOfParticles - _particles.Count;
				float timePerParticle = _regenSpan / particlesToAdd;
				_regenCounter += (float)dt.ElapsedGameTime.TotalMilliseconds;
				if (_regenCounter >= timePerParticle)
				{
					_regenCounter -= (float)Math.Floor(_regenCounter);
					_particles.Add(GenerateParticle());
				}
			}

			return true;
		}
	}
}

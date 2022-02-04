using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Pandorai.ParticleSystems
{
	public class Particle
	{
		private Vector2 position;
		private Vector2 velocity;
		private float lifeTime;

		public Particle(Vector2 pos, Vector2 vel, float life)
		{
			position = pos;
			velocity = vel;
			lifeTime = life;
		}

		public void Update(GameTime dt, Vector2 force, float unitMultiplier)
		{
			velocity += force * (float)dt.ElapsedGameTime.TotalSeconds;
			position += velocity * unitMultiplier * (float)dt.ElapsedGameTime.TotalSeconds;
			lifeTime += (float)dt.ElapsedGameTime.TotalMilliseconds;
		}

		public Vector2 Position { get => position; }
		public Vector2 Velocity { get => velocity; }
		public float LifeTime { get => lifeTime; set => lifeTime = value; }
	}
}

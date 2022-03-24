using Microsoft.Xna.Framework;

namespace Pandorai.ParticleSystems
{
	public class Particle
	{
		private Vector2 _position;
		private Vector2 _velocity;
		private float _lifeTime;

		public Particle(Vector2 pos, Vector2 vel, float life)
		{
			_position = pos;
			_velocity = vel;
			_lifeTime = life;
		}

		public Vector2 Position { get => _position; }
		public Vector2 Velocity { get => _velocity; }
		public float LifeTime { get => _lifeTime; set => _lifeTime = value; }

		public void Update(GameTime dt, Vector2 force, float unitMultiplier)
		{
			_velocity += force * (float)dt.ElapsedGameTime.TotalSeconds;
			_position += _velocity * unitMultiplier * (float)dt.ElapsedGameTime.TotalSeconds;
			_lifeTime += (float)dt.ElapsedGameTime.TotalMilliseconds;
		}
	}
}

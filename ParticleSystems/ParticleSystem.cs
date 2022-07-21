using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System.Collections.Generic;

namespace Pandorai.ParticleSystems
{
	public abstract class ParticleSystem
	{
		protected List<Particle> _particles = new List<Particle>();
		protected Vector2 _centralPosition;
		protected Texture2D _baseTexture;
		protected Color _baseColor;
		protected float _maxParticleLife;
		protected int _numberOfParticles;
		protected float _particleSize;
		protected bool _isWorldCoordinates;
		protected float _maxRange = 100;

		protected abstract Particle GenerateParticle();

		public abstract bool Update(GameTime dt);

		public virtual void Draw(SpriteBatch batch)
		{
			batch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			foreach (var particle in _particles)
			{
				float alpha = 1 - particle.LifeTime / _maxParticleLife;
				if(!_isWorldCoordinates)
				{
					batch.Draw(_baseTexture, GetParticleDisplayRectangle(particle.Position), new Color(_baseColor, alpha));
				}
				else
				{
					batch.Draw(_baseTexture, GetParticleDisplayRectangle(Main.Game.Camera.GetViewportPosition(particle.Position)), new Color(_baseColor, alpha));
				}
			}
			batch.End();
		}

		protected Rectangle GetParticleDisplayRectangle(Vector2 position)
		{
			var trueSize = _particleSize * Main.Game.Map.TileSize / Options.DefaultUnitSize;
			return new Rectangle(position.ToPoint() - new Point((int)(trueSize / 2)), new Point((int)trueSize));
		}

		public bool IsInViewport()
		{
			var viewportPos = Main.Game.Camera.GetViewportPosition(CentralPosition);
			return (_isWorldCoordinates && Main.Game.Camera.Viewport.Enlarge((int)_maxRange, (int)_maxRange).Contains(viewportPos))
				|| (!_isWorldCoordinates && Main.Game.Camera.Viewport.Enlarge((int)_maxRange, (int)_maxRange).Contains(_centralPosition));
		}

		public Vector2 CentralPosition {
			get => _centralPosition;
			set => _centralPosition = value;
		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Utility;
using System.Collections.Generic;

namespace Pandorai.ParticleSystems
{
	public abstract class ParticleSystem
	{
		protected List<Particle> particles = new List<Particle>();
		protected Vector2 centralPosition;
		protected Texture2D baseTexture;
		protected Color baseColor;
		protected float maxParticleLife;
		protected int numberOfParticles;
		protected float particleSize;
		protected bool isWorldCoordinates;
		protected Game1 game;
		protected float maxRange = 100;

		protected abstract Particle GenerateParticle();

		public abstract bool Update(GameTime dt);

		public virtual void Draw(SpriteBatch batch)
		{
			batch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			foreach (var particle in particles)
			{
				float alpha = 1 - particle.LifeTime / maxParticleLife;
				if(!isWorldCoordinates)
				{
					batch.Draw(baseTexture, GetParticleDisplayRectangle(particle.Position), new Color(baseColor, alpha));
				}
				else
				{
					batch.Draw(baseTexture, GetParticleDisplayRectangle(game.Camera.GetViewportPosition(particle.Position)), new Color(baseColor, alpha));
				}
			}
			batch.End();
		}

		protected Rectangle GetParticleDisplayRectangle(Vector2 position)
		{
			var trueSize = particleSize * game.Map.TileSize / Options.DefaultUnitSize;
			return new Rectangle(position.ToPoint() - new Point((int)(trueSize / 2)), new Point((int)trueSize));
		}

		public bool IsInViewport()
		{
			var viewportPos = game.Camera.GetViewportPosition(CentralPosition);
			return (isWorldCoordinates && game.Camera.Viewport.Enlarge((int)maxRange, (int)maxRange).Contains(viewportPos))
				|| (!isWorldCoordinates && game.Camera.Viewport.Enlarge((int)maxRange, (int)maxRange).Contains(centralPosition));
		}

		public Vector2 CentralPosition {
			get => centralPosition;
			set => centralPosition = value;
		}
	}
}

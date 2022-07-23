using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Pandorai.Persistency.Converters;
using Pandorai.Utility;
using System.Collections.Generic;

namespace Pandorai.ParticleSystems
{
    [JsonConverter(typeof(ParticleSystemConverter))]
    public abstract class ParticleSystem
    {
        public string TypeName => GetType().Name;

        public Vector2 CentralPosition;
        [JsonIgnore]
        public Texture2D BaseTexture => TypeLegends.Textures[BaseTextureName];
        public string BaseTextureName { get; set; }
        public Color BaseColor;
        public float MaxParticleLife;
        public int NumberOfParticles;
        public float ParticleSize;
        public bool IsWorldCoordinates;
        public float MaxRange = 100;

        protected List<Particle> _particles = new List<Particle>();

        protected abstract Particle GenerateParticle();

        public abstract bool Update(GameTime dt);

        public virtual void Draw(SpriteBatch batch)
        {
            batch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            foreach (var particle in _particles)
            {
                float alpha = 1 - particle.LifeTime / MaxParticleLife;
                if (!IsWorldCoordinates)
                {
                    batch.Draw(BaseTexture, GetParticleDisplayRectangle(particle.Position), new Color(BaseColor, alpha));
                }
                else
                {
                    batch.Draw(BaseTexture, GetParticleDisplayRectangle(Main.Game.Camera.GetViewportPosition(particle.Position)), new Color(BaseColor, alpha));
                }
            }
            batch.End();
        }

        protected Rectangle GetParticleDisplayRectangle(Vector2 position)
        {
            var trueSize = ParticleSize * Main.Game.Map.TileSize / Options.DefaultUnitSize;
            return new Rectangle(position.ToPoint() - new Point((int)(trueSize / 2)), new Point((int)trueSize));
        }

        public bool IsInViewport()
        {
            var viewportPos = Main.Game.Camera.GetViewportPosition(CentralPosition);
            return (IsWorldCoordinates && Main.Game.Camera.Viewport.Enlarge((int)MaxRange, (int)MaxRange).Contains(viewportPos))
                || (!IsWorldCoordinates && Main.Game.Camera.Viewport.Enlarge((int)MaxRange, (int)MaxRange).Contains(CentralPosition));
        }
    }
}

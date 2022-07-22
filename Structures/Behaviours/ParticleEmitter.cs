using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.ParticleSystems;

namespace Pandorai.Structures.Behaviours
{
    public class ParticleEmitter : Behaviour
    {
        public Color Color = Color.White;
        public string Type = "Sparkles";
        public int NumberOfParticles = 50;
        public int ParticleLife = 1500;
        public int ParticleSpeed = 35;
        public int ParticleSize = 5;
        public int ParticleRegenTime = 1000;
        public int FireWidth = 50;
        public int OffsetX = 0;
        public int OffsetY = 0;

        public ParticleSystem ParticleSystem;

        public override void Bind()
        {
            var position = Structure.Tile.Index.ToVector2() * Main.Game.Map.TileSize + new Vector2(OffsetX, OffsetY);

            switch (Type)
            {
                case "Sparkles":
                    ParticleSystem = new PSSparkles(position, NumberOfParticles, Main.Game.squareTexture, ParticleLife, ParticleSpeed, ParticleSize, ParticleRegenTime, Color, true);
                    break;
                case "Fire":
                    ParticleSystem = new PSFire(position, NumberOfParticles, Main.Game.squareTexture, ParticleLife, ParticleSpeed, ParticleSize, ParticleRegenTime, FireWidth, Color, true);
                    break;
                default:
                    break;
            }
            
            ParticleSystemManager.AddSystem(ParticleSystem, false);
        }

        public override void Unbind()
        {
        }

        public override void SetAttribute(string name, string value)
        {
            if (name == "Type")
            {
                Type = value;
            }
            else if (name == "Color")
            {
                Color = Utility.Helper.GetColorFromHex(value);
            }
            else if (name == "NumberOfParticles")
            {
                NumberOfParticles = int.Parse(value);
            }
            else if (name == "ParticleLife")
            {
                ParticleLife = int.Parse(value);
            }
            else if (name == "ParticleSpeed")
            {
                ParticleSpeed = int.Parse(value);
            }
            else if (name == "ParticleSize")
            {
                ParticleSize = int.Parse(value);
            }
            else if (name == "ParticleRegenTime")
            {
                ParticleRegenTime = int.Parse(value);
            }
            else if (name == "FireWidth")
            {
                FireWidth = int.Parse(value);
            }
            else if (name == "OffsetX")
            {
                OffsetX = int.Parse(value);
            }
            else if (name == "OffsetY")
            {
                OffsetY = int.Parse(value);
            }
        }

        public override Behaviour Clone()
        {
            return new ParticleEmitter()
            {
                Type = Type,
                Color = Color,
                NumberOfParticles = NumberOfParticles,
                ParticleLife = ParticleLife,
                ParticleSpeed = ParticleSpeed,
                ParticleSize = ParticleSize,
                ParticleRegenTime = ParticleRegenTime,
                FireWidth = FireWidth,
            };
        }

        public override void ForceHandler(ForceType force)
        {
        }

        public override void Interact(Creature creature)
        {
        }
    }
}

using System;
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Rendering;
using Pandorai.Sounds;
using Pandorai.Tilemaps;
using Pandorai.Utility;

namespace Pandorai.Effects
{
    public class HandTorch : Effect
    {
        public int Radius { get; set; }
        public float Intensity { get; set; }
        public Color Color { get; set; }

        public bool Activated { get; set; }

        private LightSource _lightSource;
        private Creature _owner;

        public override void Use(Creature user)
        {
            Activated ^= true;

            if(Activated)
            {
                _lightSource = LightingManager.AddLightSource(user.Position, Radius, Color, Intensity);
                user.TurnEnded += UpdateLightPosition;
                _owner = user;
            }
            else
            {
                LightingManager.RemoveLightSource(_lightSource);
                user.TurnEnded -= UpdateLightPosition;
                _owner = null;
            }
        }

        private void UpdateLightPosition()
        {
            _lightSource.Position = _owner.Position;
        }

        public override void SetAttribute(string name, string value)
        {
            if(name == "Radius")
            {
                Radius = int.Parse(value);
            }
            else if(name == "Intensity")
            {
                Intensity = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if(name == "Color")
            {
                Color = Helper.GetColorFromHex(value);
            }
        }

        protected override string GetMessage()
        {
            if(Activated)
            {
                return "You have lit up the torch";
            }
            else
            {
                return "You have put out the torch";
            }
        }
    }
}
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Rendering;
using Pandorai.Tilemaps;
using System;

namespace Pandorai.Structures.Behaviours
{
	public class LightEmitter : Behaviour
	{
		public Color Color = Color.White;
		public int Radius;
		public float Intesity = 1.0f;

		private LightSource lightSource;

		public override void Bind()
		{
			lightSource = LightingManager.AddLightSource(Structure.Tile.Index.ToVector2() * Game1.game.Map.TileSize, Radius, Color, Intesity);
		}

		public override void Unbind()
		{
			LightingManager.RemoveLightSource(lightSource);
		}

		public override void SetAttribute(string name, string value)
		{
			if(name == "Color")
			{
				Color = Utility.Helper.GetColorFromHex(value);
			}
			else if(name == "Radius")
			{
				Radius = int.Parse(value);
			}
			else if(name == "Intensity")
			{
				Intesity = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
			}
		}

		public override Behaviour Clone()
		{
			return new LightEmitter()
			{
				Color = Color,
				Radius = Radius,
				Intesity = Intesity,
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

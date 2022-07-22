using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Rendering;

namespace Pandorai.Structures.Behaviours
{
	public class LightEmitter : Behaviour
	{
		public Color Color = Color.White;
		public int Radius;
		public float Intesity = 1.0f;

		public LightSource LightSource;

		public override void Bind()
		{
			LightSource = LightingManager.AddLightSource(Structure.Tile.Index.ToVector2() * Main.Game.Map.TileSize, Radius, Color, Intesity);
		}

		public override void Unbind()
		{
			LightingManager.RemoveLightSource(LightSource);
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

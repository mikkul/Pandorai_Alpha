using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pandorai.MapGeneration.CustomRegions
{
	public class CustomRegion
	{
		public string Name;
		public string SampleName;
		public Color FloorColor = Color.White;
		public Color BorderColor = Color.White;
		public string MusicThemeName;
		public List<LocationSpecifier> LocationSpecifiers = new List<LocationSpecifier>();
		public List<DimensionSpecifier> DimensionSpecifiers = new List<DimensionSpecifier>();
		public List<ElementNode> Content = new List<ElementNode>();
	}
}

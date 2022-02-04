using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pandorai.MapGeneration
{
	public class Hallway
	{
		public List<Point> Area;
		public Point Entrance1;
		public Point Entrance2;
		public Region ConnectedRegion;
	}
}

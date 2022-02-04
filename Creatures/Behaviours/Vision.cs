using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pandorai.Creatures.Behaviours
{
	public abstract class Vision : Behaviour
	{
		public List<Point> VisibleTiles { get; protected set; }
	}
}

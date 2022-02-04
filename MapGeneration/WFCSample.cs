using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pandorai.MapGeneration
{
	public class WFCSample
	{
		public int[,] Data;
		public Dictionary<Color, TileType> TileColorLegend = new Dictionary<Color, TileType>();
		public Dictionary<Color, float> ColorMultiplierLegend = new Dictionary<Color, float>();
		public Dictionary<TileType, Point> TilePositionLegend = new Dictionary<TileType, Point>();
		public bool IsPeriodic;
	}

	public enum TileType
	{
		Floor,
		Wall,
		Empty,
		Mud,
		Path,
	}
}

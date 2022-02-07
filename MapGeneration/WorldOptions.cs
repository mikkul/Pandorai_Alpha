using Pandorai.Creatures;
using Pandorai.Items;
using Pandorai.Structures;
using System;
using System.Collections.Generic;

namespace Pandorai.MapGeneration
{
	public static class WorldOptions
	{
		public const int Width = 140;
		public const int Height = 100;

		public static int RegionPathwayPasses = 3;
		public static int MinimalRoomSize = 40;

		public static List<Area> Areas;

		public static Dictionary<string, Type> TypeLegend = new Dictionary<string, Type>
		{
		};

		public static void LoadAreasFromFile(Game1 game)
		{
			Areas = new List<Area>();

			Areas.Add(new Area("sampleAreaFile.json"));
			Areas.Add(new Area("undergroundArea.json"));

			foreach (var area in Areas)
			{
				area.LoadArea(game);
			}
		}
	}
}

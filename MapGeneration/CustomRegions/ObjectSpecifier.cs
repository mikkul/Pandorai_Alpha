using Pandorai.Tilemaps;
using System.Collections.Generic;
using Pandorai.Utility;

namespace Pandorai.MapGeneration.CustomRegions
{
	public class ObjectSpecifier
	{
		public string Name;
	}

	public class EntitySpecifier : ObjectSpecifier
	{
		public List<LocationSpecifier> LocationSpecifiers = new List<LocationSpecifier>();
		public Range Number;
	}

	public class Modifier : ObjectSpecifier
	{
		public ElementNode Parent;
		public LocationSpecifier Spec;
	}

	public class TriggerSpecifier : ObjectSpecifier
	{
		public List<LocationSpecifier> LocationSpecifiers = new List<LocationSpecifier>();
		public CreatureIncomingHandler Handler;
	}
}

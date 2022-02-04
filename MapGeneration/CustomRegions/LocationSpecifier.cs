using System.Collections.Generic;

namespace Pandorai.MapGeneration.CustomRegions
{
	public abstract class LocationSpecifier
	{
		public LocationType Type;
	}

	public class ProximitySpec : LocationSpecifier
	{
		public string ProximityTo;
		public DistanceDescription Distance;
		public int Tolerance = 2;

		public ProximitySpec()
		{
			Type = LocationType.Proximity;
		}

		private static readonly Dictionary<string, DistanceDescription> distanceKeys = new Dictionary<string, DistanceDescription>
		{
			{ "very close", DistanceDescription.VeryClose },
			{ "close", DistanceDescription.Close },
			{ "equidistant", DistanceDescription.Equidistant },
			{ "far", DistanceDescription.Far },
			{ "very far", DistanceDescription.VeryFar },
			{ "farthest", DistanceDescription.FarthestAway },
		};

		public static Dictionary<string, DistanceDescription> DistanceKeys
		{
			get => distanceKeys;
		}
	}

	public class LayerSpec : LocationSpecifier
	{
		public int Thickness = 1;
		public LayerLocation Location;
		public int Offset = 0;
		
		public LayerSpec()
		{
			Type = LocationType.Layer;
		}

		private static readonly Dictionary<string, LayerLocation> locationKeys = new Dictionary<string, LayerLocation>
		{
			{ "border", LayerLocation.Border },
			{ "mid", LayerLocation.Mid },
			{ "center", LayerLocation.Center },
		};

		public static Dictionary<string, LayerLocation> LocationKeys
		{
			get => locationKeys;
		}
	}

	public class StripeSpec : LocationSpecifier
	{
		public int Thickness = 1;
		public StripeDirection Direction;
		public float Position = 0;

		public StripeSpec()
		{
			Type = LocationType.Layer;
		}

		private static readonly Dictionary<string, StripeDirection> directionKeys = new Dictionary<string, StripeDirection>
		{
			{ "across", StripeDirection.Across },
			{ "along", StripeDirection.Along },
		};

		public static Dictionary<string, StripeDirection> DirectionKeys
		{
			get => directionKeys;
		}
	}

	public class PartSpec : LocationSpecifier
	{

	}

	public enum LocationType
	{
		Proximity,
		Layer,
	}

	public enum LayerLocation
	{
		Border,
		Mid,
		Center,
	}

	public enum StripeLocation
	{
		Front,
		Mid,
		Back,
	}

	public enum DistanceDescription
	{
		VeryClose,
		Close,
		Equidistant,
		Far,
		VeryFar,
		FarthestAway,
	}
}

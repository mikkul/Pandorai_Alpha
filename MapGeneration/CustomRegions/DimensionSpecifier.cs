using Pandorai.Utility;
using System.Collections.Generic;
using Random = System.Random;

namespace Pandorai.MapGeneration.CustomRegions
{
	public abstract class DimensionSpecifier
	{
		public List<Placement> PossiblePlacements = new List<Placement>();
		public Range OffsetX = Range.Zero;
		public Range OffsetY = Range.Zero;
		public int Id;
		public DimensionSpecifier Parent = null;
		public List<DimensionSpecifier> Children = new List<DimensionSpecifier>();

		public abstract AreaDataType[,] GetAreaData(Random rng);
	}

	public class RectangularArea : DimensionSpecifier
	{
		public Range Width;
		public Range Height;
		public float RotationChance = 0.0f;
		public bool FillInterior = true;

		public override AreaDataType[,] GetAreaData(Random rng)
		{
			int realWidth = (int)Width.GetRandom(rng);
			int realHeight = (int)Height.GetRandom(rng);
			float chance = (float)rng.NextDouble();
			if(chance <= RotationChance)
			{
				Helper.Swap(ref realWidth, ref realHeight);
			}
			AreaDataType[,] data = new AreaDataType[realWidth, realHeight];
			data.Populate(AreaDataType.Free);

			if(FillInterior)
			{
				float offset = 0.2f;
				var preplacedArea = new AreaDataType[(int)(realWidth * (1 - offset * 2)), (int)(realHeight * (1 - offset * 2))];
				preplacedArea.Populate(AreaDataType.PreplacedFloor);
				preplacedArea.CopyTo(data, (int)(realWidth * offset), (int)(realHeight * offset));
			}

			return data;
		}
	}

	public enum Placement
	{
		Left,
		Right,
		Top,
		Bottom,
		Center,
	}

	public enum AreaDataType
	{
		Free,
		PreplacedFloor,
		PreplacedBorder,
		Locked,
	}
}

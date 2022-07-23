using System;
using Microsoft.Xna.Framework;
using Pandorai.Visibility;

namespace Pandorai.Creatures.Behaviours
{
	public class NormalVision : Vision
	{
		public int RangeLimit;

		private FieldOfView _fov;

		public NormalVision()
		{
			_fov = new FieldOfView(BlocksLight, SetVisibility, GetDistance);
		}

		protected override void CalculateFOV()
		{
			VisibleTiles.Clear();
			_fov.Compute(Owner.MapIndex, RangeLimit);
		}

		bool BlocksLight(int x, int y)
		{
			if (Main.Game.Map.GetTile(x, y) != null)
				return Main.Game.Map.GetTile(x, y).CollisionFlag;
			else return false;
		}

		void SetVisibility(int x, int y)
		{
			if (Main.Game.Map.GetTile(x, y) != null)
				VisibleTiles.Add(new Point(x, y));
		}

		int GetDistance(int x, int y)
		{
			return (int)Math.Sqrt(x * x + y * y);
		}

		public override Behaviour Clone()
		{
			return new NormalVision()
			{
				RangeLimit = RangeLimit,
			};
		}

		public override void SetAttribute(string name, string value)
		{
			switch (name)
			{
				case "RangeLimit":
					RangeLimit = int.Parse(value);
					break;
				default:
					break;
			}
		}
	}
}

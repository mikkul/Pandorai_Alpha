using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Pandorai.Visibility;

namespace Pandorai.Creatures.Behaviours
{
	public class NormalVision : Vision
	{
		private FieldOfView FOV;

		public int RangeLimit;

		public NormalVision()
		{
			FOV = new FieldOfView(BlocksLight, SetVisibility, GetDistance);
			VisibleTiles = new List<Point>();
		}

		public override void Bind()
		{
			Owner.TurnEnded += CalculateFOV;
		}

		void CalculateFOV()
		{
			VisibleTiles.Clear();
			FOV.Compute(Owner.MapIndex, RangeLimit);
		}

		bool BlocksLight(int x, int y)
		{
			if (Owner.game.Map.GetTile(x, y) != null)
				return Owner.game.Map.GetTile(x, y).CollisionFlag;
			else return false;
		}

		void SetVisibility(int x, int y)
		{
			if (Owner.game.Map.GetTile(x, y) != null)
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

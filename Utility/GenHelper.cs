using Microsoft.Xna.Framework;
using Pandorai.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandorai.Utility
{
	public delegate bool PointCheckHandler(Point point);

	public static class GenHelper
	{
		public static Point[] GetNeighbours(Point point)
		{
			Point[] neighbours = new Point[4]
			{
				new Point(point.X - 1, point.Y),
				new Point(point.X + 1, point.Y),
				new Point(point.X, point.Y - 1),
				new Point(point.X, point.Y + 1),
			};

			return neighbours;
		}

		public static List<Point> GetNeighboursRecursive(Point startPoint, int maxDepth)
		{
			List<Point> totalArea = GetNeighbours(startPoint).ToList();
			recurse(new List<Point>(totalArea), 1);

			void recurse(List<Point> points, int currentDepth)
			{
				if (currentDepth < maxDepth)
				{
					int nextDepth = ++currentDepth;
					foreach (var point in points)
					{
						var neighbours = GetNeighbours(point).ToList();
						totalArea.AddRange(neighbours);
						recurse(new List<Point>(points), nextDepth);
					}
				}
				else
				{
					totalArea.AddRange(points);
				}
			}
			totalArea.Remove(startPoint);
			totalArea = totalArea.Distinct().ToList();

			return totalArea;
		}

		public static Point[] Get8Neighbours(Point point)
		{
			Point[] neighbours = new Point[8]
			{
				new Point(point.X - 1, point.Y),
				new Point(point.X + 1, point.Y),
				new Point(point.X, point.Y - 1),
				new Point(point.X, point.Y + 1),
				new Point(point.X + 1, point.Y + 1),
				new Point(point.X - 1, point.Y + 1),
				new Point(point.X + 1, point.Y - 1),
				new Point(point.X - 1, point.Y - 1),
			};

			return neighbours;
		}

		public static List<Point> GetBresenhamLine(int x, int y, int x2, int y2)
		{
			List<Point> line = new List<Point>();
			int w = x2 - x;
			int h = y2 - y;
			int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
			if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
			if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
			if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
			int longest = Math.Abs(w);
			int shortest = Math.Abs(h);
			if (!(longest > shortest))
			{
				longest = Math.Abs(h);
				shortest = Math.Abs(w);
				if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
				dx2 = 0;
			}
			int numerator = longest >> 1;
			for (int i = 0; i <= longest; i++)
			{
				line.Add(new Point(x, y));
				numerator += shortest;
				if (!(numerator < longest))
				{
					numerator -= longest;
					x += dx1;
					y += dy1;
				}
				else
				{
					x += dx2;
					y += dy2;
				}
			}

			return line;
		}

		public static List<Point> GetBresenhamLine(Point pos1, Point pos2)
        {
			return GetBresenhamLine(pos1.X, pos1.Y, pos2.X, pos2.Y);
        }

		public static RaycastInfo Raycast(Point point1, Point point2, PointCheckHandler collisionHandler, PointCheckHandler savePointHandler)
		{
			var bresenhamLine = GetBresenhamLine(point1.X, point1.Y, point2.X, point2.Y);

			var pointsBetween = new List<Point>();

			foreach (var point in bresenhamLine)
			{
				if (savePointHandler(point))
				{
					pointsBetween.Add(point);
				}

				if (collisionHandler(point))
				{
					return new RaycastInfo { Hit = true, Position = point, PointsBetween = pointsBetween };
				}
			}

			return new RaycastInfo { Hit = false };
		}

		public static List<Point> GetCircleBorderPoints(int centerX, int centerY, int radius, PointCheckHandler plotCheck = null)
		{
			List<Point> plottedPoints = new List<Point>();

			int x = radius;
			int y = 0;
			int P = 1 - radius;

			// plot initial points
			plotPoint(new Point(x + centerX, y + centerY));
			if (radius > 0)
			{
				plotPoint(new Point(-x + centerX, y + centerY));
				plotPoint(new Point(y + centerX, x + centerY));
				plotPoint(new Point(y + centerX, -x + centerY));
			}

			while (x > 0)
			{
				y++;

				if (P <= 0)
				{
					P = P + 2 * y + 1;
				}
				else
				{
					x--;
					P = P + 2 * y - 2 * x + 1;
				}

				if (x < y)
					break;

				plotPoint(new Point(x + centerX, y + centerY));
				plotPoint(new Point(-x + centerX, y + centerY));
				plotPoint(new Point(x + centerX, -y + centerY));
				plotPoint(new Point(-x + centerX, -y + centerY));

				if (x != y)
				{
					plotPoint(new Point(y + centerX, x + centerY));
					plotPoint(new Point(-y + centerX, x + centerY));
					plotPoint(new Point(y + centerX, -x + centerY));
					plotPoint(new Point(-y + centerX, -x + centerY));
				}
			}

			void plotPoint(Point point)
			{
				if (plotCheck == null || plotCheck(point))
				{
					plottedPoints.Add(point);
				}
			}

			return plottedPoints;
		}

		public static List<Point> GetRingAreaPoints(int centerX, int centerY, int radius, int thiccness, ExpansionDirection expansionDirection, PointCheckHandler plotCheck = null)
		{
			List<Point> area = new List<Point>();

			for (int i = 0; i < thiccness; i++)
			{
				List<Point> border = null;
				int offset = 0;
				if (expansionDirection == ExpansionDirection.In)
				{
					offset = -i;
				}
				else if (expansionDirection == ExpansionDirection.Out)
				{
					offset = i;
				}
				else if (expansionDirection == ExpansionDirection.InOut)
				{
					offset = i % 2 == 0 ? (int)Math.Ceiling(i / 2f) : (int)-Math.Ceiling(i / 2f);
				}
				else if (expansionDirection == ExpansionDirection.OutIn)
				{
					offset = i % 2 == 0 ? (int)-Math.Ceiling(i / 2f) : (int)Math.Ceiling(i / 2f);
				}
				border = GetCircleBorderPoints(centerX, centerY, radius + offset, plotCheck);
				area.AddRange(border);
			}

			return area;
		}

		public static Point FloodfillSearchFirst<T>(T[,] map, Point startPoint, PointCheckHandler goalCondition, PointCheckHandler searchConstraint = null)
		{
			Queue<Point> openList = new Queue<Point>();
			List<Point> closedList = new List<Point>();

			openList.Enqueue(startPoint);

			while (openList.Count > 0)
			{
				var currentPoint = openList.Dequeue();

				if (closedList.Contains(currentPoint)) continue;

				var neighbours = GetNeighbours(currentPoint);
				foreach (var neighbour in neighbours)
				{
					if (!map.IsPointInBounds(neighbour)) continue;

					if (!closedList.Contains(neighbour) && ((searchConstraint != null && searchConstraint(neighbour)) || searchConstraint == null))
					{
						if (goalCondition(neighbour))
						{
							return neighbour;
						}

						openList.Enqueue(neighbour);
					}
				}
				closedList.Add(currentPoint);
			}

			return Helper.NullPoint;
		}

		public static List<Point> ExpandArea(List<Point> area, PointCheckHandler tileCondition, object[,] bounds = null)
		{
			List<Point> expandedLayer = new List<Point>();

			foreach (var point in area)
			{
				var neighbours = Get8Neighbours(point);

				foreach (var neighbour in neighbours)
				{
					if (bounds != null && !bounds.IsPointInBounds(neighbour)) continue;

					if (tileCondition(neighbour))
					{
						expandedLayer.Add(neighbour);
					}
				}
			}

			return expandedLayer.Distinct().ToList();
		}

		public static (Vector2 answer0, Vector2 answer1) GetCircleCenterFromPoints(Vector2 p1, Vector2 p2, float radius)
        {
			double q = Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));

			double y3 = (p1.Y + p2.Y) / 2;

			double x3 = (p1.X + p2.X) / 2;

			double basex = Math.Sqrt(Math.Pow(radius, 2) - Math.Pow((q / 2), 2)) * (p1.Y - p2.Y) / q;
			double basey = Math.Sqrt(Math.Pow(radius, 2) - Math.Pow((q / 2), 2)) * (p2.X - p1.X) / q;

			double centerx1 = x3 + basex; //center x of circle 1
			double centery1 = y3 + basey; //center y of circle 1
			double centerx2 = x3 - basex; //center x of circle 2
			double centery2 = y3 - basey; //center y of circle 2

			return (new Vector2((float)centerx1, (float)centery1), new Vector2((float)centerx2, (float)centery2));
		}

		public static float GetCircleRadiusFromPointsAndAngle(Vector2 p1, Vector2 p2, float theta)
        {
			if(theta == 180)
            {
				return (p1 - p2).Length() / 2;
			}
			double alpha = (180f - theta) / 2;
			double dist = (p1 - p2).Length();
			double radius = dist * Math.Sin(alpha) / Math.Sin(theta);
			return (float)radius;
        }

		/// <summary>
		/// Checks whether a point lies on the left side, right side or on the specified line
		/// </summary>
		/// <param name="p1">First point determining the line</param>
		/// <param name="p2">Second point determinig the line</param>
		/// <param name="point">Point to be checked</param>
		/// <returns>-1 if point is on the left of the line, 1 if on the right, 0 if right on the line</returns>
		public static int PointOnALine(Vector2 p1, Vector2 p2, Vector2 point)
        {
			return Math.Sign((p2.X - p1.X) * (point.Y - p1.Y) - (p2.Y - p1.Y) * (point.X - p1.X));
        }
	}

	public struct RaycastInfo
	{
		public bool Hit;
		public Point Position;
		public List<Point> PointsBetween;
	}

	public struct RangeF
	{
		public float Min;
		public float Max;

		private static RangeF zero = new RangeF(0, 0);
		public static RangeF Zero { get => zero; }

		public RangeF(float min, float max)
		{
			Min = min;
			Max = max;
		}

		public float GetRandom(Random rng)
		{
			return rng.NextFloat(Min, Max);
		}
	}

    public struct Range
    {
        public int Min;
        public int Max;
        public float Weight 
		{ 
			get => weight; 
			set
			{
				if(value < -1) weight = -1;
				else if(value > 1) weight = 1;
				else weight = value;
			} 
		}

        private static Range zero = new Range(0, 0);
        private float weight;
		private Dictionary<int, float> numberProbabilities;

        public static Range Zero { get => zero; }

        public Range(int min, int max) : this(min, max, 0f)
        {
        }

        public Range(int min, int max, float weight) : this()
        {
            Min = min;
            Max = max;
            Weight = weight;
			numberProbabilities = new Dictionary<int, float>();
			for (int i = Min; i <= Max; i++)
			{
				//numberProbabilities[i] = Max - i
			}
        }

        public int GetRandom(Random rng)
        {

            return rng.Next(Min, Max + 1);
        }
    }

    public enum ExpansionDirection
	{
		Out,
		In,
		InOut,
		OutIn,
	}
}
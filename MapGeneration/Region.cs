using Microsoft.Xna.Framework;
using Pandorai.Tilemaps;
using Pandorai.Utility;
using System.Collections.Generic;
using System;

namespace Pandorai.MapGeneration
{
	public enum PlacementType
	{
		Part,
		Exact,
	}

	public enum PlacementDirection
	{
		Horizontal,
		Vertical,
	}

	public enum StripeDirection
	{
		Along,
		Across,
	}

	public class Region
	{
		public string Name;
		public Color Color;
		public int Id;

		public List<Point> Area = new List<Point>();
		public List<Point> Border = new List<Point>();

		public Point MidPoint;
		public Rectangle Bounds;
		public int[,] OpArea;
		public List<List<Point>> InteriorLayers;

		public Region()
		{
		}

		public void ProcessInterior(Tile[,] map)
		{
			CalculateMidPoint();
			CalculateOuterBounds();
			CalculateLayers(map);
		}

		private void CalculateMidPoint()
		{
			Point sumOfRegionTileLocations = Point.Zero;
			foreach (var point in Area)
			{
				sumOfRegionTileLocations.X += point.X;
				sumOfRegionTileLocations.Y += point.Y;
			}
			MidPoint = new Point(sumOfRegionTileLocations.X / Area.Count, sumOfRegionTileLocations.Y / Area.Count);
		}

		private void CalculateOuterBounds()
		{
			int minX = int.MaxValue, minY = int.MaxValue;
			int maxX = int.MinValue, maxY = int.MinValue;

			foreach (var point in Border)
			{
				if (point.X < minX) minX = point.X;
				if (point.Y < minY) minY = point.Y;
				if (point.X > maxX) maxX = point.X;
				if (point.Y > maxY) maxY = point.Y;
			}

			Bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);

			OpArea = new int[Bounds.Width + 1, Bounds.Height + 1];
			foreach (var point in Border)
			{
				SetOp(point, -1);
			}
			foreach (var point in Area)
			{
				SetOp(point, -2);
			}
		}

		private void CalculateLayers(Tile[,] map)
		{
			InteriorLayers = new List<List<Point>>();

			List<Point> lastLayer = new List<Point>(Border);

			while(lastLayer.Count > 0)
			{
				List<Point> newLayer = new List<Point>();

				foreach (var point in lastLayer)
				{
					var neighbours = GenHelper.Get8Neighbours(point);

					foreach (var neighbour in neighbours)
					{
						if(GetOp(neighbour) == -2)
						{
							newLayer.Add(neighbour);
							SetOp(neighbour, InteriorLayers.Count + 1);
							if(map[neighbour.X, neighbour.Y].BaseColor == this.Color)
								map[neighbour.X, neighbour.Y].BaseColor = map[neighbour.X, neighbour.Y].BaseColor.Brighten(-0.1f * InteriorLayers.Count);
						}
					}
				}

				if(newLayer.Count > 0)
				{
					InteriorLayers.Add(newLayer);
					lastLayer = newLayer;
				}
				else
				{
					break;
				}
			}
		}

		public List<Point> GetCorners(PlacementType placement, int value)
		{
			List<Point> corners = new List<Point>();

			int layerIndex = placement == PlacementType.Exact ? value : (int)Math.Round((double)(InteriorLayers.Count * value)) - 1;

			DistanceMeasurer<Point> distanceMeasurer = (p1, p2) => p1.DistanceTo(p2);

			var boundsCorner1 = Bounds.Location;
			corners.Add(Helper.FindClosestPoint(boundsCorner1, InteriorLayers[layerIndex], distanceMeasurer));
			var boundsCorner2 = Bounds.Location + new Point(Bounds.Width, 0);
			corners.Add(Helper.FindClosestPoint(boundsCorner2, InteriorLayers[layerIndex], distanceMeasurer));
			var boundsCorner3 = Bounds.Location + new Point(0, Bounds.Height);
			corners.Add(Helper.FindClosestPoint(boundsCorner3, InteriorLayers[layerIndex], distanceMeasurer));
			var boundsCorner4 = Bounds.Location + Bounds.Size;
			corners.Add(Helper.FindClosestPoint(boundsCorner4, InteriorLayers[layerIndex], distanceMeasurer));

			return corners;
		}

		public List<Point> GetStripe(StripeDirection stripeDirection, float position, int offsetValue)
		{
			List<Point> stripe = new List<Point>();

			int distMidToCornersX = 0;
			int distMidToCornersY = 0;
			var outermostCorners = GetCorners(PlacementType.Exact, 0);
			foreach (var corner in outermostCorners)
			{
				distMidToCornersX += Math.Abs(MidPoint.X - corner.X);
				distMidToCornersY += Math.Abs(MidPoint.Y - corner.Y);
			}

			PlacementDirection longSide = distMidToCornersX > distMidToCornersY ? PlacementDirection.Horizontal : PlacementDirection.Vertical;

			int maxNegativeOffset = 0;
			int maxPositiveOffset = 0;

			// figure out max offsets, that is how far the region stretches in the opposite direction of stripeDirection
			if ((stripeDirection == StripeDirection.Along && longSide == PlacementDirection.Horizontal)
				|| (stripeDirection == StripeDirection.Across && longSide == PlacementDirection.Vertical)) // stripe goes in x axis
			{
				// going in negative direction first (top)
				Point nextPoint = MidPoint;
				maxNegativeOffset--; // compensate for the middle point
				while (GetOp(nextPoint) > 0)
				{
					maxNegativeOffset++;
					nextPoint.Y--;
				}
				// now positive direction (bottom)
				nextPoint = MidPoint;
				maxPositiveOffset--; // compensate for the middle point
				while (GetOp(nextPoint) > 0)
				{
					maxPositiveOffset++;
					nextPoint.Y++;
				}
			}
			else if ((stripeDirection == StripeDirection.Across && longSide == PlacementDirection.Horizontal)
				|| (stripeDirection == StripeDirection.Along && longSide == PlacementDirection.Vertical)) // stripe goes in y axis
			{
				// going in negative direction first (left)
				Point nextPoint = MidPoint;
				maxNegativeOffset--; // compensate for the middle point
				while (GetOp(nextPoint) > 0)
				{
					maxNegativeOffset++;
					nextPoint.X--;
				}
				// now positive direction (right)
				nextPoint = MidPoint;
				maxPositiveOffset--; // compensate for the middle point
				while (GetOp(nextPoint) > 0)
				{
					maxPositiveOffset++;
					nextPoint.X++;
				}
			}

			int offsetIndex = 0;
			if (offsetValue < 0) offsetIndex = (int)Math.Round(maxNegativeOffset * position) + offsetValue;
			else offsetIndex = (int)Math.Round(maxPositiveOffset * position) + offsetValue;

			// create the actual stripe
			if ((stripeDirection == StripeDirection.Along && longSide == PlacementDirection.Horizontal)
				|| (stripeDirection == StripeDirection.Across && longSide == PlacementDirection.Vertical)) // stripe goes in x axis
			{
				// going in negative direction first (left)
				Point nextPoint = MidPoint + new Point(0, offsetIndex);
				while (GetOp(nextPoint) > 0)
				{
					stripe.Add(nextPoint);
					nextPoint.X--;
				}
				// now positive direction (right)
				nextPoint = MidPoint + new Point(0, offsetIndex);
				while (GetOp(nextPoint) > 0)
				{
					stripe.Add(nextPoint);
					nextPoint.X++;
				}
				if(stripe.Count > 0)
					stripe.RemoveAt(0); // compensate for the double middle point
				stripe.Sort((p1, p2) => p1.X.CompareTo(p2.X)); // sort the list so that the first point is the left-most point
			}
			else if ((stripeDirection == StripeDirection.Across && longSide == PlacementDirection.Horizontal)
				|| (stripeDirection == StripeDirection.Along && longSide == PlacementDirection.Vertical)) // stripe goes in y axis
			{
				// going in negative direction first (top)
				Point nextPoint = MidPoint + new Point(offsetIndex, 0);
				while (GetOp(nextPoint) > 0)
				{
					stripe.Add(nextPoint);
					nextPoint.Y--;
				}
				// now positive direction (bottom)
				nextPoint = MidPoint + new Point(offsetIndex, 0);
				while (GetOp(nextPoint) > 0)
				{
					stripe.Add(nextPoint);
					nextPoint.Y++;
				}
				if(stripe.Count > 0)
					stripe.RemoveAt(0); // compensate for the double middle point
				stripe.Sort((p1, p2) => p1.Y.CompareTo(p2.Y)); // sort the list so that the first point is the top-most point
			}

			return stripe;
		}

		private int GetOp(Point point)
		{
			int x = point.X - Bounds.Left;
			int y = point.Y - Bounds.Top;
			if (OpArea.IsPointInBounds(new Point(x, y))) return OpArea[x, y];
			else return -99;
		}

		private void SetOp(Point point, int value)
		{
			int x = point.X - Bounds.Left;
			int y = point.Y - Bounds.Top;
			if (OpArea.IsPointInBounds(new Point(x, y))) OpArea[x, y] = value;
		}
	}
}

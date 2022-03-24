using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Diagnostics;

namespace Pandorai.Utility
{
	static class Extensions
	{
		public static T GetRandomElement<T>(this IEnumerable<T> list, Random random)
		{
			if (list.Count() == 0) return default(T);
			return list.ElementAt(random.Next(list.Count()));
		}

		public static float ManhattanDistanceTo(this Point thisPoint, Point point2)
		{
			return Math.Abs(thisPoint.X - point2.X) + Math.Abs(thisPoint.Y - point2.Y);
		}

		public static float DistanceToSquared(this Point thisPoint, Point point2)
		{
			return (thisPoint.X - point2.X) * (thisPoint.X - point2.X) + (thisPoint.Y - point2.Y) * (thisPoint.Y - point2.Y);
		}

		public static float DistanceTo(this Point thisPoint, Point point2)
        {
			return (float)Math.Sqrt(thisPoint.DistanceToSquared(point2));
        }

		public static Color Brighten(this Color color, float value)
		{
			int value255 = (int)Math.Round(value * 255);
			return new Color(color.R + value255, color.G + value255, color.B + value255);
		}

		public static Rectangle Enlarge(this Rectangle rectangle, int horizontalValue, int verticalValue, bool aroundCenter = true)
		{
			var newRectangle = rectangle;
			if (aroundCenter)
			{
				newRectangle.X -= horizontalValue;
				newRectangle.Y -= verticalValue;
				newRectangle.Width += horizontalValue * 2;
				newRectangle.Height += verticalValue * 2;
			}
			else
			{
				newRectangle.Width += horizontalValue;
				newRectangle.Height += verticalValue;
			}
			return newRectangle;
		}

		public static Texture2D ExtractSubtexture(this Texture2D texture, Rectangle sourceRect, GraphicsDevice graphicsDevice)
		{
			Texture2D subTexture = new Texture2D(graphicsDevice, sourceRect.Width, sourceRect.Height);
			Color[] data = new Color[sourceRect.Width * sourceRect.Height];
			try
			{
				texture.GetData(0, sourceRect, data, 0, data.Length);
			}
			catch (InvalidOperationException)
			{
				// idk, only happened once and im not sure why, just made sure it doesnt break anything
			}
			subTexture.SetData(data);
			return subTexture;
		}

		public static double NextDouble(this Random random, double min, double max)
		{
			return random.NextDouble() * (max - min) + min;
		}

		public static void Populate<T>(this T[] collection, T value)
		{
			for (int i = 0; i < collection.Length; i++)
			{
				collection[i] = value;
			}
		}

		public static void Populate<T>(this T[,] collection, T value)
		{
			for (int x = 0; x < collection.GetLength(0); x++)
			{
				for (int y = 0; y < collection.GetLength(1); y++)
				{
					collection[x, y] = value;
				}
			}
		}

		public static void Populate<T>(this T[,] collection, Func<T> replaceFunc)
		{
			for (int x = 0; x < collection.GetLength(0); x++)
			{
				for (int y = 0; y < collection.GetLength(1); y++)
				{
					collection[x, y] = replaceFunc();
				}
			}
		}

		public static List<Point> CreateBorder<T>(this T[,] collection, T borderValue, int thickness = 1)
		{
			List<Point> borderPoints = new List<Point>();
			for (int x = 0; x < collection.GetLength(0); x++)
			{
				for (int y = 0; y < collection.GetLength(1); y++)
				{
					if (x < thickness || x >= collection.GetLength(0) - thickness
						|| y < thickness || y >= collection.GetLength(1) - thickness)
					{
						collection[x, y] = borderValue;
						borderPoints.Add(new Point(x, y));
					}
				}
			}
			return borderPoints;
		}

		public static List<Point> CreateBorder<T>(this T[,] collection, Func<T> borderValue, int thickness = 1)
		{
			List<Point> borderPoints = new List<Point>();
			for (int x = 0; x < collection.GetLength(0); x++)
			{
				for (int y = 0; y < collection.GetLength(1); y++)
				{
					if (x < thickness || x >= collection.GetLength(0) - thickness
						|| y < thickness || y >= collection.GetLength(1) - thickness)
					{
						collection[x, y] = borderValue();
						borderPoints.Add(new Point(x, y));
					}
				}
			}
			return borderPoints;
		}

		public static void CopyTo<T>(this T[,] from, T[,] to, int offsetX = 0, int offsetY = 0)
		{
			for (int x = 0; x < from.GetLength(0); x++)
			{
				for (int y = 0; y < from.GetLength(1); y++)
				{
					var finalX = x + offsetX;
					var finalY = y + offsetY;
					if(finalX >= 0 && finalX < to.GetLength(0) && finalY >= 0 && finalY < to.GetLength(1))
					{
						to[finalX, finalY] = from[x, y];
					}
				}
			}
		}

		public static void CopyTo<T>(this T[,] from, T[,] to, Func<T, bool> valueCheck, int offsetX = 0, int offsetY = 0)
		{
			for (int x = 0; x < from.GetLength(0); x++)
			{
				for (int y = 0; y < from.GetLength(1); y++)
				{
					var finalX = x + offsetX;
					var finalY = y + offsetY;
					if (finalX >= 0 && finalX < to.GetLength(0) && finalY >= 0 && finalY < to.GetLength(1) && valueCheck(from[x, y]))
					{
						to[finalX, finalY] = from[x, y];
					}
				}
			}
		}

		public static Point GetIndexOf<T>(this T[,] collection, T element)
		{
			for (int x = 0; x < collection.GetLength(0); x++)
			{
				for (int y = 0; y < collection.GetLength(1); y++)
				{
					if (collection[x, y].Equals(element)) return new Point(x, y);
				}
			}
			return new Point(-1, -1);
		}

		public static List<T> Shuffle<T>(this List<T> list, Random rng)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
			return list;
		}

		public static bool IsPointInBounds<T>(this T[,] collection, Point point)
		{
			return point.X >= 0 && point.X < collection.GetLength(0) && point.Y >= 0 && point.Y < collection.GetLength(1); 
		}

		public static bool IsPointInBounds<T>(this T[,] collection, int x, int y)
		{
			return x >= 0 && x < collection.GetLength(0) && y >= 0 && y < collection.GetLength(1);
		}

		public static Rectangle Displace(this Rectangle rect, Point displacement)
		{
			return new Rectangle(rect.Location + displacement, rect.Size);
		}

		public static Rectangle Displace(this Rectangle rect, Vector2 displacement)
		{
			return new Rectangle(rect.Location + displacement.ToPoint(), rect.Size);
		}

		public static Rectangle Displace(this Rectangle rect, int displacementX, int displacementY)
		{
			return new Rectangle(rect.X + displacementX, rect.Y + displacementY, rect.Width, rect.Height);
		}

		public static Rectangle Displace(this Rectangle rect, float displacementX, float displacementY)
		{
			return new Rectangle(rect.X + (int)displacementX, rect.Y + (int)displacementY, rect.Width, rect.Height);
		}

		public static float NextFloat(this Random random)
		{
			return (float)random.NextDouble();
		}

		public static float NextFloat(this Random random, float min, float max)
		{
			return random.NextFloat() * (max - min) + min;
		}

		public static Color ToXnaColor(this System.Drawing.Color original)
		{
			return new Color(original.R, original.G, original.B, original.A);
		}

		public static Color ModifyMultiply(this Color color, float r = 1, float g = 1, float b = 1, float a = 1)
		{
			var vecColor = color.ToVector4();
			return new Color(vecColor.X * r, vecColor.Y * g, vecColor.Z * b, vecColor.W * a);
		}

		public static T GetValue<T>(this T[,] array, Point index)
		{
			if (!array.IsPointInBounds(index))
			{
				Debug.WriteLine("Point was out of bounds");
				return default(T);
			}

			return array[index.X, index.Y];
		}

		public static void SetValue<T>(this T[,] array, Point index, T value)
		{
			if (!array.IsPointInBounds(index))
			{
				Debug.WriteLine("Point was out of bounds");
				return;
			}

			array[index.X, index.Y] = value;
		}

		public static void SetValue<T>(this T[,] array, Point index, Func<T> value)
		{
			if (!array.IsPointInBounds(index))
			{
				Debug.WriteLine("Point was out of bounds");
				return;
			}

			array[index.X, index.Y] = value();
		}

		public static int GetArea(this Rectangle rect)
		{
			return rect.Width * rect.Height;
		}

		public static T[,] GetValues<T>(this T[,] arr, Rectangle area)
		{
			T[,] subset = new T[area.Width, area.Height];
			for (int x = 0; x < area.Width; x++)
			{
				for (int y = 0; y < area.Height; y++)
				{
					subset[x, y] = arr[x + area.X, y + area.Y];
				}
			}
			return subset;
		}

		public static void SetValues<T>(this T[,] arr, Rectangle area, T value)
		{
			for (int x = 0; x < area.Width; x++)
			{
				for (int y = 0; y < area.Height; y++)
				{
					arr[x + area.X, y + area.Y] = value;
				}
			}
		}

		public static void SetValues<T>(this T[,] arr, Rectangle area, Func<T> value)
		{
			for (int x = 0; x < area.Width; x++)
			{
				for (int y = 0; y < area.Height; y++)
				{
					arr[x + area.X, y + area.Y] = value();
				}
			}
		}

		public static Rectangle GetRect<T>(this T[,] arr)
        {
			return new Rectangle(0, 0, arr.GetLength(0), arr.GetLength(1));
        }

		public static T Pop<T>(this List<T> list)
        {
			T poppedElement = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return poppedElement;
        }
	}
}
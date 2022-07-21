using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Pandorai.Utility
{
	public delegate float DistanceMeasurer<T>(Point point, T element);

	public static class Helper
	{
		public static Point NullPoint = new Point(int.MinValue, int.MinValue);

		public static T FindClosestPoint<T>(Point comparedPoint, List<T> collection, DistanceMeasurer<T> distanceMeasurer)
		{
			T closestElement = default;

			float lowestDist = float.MaxValue - 1;
			foreach (var element in collection)
			{
				float dist = distanceMeasurer(comparedPoint, element);
				if (dist < lowestDist)
				{
					lowestDist = dist;
					closestElement = element;
				}
			}

			return closestElement;
		}

		public static Color GetColorFromHex(string hex)
		{
			var systemDrawingColor = System.Drawing.ColorTranslator.FromHtml(hex.ToUpper());
			var xnaColor = new Color(systemDrawingColor.R, systemDrawingColor.G, systemDrawingColor.B);
			return xnaColor;
		}

		public static Color CreateHSLColor(int hue, float saturation, float luminance)
		{
			byte r = 0;
			byte g = 0;
			byte b = 0;

			if (saturation == 0)
			{
				r = g = b = (byte)(luminance * 255);
			}
			else
			{
				float v1, v2;
				float h = (float)hue / 360;

				v2 = (luminance < 0.5) ? (luminance * (1 + saturation)) : ((luminance + saturation) - (luminance * saturation));
				v1 = 2 * luminance - v2;

				r = (byte)(255 * HueToRGB(v1, v2, h + (1.0f / 3)));
				g = (byte)(255 * HueToRGB(v1, v2, h));
				b = (byte)(255 * HueToRGB(v1, v2, h - (1.0f / 3)));
			}
			
			return new Color(r, g , b);
		}

		private static float HueToRGB(float v1, float v2, float vH)
		{
			if (vH < 0)
				vH += 1;

			if (vH > 1)
				vH -= 1;

			if ((6 * vH) < 1)
				return (v1 + (v2 - v1) * 6 * vH);

			if ((2 * vH) < 1)
				return v2;

			if ((3 * vH) < 2)
				return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

			return v1;
		}

		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		public static Range GetNumberRangeFromString(string input)
		{
			Range result = new Range();
			var match = Regex.Match(input, @"(-?\d+)-?\(?(-?\d*)\)?");
			result.Min = int.Parse(match.Groups[1].Value);
			result.Max = match.Groups[2].Value != string.Empty ? int.Parse(match.Groups[2].Value) : result.Min;
			return result;
		}

		public static List<string> GetStringChoice(string input)
		{
			List<string> result = new List<string>();
			var matches = Regex.Matches(input, @"\w+");
			foreach (Match match in matches)
			{
				result.Add(match.Value);
			}
			return result;
		}

		public static T SelectRandomEnum<T>(Random rng, int[] weights = null)
		{
			var values = Enum.GetValues(typeof(T));

			if(weights == null)
			{
				return (T)values.GetValue(rng.Next(values.Length));
			}
			else if(weights.Length != values.Length)
			{
				Debug.Print("Provided weights don't match this enum");
				return default(T);
			}
			else
			{
				var weightedValues = new List<T>();
				for (int i = 0; i < weights.Length; i++)
				{
					var currentElements = new T[weights[i]];
					currentElements.Populate((T)values.GetValue(i));
					weightedValues.AddRange(currentElements);
				}
				return weightedValues.GetRandomElement(rng);
			}
		}

		public static Vector3 GetRandomVector3(Random rng)
		{
			return new Vector3(rng.NextFloat(), rng.NextFloat(), rng.NextFloat());
		}
	}
}

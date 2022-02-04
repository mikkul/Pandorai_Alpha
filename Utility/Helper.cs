using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using System.Linq;
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

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Pandorai.Utility;
using System.Xml;
using System;

namespace Pandorai.MapGeneration
{
	public static class WFCSampleLoader
	{
		private static Dictionary<string, WFCSample> samples = new Dictionary<string, WFCSample>();

		public static Dictionary<string, WFCSample> Samples { get => samples; }

		public static void InitSamples(string folderLocation, string sampleSpreadsheetPath, Game1 game)
		{
			XmlDocument spreadsheet = new XmlDocument();
			spreadsheet.Load(sampleSpreadsheetPath);

			string[] files = Directory.GetFiles(folderLocation);
			foreach (var file in files)
			{
				string sampleName = Path.GetFileNameWithoutExtension(file);
				XmlElement element = (XmlElement)spreadsheet.DocumentElement.SelectSingleNode($"/samples/sample[@name='{sampleName}']");
				var tileLegend = element.SelectSingleNode("./tileLegend");
				WFCSample sample = LoadSample(file, tileLegend, game);
				sample.IsPeriodic = bool.Parse(element.GetAttribute("isPeriodic"));

				samples.Add(sampleName, sample);
			}
		}

		private static WFCSample LoadSample(string pathToSample, XmlNode tileLegend, Game1 game)
		{
			WFCSample sample = new WFCSample();

			foreach (XmlElement node in tileLegend.ChildNodes)
			{
				Color color = Helper.GetColorFromHex(node.GetAttribute("color"));
				TileType type = (TileType)Enum.Parse(typeof(TileType), node.GetAttribute("type"), true);
				float multiplier = 1.0f;
				if(node.HasAttribute("multiplier"))
				{
					multiplier = float.Parse(node.GetAttribute("multiplier"));
				}
				sample.TileColorLegend.Add(color, type);
				sample.ColorMultiplierLegend.Add(color, multiplier);
			}

			var inputImg = new System.Drawing.Bitmap(pathToSample);
			int[,] sampleData = new int[inputImg.Width, inputImg.Height];

			List<Color> existingColors = new List<Color>();

			for (int x = 0; x < inputImg.Width; x++)
			{
				for (int y = 0; y < inputImg.Height; y++)
				{
					System.Drawing.Color pixelColor = inputImg.GetPixel(x, y);
					sampleData[x, y] = pixelColor.ToArgb();

					var xnaPixelColor = pixelColor.ToXnaColor();

					if (!existingColors.Contains(xnaPixelColor))
					{
						existingColors.Add(pixelColor.ToXnaColor());
						if(!sample.TilePositionLegend.ContainsKey(sample.TileColorLegend[xnaPixelColor]))
							sample.TilePositionLegend.Add(sample.TileColorLegend[xnaPixelColor], new Point(x, y));
					}
				}
			}

			sample.Data = sampleData;

			return sample;
		}
	}
}

using DeBroglie;
using DeBroglie.Models;
using DeBroglie.Topo;
using DeBroglie.Constraints;
using Microsoft.Xna.Framework;
using Pandorai.MapGeneration.CustomRegions;
using System;
using System.Collections.Generic;
using System.IO;
using Pandorai.Utility;

namespace Pandorai.MapGeneration
{
	public static class WFCGenerator
	{
		public static int[,] GetOutput(WFCSample sampleObj, bool isInputPeriodic, bool isOutputPeriodic, int outputWidth, int outputHeight, AreaDataType[,] constraint = null)
		{
			// input
			ITopoArray<Tile> sample = TopoArray.Create(sampleObj.Data, isInputPeriodic).ToTiles();

			var model = new OverlappingModel(sample, 3, 4, true);
			foreach (var multiplier in sampleObj.ColorMultiplierLegend)
			{
				var tilePos = sampleObj.TilePositionLegend[sampleObj.TileColorLegend[multiplier.Key]];
				model.MultiplyFrequency(sample.Get(tilePos.X, tilePos.Y), multiplier.Value);
			}

			// output dimensions
			var outputTopology = new GridTopology(outputWidth, outputHeight, isOutputPeriodic);

			// run wfc and return the output
			TilePropagator propagator = null;
			Resolution result = Resolution.Contradiction;
			while (result == Resolution.Contradiction)
			{
				propagator = new TilePropagator(model, outputTopology, false);

				if (constraint != null)
				{
					for (int x = 0; x < constraint.GetLength(0); x++)
					{
						for (int y = 0; y < constraint.GetLength(1); y++)
						{
							Microsoft.Xna.Framework.Point tilePos;

							if (constraint[x, y] == AreaDataType.PreplacedFloor)
							{
								tilePos = sampleObj.TilePositionLegend[TileType.Floor];
								propagator.Select(x, y, 0, sample.Get(tilePos.X, tilePos.Y));
							}
							else if (constraint[x, y] == AreaDataType.PreplacedBorder)
							{
								tilePos = sampleObj.TilePositionLegend[TileType.Wall];
								propagator.Select(x, y, 0, sample.Get(tilePos.X, tilePos.Y));
							}
							else if (constraint[x, y] == AreaDataType.Locked)
							{
								tilePos = sampleObj.TilePositionLegend[TileType.Empty];
								propagator.Select(x, y, 0, sample.Get(tilePos.X, tilePos.Y));
							}
						}
					}
				}

				result = propagator.Run();
				Console.WriteLine(result);
			}

			var output = propagator.ToValueArray<int>().ToArray2d();
			return output;
		}
	}
}

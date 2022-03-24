using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Tilemaps;
using Pandorai.Utility;
using System;
using System.Collections.Generic;

namespace Pandorai.ParticleSystems
{
	public static class ParticleSystemManager
	{
		static List<ParticleSystem> dependentParticleSystems;
		static List<ParticleSystem> undergroundParticleSystems = new List<ParticleSystem>();
		static List<ParticleSystem> surfaceParticleSystems = new List<ParticleSystem>();

		static List<ParticleSystem> independentParticleSystems = new List<ParticleSystem>();

		public static Main game;

		public static void MapSwitchHandler(ActiveMap activeMap)
		{
			if (activeMap == ActiveMap.Underground)
			{
				dependentParticleSystems = undergroundParticleSystems;
			}
			else
			{
				dependentParticleSystems = surfaceParticleSystems;
			}
		}

		public static void AdjustToTileSize(int oldSize, int newSize)
		{
			Adjust(undergroundParticleSystems);
			Adjust(surfaceParticleSystems);
			Adjust(independentParticleSystems);

			void Adjust(List<ParticleSystem> list)
			{
				foreach (var system in list)
				{
					system.CentralPosition *= (float)newSize / (float)oldSize;
				}
			}
		}

		public static void Update(GameTime dt)
		{
			for (int i = dependentParticleSystems.Count - 1; i >= 0; i--)
			{
				var system = dependentParticleSystems[i];
				if (system == null)
				{
					dependentParticleSystems.RemoveAt(i);
					continue;
				}

				if (system.IsInViewport())
				{
					if (!system.Update(dt)) // system should die out
					{
						dependentParticleSystems.RemoveAt(i);
					}
				}
			}

			for (int i = independentParticleSystems.Count - 1; i >= 0; i--)
			{
				var system = independentParticleSystems[i];
				if (system == null)
				{
					dependentParticleSystems.RemoveAt(i);
					continue;
				}

				if (system.IsInViewport())
				{
					if (!system.Update(dt)) // system should die out
					{
						independentParticleSystems.RemoveAt(i);
					}
				}
			}
		}

		public static void Render(SpriteBatch batch)
		{
			for (int i = dependentParticleSystems.Count - 1; i >= 0; i--)
			{
				var system = dependentParticleSystems[i];
				if (system.IsInViewport())
				{
					system.Draw(batch);
				}
			}

			for (int i = independentParticleSystems.Count - 1; i >= 0; i--)
			{
				var system = independentParticleSystems[i];
				if (system.IsInViewport())
				{
					system.Draw(batch);
				}
			}
		}

		public static void Clear()
		{
			dependentParticleSystems.Clear();
			independentParticleSystems.Clear();
		}

		public static void AddSystem(ParticleSystem system, bool isDependent)
		{
			if(isDependent)
			{
				dependentParticleSystems.Add(system);
			}
			else
			{
				independentParticleSystems.Add(system);
			}
		}
	}
}

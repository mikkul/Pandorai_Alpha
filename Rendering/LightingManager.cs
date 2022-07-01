using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Tilemaps;
using Pandorai.Utility;

namespace Pandorai.Rendering
{
	public class LightSource
	{
		public Vector2 Position;
		public float Radius;
		public Color Color;
		public float Intensity;

		public LightSource(Vector2 pos, float radius)
		{
			Position = pos;
			Radius = radius;
			Color = Color.White;
			Intensity = 1.0f;
		}

		public LightSource(Vector2 pos, float radius, Color color)
		{
			Position = pos;
			Radius = radius;
			Color = color;
			Intensity = 1.0f;
		}

		public LightSource(Vector2 pos, float radius, Color color, float intensity)
		{
			Position = pos;
			Radius = radius;
			Color = color;
			Intensity = intensity;
		}
	}

	public struct LightingMask
	{
		public Texture2D IntensityMask;
		public Texture2D ColorMask;
	}

	public static class LightingManager
	{
		public static Texture2D LightSourceMask;
		public static Effect LightingMaskEffect;

		public static float AmbientLight = 0.0f;

		static List<LightSource> lightSources;
		static List<LightSource> undergroundLightSources = new List<LightSource>();
		static List<LightSource> surfaceLightSources = new List<LightSource>();

		static RenderTarget2D renderTarget;
		static RenderTarget2D renderTarget2;

		public static void MapSwitchHandler(ActiveMap activeMap)
		{
			if(activeMap == ActiveMap.Underground)
			{
				lightSources = undergroundLightSources;
				AmbientLight = 0.30f;
			}
			else
			{
				lightSources = surfaceLightSources;
				AmbientLight = 0.80f;
			}
		}

		public static void AdjustToTileSize(int oldSize, int newSize)
		{
			Adjust(undergroundLightSources);
			Adjust(surfaceLightSources);

			void Adjust(List<LightSource> list)
			{
				foreach (var lightSource in list)
				{
					lightSource.Position *= (float)newSize / (float)oldSize;
					lightSource.Radius = (float)newSize / (float)oldSize * (float)lightSource.Radius;
				}
			}
		}

		public static void ClearLightSources()
		{
			lightSources.Clear();
		}

		public static LightSource AddLightSource(Vector2 location, int radius)
		{
			LightSource light = new LightSource(location, radius);
			lightSources.Add(light);
			return light;
		}

		public static LightSource AddLightSource(Vector2 location, int radius, Color color)
		{
			LightSource light = new LightSource(location, radius, color);
			lightSources.Add(light);
			return light;
		}

		public static LightSource AddLightSource(Vector2 location, int radius, Color color, float intensity)
		{
			LightSource light = new LightSource(location, radius, color, intensity);
			lightSources.Add(light);
			return light;
		}

		public static void RemoveLightSource(LightSource lightSource)
		{
			lightSources.Remove(lightSource);
		}

		public static void RefreshRenderTarget(GraphicsDevice graphics, Camera camera)
		{
			renderTarget = new RenderTarget2D(graphics, camera.Viewport.Width, camera.Viewport.Height);
			renderTarget2 = new RenderTarget2D(graphics, camera.Viewport.Width, camera.Viewport.Height);
		}

		public static LightingMask CreateLightingMask(GraphicsDevice graphics, SpriteBatch batch, Camera camera)
		{
			graphics.SetRenderTarget(renderTarget);
			batch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			foreach (var lightSource in lightSources)
			{
				var viewportPos = camera.GetViewportPosition(lightSource.Position);
				if (camera.Viewport.Enlarge((int)lightSource.Radius, (int)lightSource.Radius).Contains(viewportPos))
				{
					var rect = new Rectangle((int)(viewportPos.X - lightSource.Radius), (int)(viewportPos.Y - lightSource.Radius), (int)lightSource.Radius * 2, (int)lightSource.Radius * 2);

					if(lightSource.Intensity > 1.0f)
					{
						batch.Draw(LightSourceMask, rect, Color.White);
						batch.Draw(LightSourceMask, rect, Color.White.ModifyMultiply(a: lightSource.Intensity - 1));
					}
					else
					{
						batch.Draw(LightSourceMask, rect, Color.White.ModifyMultiply(a: lightSource.Intensity));
					}
				}
			}
			batch.End();

			graphics.SetRenderTarget(renderTarget2);
			graphics.Clear(Color.Black);
			batch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			foreach (var lightSource in lightSources)
			{
				var viewportPos = camera.GetViewportPosition(lightSource.Position);
				if (camera.Viewport.Enlarge((int)lightSource.Radius, (int)lightSource.Radius).Contains(viewportPos))
				{
					var rect = new Rectangle((int)(viewportPos.X - lightSource.Radius), (int)(viewportPos.Y - lightSource.Radius), (int)lightSource.Radius * 2, (int)lightSource.Radius * 2);

					batch.Draw(LightSourceMask, rect, lightSource.Color);
				}
			}
			batch.End();

			return new LightingMask
			{
				IntensityMask = renderTarget,
				ColorMask = renderTarget2,
			};
		}
	}
}

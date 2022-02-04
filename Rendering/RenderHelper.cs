using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Pandorai.Rendering
{
	public delegate int IntReturnDelegate();

	public class RenderHelper
	{
		private RenderTarget2D renderTarget;
		private RenderTarget2D postprocessingBuffer1;
		private RenderTarget2D postprocessingBuffer2;

		private List<Effect> postprocessingEffects = new List<Effect>();

		private IntReturnDelegate widthCalculationFunc;
		private IntReturnDelegate heightCalculationFunc;

		private Game1 game;

		public RenderHelper(Game1 _game, IntReturnDelegate widthCalc, IntReturnDelegate heightCalc)
		{
			game = _game;
			widthCalculationFunc = widthCalc;
			heightCalculationFunc = heightCalc;
			RefreshRenderTargets();
		}

		public void RefreshRenderTargets()
		{
			renderTarget = new RenderTarget2D(game.GraphicsDevice, widthCalculationFunc(), heightCalculationFunc());
			postprocessingBuffer1 = new RenderTarget2D(game.GraphicsDevice, widthCalculationFunc(), heightCalculationFunc());
			postprocessingBuffer2 = new RenderTarget2D(game.GraphicsDevice, widthCalculationFunc(), heightCalculationFunc());
		}

		public void ApplyEffect(Effect fx)
		{
			postprocessingEffects.Add(fx);
		}

		public Texture2D RenderTexture(SpriteBatch spriteBatch, RenderTarget2D texture)
		{
			RenderTarget2D lastBuffer = null;
			RenderTarget2D currentBuffer = postprocessingBuffer1;

			game.GraphicsDevice.SetRenderTarget(postprocessingBuffer1);
			spriteBatch.Begin();
			spriteBatch.Draw(texture, renderTarget.Bounds, Color.White);
			spriteBatch.End();

			for (int i = 0; i < postprocessingEffects.Count; i++)
			{
				if(i % 2 == 0) // even number
				{
					currentBuffer = postprocessingBuffer2;
					lastBuffer = postprocessingBuffer1;
				}
				else // odd number
				{
					currentBuffer = postprocessingBuffer1;
					lastBuffer = postprocessingBuffer2;
				}

				game.GraphicsDevice.SetRenderTarget(currentBuffer);
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, effect: postprocessingEffects[i]);
				spriteBatch.Draw(lastBuffer, renderTarget.Bounds, Color.White);
				spriteBatch.End();
			}

			game.GraphicsDevice.SetRenderTarget(renderTarget);
			spriteBatch.Begin();
			spriteBatch.Draw(currentBuffer, renderTarget.Bounds, Color.White);
			spriteBatch.End();
			game.GraphicsDevice.SetRenderTarget(null);

			postprocessingEffects.Clear();

			return renderTarget;
		}
	}
}

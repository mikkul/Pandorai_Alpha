using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace Pandorai.Rendering
{
	public class RenderHelper
	{
		private RenderTarget2D _renderTarget;
		private RenderTarget2D _postprocessingBuffer1;
		private RenderTarget2D _postprocessingBuffer2;

		private List<Effect> _postprocessingEffects = new List<Effect>();

		private Func<int> _widthCalculationFunc;
		private Func<int> _heightCalculationFunc;


		public RenderHelper(Func<int> widthCalc, Func<int> heightCalc)
		{
			_widthCalculationFunc = widthCalc;
			_heightCalculationFunc = heightCalc;
			RefreshRenderTargets();
		}

		public void RefreshRenderTargets()
		{
			_renderTarget = new RenderTarget2D(Main.Game.GraphicsDevice, _widthCalculationFunc(), _heightCalculationFunc());
			_postprocessingBuffer1 = new RenderTarget2D(Main.Game.GraphicsDevice, _widthCalculationFunc(), _heightCalculationFunc());
			_postprocessingBuffer2 = new RenderTarget2D(Main.Game.GraphicsDevice, _widthCalculationFunc(), _heightCalculationFunc());
		}

		public void ApplyEffect(Effect fx)
		{
			_postprocessingEffects.Add(fx);
		}

		public Texture2D RenderTexture(SpriteBatch spriteBatch, RenderTarget2D texture)
		{
			RenderTarget2D lastBuffer = null;
			RenderTarget2D currentBuffer = _postprocessingBuffer1;

			Main.Game.GraphicsDevice.SetRenderTarget(_postprocessingBuffer1);
			spriteBatch.Begin();
			spriteBatch.Draw(texture, _renderTarget.Bounds, Color.White);
			spriteBatch.End();

			for (int i = 0; i < _postprocessingEffects.Count; i++)
			{
				if(i % 2 == 0) // even number
				{
					currentBuffer = _postprocessingBuffer2;
					lastBuffer = _postprocessingBuffer1;
				}
				else // odd number
				{
					currentBuffer = _postprocessingBuffer1;
					lastBuffer = _postprocessingBuffer2;
				}

				Main.Game.GraphicsDevice.SetRenderTarget(currentBuffer);
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, effect: _postprocessingEffects[i]);
				spriteBatch.Draw(lastBuffer, _renderTarget.Bounds, Color.White);
				spriteBatch.End();
			}

			Main.Game.GraphicsDevice.SetRenderTarget(_renderTarget);
			spriteBatch.Begin();
			spriteBatch.Draw(currentBuffer, _renderTarget.Bounds, Color.White);
			spriteBatch.End();
			Main.Game.GraphicsDevice.SetRenderTarget(null);

			_postprocessingEffects.Clear();

			return _renderTarget;
		}
	}
}

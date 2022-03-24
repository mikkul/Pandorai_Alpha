using Microsoft.Xna.Framework;

namespace Pandorai.Sprites
{
	public class TilesheetSprite
	{
		public Rectangle Rect;

		public int AnimatedFrameCount;
		public float TimePerFrame; // in miliseconds

		private int _currentAnimationFrame;
		private float _timeElapsed;

		public TilesheetSprite(Rectangle rect, int frameCount, float timePerFrame)
		{
			Rect = rect;
			AnimatedFrameCount = frameCount;
			TimePerFrame = timePerFrame;
		}

		public void Update(float dt)
		{
			if (AnimatedFrameCount <= 0) return;

			_timeElapsed += dt * 1000;
			if(_timeElapsed >= TimePerFrame)
			{
				_timeElapsed -= TimePerFrame;
				Rect.X += Rect.Width;
				_currentAnimationFrame++;
				if(_currentAnimationFrame >= AnimatedFrameCount)
				{
					_currentAnimationFrame = 0;
					Rect.X -= Rect.Width * AnimatedFrameCount;
				}
			}
		}
	}
}

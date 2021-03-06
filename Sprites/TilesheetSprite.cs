using Microsoft.Xna.Framework;

namespace Pandorai.Sprites
{
	public class TilesheetSprite
	{
		public Rectangle Rect;

		public int AnimatedFrameCount;
		public float TimePerFrame; // in miliseconds

		private int currentAnimationFrame;
		private float timeElapsed;

		public TilesheetSprite(Rectangle rect, int frameCount, float timePerFrame)
		{
			Rect = rect;
			AnimatedFrameCount = frameCount;
			TimePerFrame = timePerFrame;
		}

		public void Update(float dt)
		{
			if (AnimatedFrameCount <= 0) return;

			timeElapsed += dt * 1000;
			if(timeElapsed >= TimePerFrame)
			{
				timeElapsed -= TimePerFrame;
				Rect.X += Rect.Width;
				currentAnimationFrame++;
				if(currentAnimationFrame >= AnimatedFrameCount)
				{
					currentAnimationFrame = 0;
					Rect.X -= Rect.Width * AnimatedFrameCount;
				}
			}
		}
	}
}

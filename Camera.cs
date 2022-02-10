using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using Pandorai.Rendering;
using Pandorai.Sounds;
using System;

namespace Pandorai
{
	public class Camera
	{
		public Vector2 Position;

		public Rectangle Viewport;

		public bool IsFollowing = false;
		public Vector2 FollowedBody = Vector2.Zero;
		public float FollowingSpeed = 25;

		public bool IsPannning = false;
		public Vector2 PanDestination = Vector2.Zero;
		public float PanningSpeed = 0.5f;
		public float PanningCurveSteepness = 5;
		public float PanningCurveBias = 2.5f;

		private float panLerpPercentage = 0;

		public Shake CameraShake { 
			set
			{
				ShakeX = new Shake(value.Duration, value.Frequency, value.Amplitude, value.Rng);
				ShakeY = new Shake(value.Duration, value.Frequency, value.Amplitude, value.Rng);
			}
		}

		public Shake ShakeX;
		public Shake ShakeY;

		public Camera()
		{
			// initialize a viewport relative to the game window
			Viewport = new Rectangle(0, 0, 0, 0);

			CameraShake = new Shake(0, 0, 0, null);
		}

		public void ShakeCamera()
		{
			ShakeX.StartShaking();
			ShakeY.StartShaking();
			SoundManager.PlaySound("explosion_low");
		}

		public Vector2 GetViewportPosition(Vector2 worldPosition)
		{
			return worldPosition - Position;
		}

		public void UpdateViewport(Options options)
		{
			Viewport.Width = (int)(Options.oldResolution.X * 0.75);
			Viewport.Height = (int)(Options.oldResolution.Y * 0.75);
		}

		public bool IsInViewport(Vector2 vector)
		{
			if(vector.X >= Viewport.Left && vector.X < Viewport.Right && vector.Y >= Viewport.Top && vector.Y < Viewport.Bottom)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public void Follow(Vector2 followed)
		{
			IsFollowing = true;
			FollowedBody = followed;
		}

		public void StartPanning(Vector2 destination)
		{
			panLerpPercentage = 0;
			PanDestination = destination;
			IsPannning = true;
			IsFollowing = false;
		}

		public void Update(float dt)
		{
			if(IsFollowing)
			{
				//Position.X += (FollowedBody.X - Viewport.Width / 2 - Position.X) * FollowingSpeed * dt;
				//Position.Y += (FollowedBody.Y - Viewport.Height / 2 - Position.Y) * FollowingSpeed * dt;
				Position.X = FollowedBody.X - Viewport.Width / 2;
				Position.Y = FollowedBody.Y - Viewport.Height / 2;
			}

			else if(IsPannning)
			{
				panLerpPercentage += PanningSpeed * dt;

				double lerpValue = 1 / (1 + Math.Pow((PanningCurveBias * panLerpPercentage) / (1 - panLerpPercentage), -PanningCurveSteepness));

				if (panLerpPercentage >= 1)
				{
					lerpValue = 1;
					IsPannning = false;
				}

				Vector2 lerpVector = Vector2.Lerp(Position, PanDestination, (float)lerpValue);

				Position = new Vector2(lerpVector.X, lerpVector.Y);
			}
		}
	}
}

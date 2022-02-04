using Pandorai.Utility;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Pandorai.Rendering
{
	public class Shake
	{
		public float Value { get => value; }
		private float value;

		private int duration;
		private int frequency;
		private int amplitude;
		private float[] samples;
		private int timePassed;
		private Random rng;

		private Timer timer = null;

		public int Duration { get => duration; }
		public int Frequency { get => frequency; }
		public int Amplitude { get => amplitude; }
		public Random Rng { get => rng; }

		/// <param name="_duration">duration in miliseconds</param> 
		/// <param name="_frequency">frequency in Hz (times per second)</param> 
		/// <param name="_amplitude">maximum displacement in pixels</param> 
		public Shake(int _duration, int _frequency, int _amplitude, Random _rng)
		{
			rng = _rng;

			duration = _duration;
			frequency = _frequency;
			amplitude = _amplitude;

			int sampleCount = (int)(((float)duration / 1000f) * frequency);
			samples = new float[sampleCount];
			for (int i = 0; i < sampleCount; i++)
			{
				samples[i] = (float)rng.NextDouble() * 2 - 1;
			}
		}

		public void StartShaking()
		{
			if (timer != null && timer.Enabled) return;

			timePassed = 0;

			timer = new Timer(1);
			timer.Elapsed += (o, e) =>
			{
				value = GetAmplitude();
				Console.WriteLine(value);
				timePassed += 10;
				if(timePassed >= duration)
				{
					timer.Stop();
					timer.Dispose();
					value = 0;
					return;
				}	
			};
			timer.Enabled = true;
		}

		private float GetAmplitude()
		{
			float s = (float)timePassed / 1000f * (float)frequency;
			int sample0 = (int)Math.Floor(s);
			int sample1 = sample0 + 1;
			float decay = GetDecay();
			return (GetSample(sample0) + (s - sample0) * (GetSample(sample1) - GetSample(sample0))) * decay * amplitude;
		}

		private float GetSample(int index)
		{
			if (index >= samples.Length || index < 0) return 0;

			return samples[index];
		}

		private float GetDecay()
		{
			return (float)(duration - timePassed) / duration;
		}
	}
}

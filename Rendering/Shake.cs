using System;
using System.Timers;

namespace Pandorai.Rendering
{
	public class Shake
	{
		private float _value;
		private int _duration;
		private int _frequency;
		private int _amplitude;
		private float[] _samples;
		private int _timePassed;
		private Random _rng;

		private Timer timer = null;

		public float Value { get => _value; }
		public int Duration { get => _duration; }
		public int Frequency { get => _frequency; }
		public int Amplitude { get => _amplitude; }
		public Random Rng { get => _rng; }

		/// <param name="_duration">duration in miliseconds</param> 
		/// <param name="_frequency">frequency in Hz (times per second)</param> 
		/// <param name="_amplitude">maximum displacement in pixels</param> 
		public Shake(int duration, int frequency, int amplitude, Random rng)
		{
			_rng = rng;

			_duration = duration;
			_frequency = frequency;
			_amplitude = amplitude;

			int sampleCount = (int)(((float)_duration / 1000f) * _frequency);
			_samples = new float[sampleCount];
			for (int i = 0; i < sampleCount; i++)
			{
                _samples[i] = (float)_rng.NextDouble() * 2 - 1;
			}
		}

		public void StartShaking()
		{
			if (timer != null && timer.Enabled) return;

			_timePassed = 0;

			timer = new Timer(1);
			timer.Elapsed += (o, e) =>
			{
				_value = GetAmplitude();
				Console.WriteLine(_value);
				_timePassed += 10;
				if(_timePassed >= _duration)
				{
					timer.Stop();
					timer.Dispose();
					_value = 0;
					return;
				}	
			};
			timer.Enabled = true;
		}

		private float GetAmplitude()
		{
			float s = (float)_timePassed / 1000f * (float)_frequency;
			int sample0 = (int)Math.Floor(s);
			int sample1 = sample0 + 1;
			float decay = GetDecay();
			return (GetSample(sample0) + (s - sample0) * (GetSample(sample1) - GetSample(sample0))) * decay * _amplitude;
		}

		private float GetSample(int index)
		{
			if (index >= _samples.Length || index < 0) return 0;

			return _samples[index];
		}

		private float GetDecay()
		{
			return (float)(_duration - _timePassed) / _duration;
		}
	}
}

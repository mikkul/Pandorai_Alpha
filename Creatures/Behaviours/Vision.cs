using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pandorai.Creatures.Behaviours
{
	public abstract class Vision : Behaviour
	{
		private bool _enabled = true;

		public List<Point> VisibleTiles { get; protected set; } = new List<Point>();

		public sealed override void Bind()
		{
			Owner.TurnEnded += CalculateVisibleTiles;
		}

		public void Enable()
		{
			_enabled = true;
			CalculateFOV();
		}

		public void Disable()
		{
			_enabled = false;
			VisibleTiles.Clear();
		}

		protected abstract void CalculateFOV();

		private void CalculateVisibleTiles()
		{
			if(_enabled)
			{
				CalculateFOV();
			}
		}
	}
}

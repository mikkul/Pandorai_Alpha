using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Pandorai.Creatures.Behaviours
{
	public class ParalelPosition : Behaviour
	{
		public override Behaviour Clone()
		{
			return new ParalelPosition
			{

			};
		}

		public override void SetAttribute(string name, string value)
		{
			
		}

		public override void Bind()
		{
			Owner.TurnCame += Work;
		}

		private void Work()
		{
			if (Owner.Target == Owner.MapIndex) return;

			var diffX = Owner.Target.X - Owner.MapIndex.X;
			var diffY = Owner.Target.Y - Owner.MapIndex.Y;

			if (diffX == 0 || diffY == 0) return;

			var availableTiles = Utility.GenHelper.GetNeighbours(Owner.MapIndex).Where(t => !Owner.Game.Map.GetTile(t).CollisionFlag);
			Point bestTile = Owner.MapIndex;
			int bestDiff = int.MaxValue;
			foreach (var tile in availableTiles)
			{
				int diff = Math.Min(Math.Abs(tile.X - Owner.Target.X), Math.Abs(tile.Y - Owner.Target.Y));
				if (diff < bestDiff)
				{
					bestDiff = diff;
					bestTile = tile;
				}
			}

			Owner.RequestMovement(bestTile);
		}
	}
}

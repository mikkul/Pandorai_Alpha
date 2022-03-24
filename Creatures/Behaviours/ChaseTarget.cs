using Pandorai.AStarSearchAlgorithm;

namespace Pandorai.Creatures.Behaviours
{
	public class ChaseTarget : Behaviour
	{
		public string Movement;

		public override Behaviour Clone()
		{
			return new ChaseTarget
			{
				Movement = Movement,
			};
		}

		public override void SetAttribute(string name, string value)
		{
			if (name == "Movement")
			{
				Movement = value;
			}
		}

		public override void Bind()
		{
			Owner.TurnCame += Work;
		}

		private void Work()
		{
			if (Owner.Target == Owner.MapIndex) return;

			var eightDirections = Movement == "Diagonal";
			var pathToTarget = AStarCreatures.GetShortestPath(Owner.Game.Map.Tiles, Owner.MapIndex, Owner.Target, true, true, eightDirections);
			if (pathToTarget.Count > 0)
				Owner.RequestMovement(pathToTarget[0]);
		}
	}
}
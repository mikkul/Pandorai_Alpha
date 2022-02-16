using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pandorai.Utility;

namespace Pandorai.Creatures.Behaviours
{
    public class RandomWalk : Behaviour
    {
        public int MaxDistanceFromHome { get; set; }
        public string Movement { get; set; }

        private Point _homePosition;
        private bool _isFirstTurn = true;

        public override void Bind()
        {
            Owner.TurnCame += Work;
        }

        private void Work()
        {
            if(_isFirstTurn)
            {
                _homePosition = Owner.MapIndex;
                _isFirstTurn = false;
                return;
            }

            if(Owner.Target != Owner.MapIndex)
            {
                return;
            }

            bool eightDirections = Movement == "Diagonal";
            List<Point> possibleMovementPoints = new();
            if(eightDirections)
            {
                possibleMovementPoints.AddRange(GenHelper.Get8Neighbours(Owner.MapIndex).ToList());
            }
            else
            {
                possibleMovementPoints.AddRange(GenHelper.GetNeighbours(Owner.MapIndex).ToList());
            }

            var chosenPoint = possibleMovementPoints.GetRandomElement(Game1.game.mainRng);
            var targetDistanceToHome = Vector2.Distance(chosenPoint.ToVector2(), _homePosition.ToVector2());
            var currentDistanceToHome = Vector2.Distance(Owner.MapIndex.ToVector2(), _homePosition.ToVector2());

            if(chosenPoint != Owner.MapIndex && (targetDistanceToHome <= MaxDistanceFromHome || targetDistanceToHome < currentDistanceToHome))
            {
                Owner.Target = chosenPoint;
                Owner.RequestMovement(chosenPoint);
            }
        }

        public override Behaviour Clone()
        {
            return new RandomWalk
            {
                MaxDistanceFromHome = MaxDistanceFromHome,
                Movement = Movement,
            };
        }

        public override void SetAttribute(string name, string value)
        {
            if(name == "MaxDistanceFromHome")
            {
                MaxDistanceFromHome = int.Parse(value);
            }
            else if (name == "Movement")
			{
				Movement = value;
			}
        }
    }
}
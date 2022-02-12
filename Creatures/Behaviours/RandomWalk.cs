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

        private Point _homePosition = Point.Zero;

        public override void Bind()
        {
            Owner.TurnCame += Work;
        }

        private void Work()
        {
            if(_homePosition == Point.Zero)
            {
                _homePosition = Owner.MapIndex;
            }

            if(Owner.Target != Owner.MapIndex)
            {
                return;
            }

            bool eightDirections = Movement == "Diagonal";
            List<Point> possibleMovementPoints = new() { Owner.MapIndex, Owner.MapIndex };
            if(eightDirections)
            {
                possibleMovementPoints.AddRange(GenHelper.Get8Neighbours(Owner.MapIndex).ToList());
            }
            else
            {
                possibleMovementPoints.AddRange(GenHelper.GetNeighbours(Owner.MapIndex).ToList());
            }

            var chosenPoint = possibleMovementPoints.GetRandomElement(Game1.game.mainRng);
            var distanceToHome = Vector2.Distance(chosenPoint.ToVector2(), _homePosition.ToVector2());
            if(chosenPoint != Owner.MapIndex && distanceToHome < MaxDistanceFromHome)
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
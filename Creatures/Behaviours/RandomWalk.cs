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
        private List<Point> _dangerousPoints = new();

        public override void Bind()
        {
            Owner.TurnCame += Work;
            Owner.GotHit += RememberDangerousTiles;
        }

        private void RememberDangerousTiles(Creature creature)
        {
            // only tiles where the owner got hit by himself are traps
            if(creature != Owner)
            {
                return;
            }

            _dangerousPoints.Add(Owner.MapIndex);
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

            var chosenPoint = possibleMovementPoints.GetRandomElement(Main.Game.MainRng);
            var targetDistanceToHome = Vector2.Distance(chosenPoint.ToVector2(), _homePosition.ToVector2());
            var currentDistanceToHome = Vector2.Distance(Owner.MapIndex.ToVector2(), _homePosition.ToVector2());

            if(chosenPoint != Owner.MapIndex && (targetDistanceToHome <= MaxDistanceFromHome || targetDistanceToHome < currentDistanceToHome) && !_dangerousPoints.Contains(chosenPoint))
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
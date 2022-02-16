using System;

namespace Pandorai.Creatures.Behaviours
{
    public class PlaceWebs : Behaviour
    {
        public int Frequency;
        public int Duration;

        private int _turnCounter;

        public override void Bind()
        {
            Owner.TurnEnded += Work;
        }

        private void Work()
        {
            _turnCounter++;

            if(_turnCounter >= Frequency)
            {
                _turnCounter = 0;
                // place a web
            }
        }

        public override Behaviour Clone()
        {
            return new PlaceWebs
            {
                Frequency = Frequency,
                Duration = Duration,
            };
        }

        public override void SetAttribute(string name, string value)
        {
            if(name == "Frequency")
            {
                Frequency = int.Parse(value);
            }
            else if(name == "Duration")
            {
                Duration = int.Parse(value);
            }
        }
    }
}
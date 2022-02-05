using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pandorai.Creatures.Behaviours
{
    public class AllSeeingVision : Vision
    {
        public override Behaviour Clone()
        {
            return new AllSeeingVision();
        }

        public override void SetAttribute(string name, string value)
        {
            
        }

        protected override void CalculateFOV()
        {
            VisibleTiles.Clear();
            for (int x = 0; x < Owner.game.Map.Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Owner.game.Map.Tiles.GetLength(1); y++)
                {
                    VisibleTiles.Add(new Point(x, y));
                }
            }
        }
    }
}
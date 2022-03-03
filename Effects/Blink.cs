using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Sounds;

namespace Pandorai.Effects
{
    public class Blink : Effect
    {
        public override void Use(Creature usingCreature)
        {
            var maxX = Game1.game.Map.Tiles.GetLength(0);
            var maxY = Game1.game.Map.Tiles.GetLength(1);

            int randomX, randomY;
            do
            {
                randomX = Game1.game.mainRng.Next(0, maxX);
                randomY = Game1.game.mainRng.Next(0, maxY);
            }
            while(Game1.game.Map.GetTile(randomX, randomY).CollisionFlag);

            usingCreature.RequestMovement(new Point(randomX, randomY));
            SoundManager.PlaySound("teleport");
            DisplayMessage(usingCreature);
        }

        public override void SetAttribute(string name, string value)
        {
        }

        protected override string GetMessage()
        {
            return "You blink into a random location";
        }
    }
}
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Sounds;

namespace Pandorai.Effects
{
    public class Blink : Effect
    {
        public override void Use(Creature usingCreature)
        {
            var maxX = Main.Game.Map.Tiles.GetLength(0);
            var maxY = Main.Game.Map.Tiles.GetLength(1);

            int randomX, randomY;
            do
            {
                randomX = Main.Game.MainRng.Next(0, maxX);
                randomY = Main.Game.MainRng.Next(0, maxY);
            }
            while(Main.Game.Map.GetTile(randomX, randomY).CollisionFlag);

            usingCreature.RequestMovement(new Point(randomX, randomY));
            SoundManager.PlaySound("teleport", usingCreature.Position);
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
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Sounds;
using Pandorai.Tilemaps;
using Pandorai.UI;

namespace Pandorai.Effects
{
    public class Slingshot : Effect
    {
        public int Damage { get; set; }
        public int Range { get; set; }

        public override void Use(Creature user)
        {
			if (user.IsPossessedCreature() && Main.Game.Player.IsInteractingWithSomeone)
            {
                return;
            }

            if(!user.Inventory.ContainsItem("Stone"))
            {
                MessageLog.DisplayMessage(@"You don't have any ammunition to use the slingshot");
                return;
            }

            Rectangle effectRange = new Rectangle(user.MapIndex - new Point(Range), new Point(Range * 2 + 1));
			Main.Game.Map.HighlightTiles(effectRange);
			Main.Game.Map.EnableTileInteraction();

            TileInteractionManager.TileClick += useHandler;

            void useHandler(TileInfo tile)
            {
                if(!effectRange.Contains(tile.Index))
                {
                    Main.Game.Map.DisableTileInteraction();
                    TileInteractionManager.TileClick -= useHandler;
                }

                var tryCreature = Main.Game.CreatureManager.GetCreature(tile.Index);

                if(tryCreature != null)
                {
                    Main.Game.Map.DisableTileInteraction();
                    TileInteractionManager.TileClick -= useHandler;

                    user.Inventory.RemoveElement("Stone", 1);

                    SoundManager.PlaySound("spell", user.Position);
                    DisplayMessage(user);

                    tryCreature.GetHit(Damage, user);

                    if (user == Main.Game.Player.PossessedCreature)
                    {
                        Main.Game.TurnManager.PlayerIsReady();
                    }
                }
            }
        }

        public override void SetAttribute(string name, string value)
        {
            if(name == "Damage")
            {
                Damage = int.Parse(value);
            }
            else if(name == "Range")
            {
                Range = int.Parse(value);
            }
        }

        protected override string GetMessage()
        {
            return "You shot a stone using a slingshot";
        }
    }
}
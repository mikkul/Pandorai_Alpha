using System.Timers;
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.ParticleSystems;
using Pandorai.Sounds;
using Pandorai.Tilemaps;

namespace Pandorai.Effects
{
    public class TransformSpell : Effect
    {
        public int Range;

        public override void SetAttribute(string name, string value)
        {
            if(name == "Range")
            {
                Range = int.Parse(value);
            }
        }

        public override void Use(Creature user)
        {
			if (user == Main.Game.Player.PossessedCreature && Main.Game.Player.IsInteractingWithSomeone)
            {
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

                if(Main.Game.CreatureManager.GetCreature(tile.Index) != null)
                {
                    Main.Game.Player.IsInteractingWithSomeone = true;
                    Main.Game.Map.DisableTileInteraction();
                    TileInteractionManager.TileClick -= useHandler;

                    ParticleSystemManager.AddSystem(new PSExplosion(user.Position, 30, Main.Game.smokeParticleTexture, 1500, 150, 35, Color.Purple, true), true);

                    var effectTimer = new Timer(1000);
                    effectTimer.Elapsed += (s, a) =>
                    {
                        Main.Game.Player.PossessedCreature = Main.Game.CreatureManager.GetCreature(tile.Index);

                        if (user == Main.Game.Player.PossessedCreature)
                        {
                            Main.Game.TurnManager.PlayerIsReady();
                        }

                        ParticleSystemManager.AddSystem(new PSImplosion(tile.Index.ToVector2() * Main.Game.Options.TileSize, 50, Main.Game.smokeParticleTexture, 800, 200, 25, Color.Purple, true), true);

                        Main.Game.Player.IsInteractingWithSomeone = false;

                        effectTimer.Stop();
                        effectTimer.Dispose();
                    };
                    effectTimer.Start();

                    SoundManager.PlaySound("spell");
                    DisplayMessage(user);
                }
            }
        }

		protected override string GetMessage()
        {
            return "You transform into another creature";
        }
    }
}
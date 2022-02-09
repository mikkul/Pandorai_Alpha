using System.Timers;
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.ParticleSystems;
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
			if (user == user.game.Player.PossessedCreature && user.game.Player.IsInteractingWithSomeone)
            {
                return;
            }

			Rectangle effectRange = new Rectangle(user.MapIndex - new Point(Range), new Point(Range * 2 + 1));
			user.game.Map.HighlightTiles(effectRange);

			user.game.Map.EnableTileInteraction();

			TileInteractionManager.TileClick += useHandler;

            void useHandler(TileInfo tile)
            {
                if(!effectRange.Contains(tile.Index))
                {
                    user.game.Map.DisableTileInteraction();
                    TileInteractionManager.TileClick -= useHandler;
                }

                if(user.game.CreatureManager.GetCreature(tile.Index) != null)
                {
                    user.game.Player.IsInteractingWithSomeone = true;
                    user.game.Map.DisableTileInteraction();
                    TileInteractionManager.TileClick -= useHandler;

                    ParticleSystemManager.AddSystem(new PSExplosion(user.Position, 30, user.game.smokeParticleTexture, 1500, 150, 35, Color.Purple, true, user.game), true);

                    var effectTimer = new Timer(1000);
                    effectTimer.Elapsed += (s, a) =>
                    {
                        user.game.Player.PossessedCreature = user.game.CreatureManager.GetCreature(tile.Index);

                        if (user == user.game.Player.PossessedCreature)
                        {
                            user.game.TurnManager.PlayerIsReady();
                        }

                        ParticleSystemManager.AddSystem(new PSImplosion(tile.Index.ToVector2() * user.game.Options.TileSize, 50, user.game.smokeParticleTexture, 800, 200, 25, Color.Purple, true, user.game), true);

                        user.game.Player.IsInteractingWithSomeone = false;

                        effectTimer.Stop();
                        effectTimer.Dispose();
                    };
                    effectTimer.Start();
                }
            }
        }
    }
}
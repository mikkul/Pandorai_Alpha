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
			if (user == user.Game.Player.PossessedCreature && user.Game.Player.IsInteractingWithSomeone)
            {
                return;
            }

			Rectangle effectRange = new Rectangle(user.MapIndex - new Point(Range), new Point(Range * 2 + 1));
			user.Game.Map.HighlightTiles(effectRange);

			user.Game.Map.EnableTileInteraction();

			TileInteractionManager.TileClick += useHandler;

            void useHandler(TileInfo tile)
            {
                if(!effectRange.Contains(tile.Index))
                {
                    user.Game.Map.DisableTileInteraction();
                    TileInteractionManager.TileClick -= useHandler;
                }

                if(user.Game.CreatureManager.GetCreature(tile.Index) != null)
                {
                    user.Game.Player.IsInteractingWithSomeone = true;
                    user.Game.Map.DisableTileInteraction();
                    TileInteractionManager.TileClick -= useHandler;

                    ParticleSystemManager.AddSystem(new PSExplosion(user.Position, 30, user.Game.smokeParticleTexture, 1500, 150, 35, Color.Purple, true, user.Game), true);

                    var effectTimer = new Timer(1000);
                    effectTimer.Elapsed += (s, a) =>
                    {
                        user.Game.Player.PossessedCreature = user.Game.CreatureManager.GetCreature(tile.Index);

                        if (user == user.Game.Player.PossessedCreature)
                        {
                            user.Game.TurnManager.PlayerIsReady();
                        }

                        ParticleSystemManager.AddSystem(new PSImplosion(tile.Index.ToVector2() * user.Game.Options.TileSize, 50, user.Game.smokeParticleTexture, 800, 200, 25, Color.Purple, true, user.Game), true);

                        user.Game.Player.IsInteractingWithSomeone = false;

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
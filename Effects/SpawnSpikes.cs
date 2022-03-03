using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.ParticleSystems;
using Pandorai.Sounds;
using Pandorai.Tilemaps;
using Pandorai.Utility;

namespace Pandorai.Effects
{
    public class SpawnSpikes : Effect
    {
        public int Damage;

        public override void SetAttribute(string name, string value)
        {
			if(name == "Damage")
			{
				Damage = int.Parse(value);
			}
        }

        public override void Use(Creature user)
        {
            if (user.IsPossessedCreature() && Game1.game.Player.IsInteractingWithSomeone)
            {
                return;
            }

            var pos = user.MapIndex;
			var attackedTiles = GenHelper.GetNeighbours(pos);

			foreach (var tile in attackedTiles)
			{
                Game1.game.GameStateManager.AddSynchronizedAction(() => Game1.game.CreatureManager.GetCreature(tile)?.GetHit(Damage, user));
				var system = new PSExplosion(tile.ToVector2() * Game1.game.Map.TileSize, 50, Game1.game.squareTexture, 500, 25, 5, Color.SlateGray, true, Game1.game);
				ParticleSystemManager.AddSystem(system, true);
			}

            if (user.IsPossessedCreature())
            {
                Game1.game.TurnManager.PlayerIsReady();
            }

            if(user.MapIndex.IsInRangeOfPlayer())
            {
                SoundManager.PlaySound("Trap_01");
            }

            DisplayMessage(user);
        }

		protected override string GetMessage()
        {
            return "You spawn stone spikes around you";
        }
    }
}
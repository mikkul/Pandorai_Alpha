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
            if (user.IsPossessedCreature() && Main.Game.Player.IsInteractingWithSomeone)
            {
                return;
            }

            var pos = user.MapIndex;
			var attackedTiles = GenHelper.GetNeighbours(pos);

			foreach (var tile in attackedTiles)
			{
                Main.Game.GameStateManager.AddSynchronizedAction(() => Main.Game.CreatureManager.GetCreature(tile)?.GetHit(Damage, user));
				var system = new PSExplosion(tile.ToVector2() * Main.Game.Map.TileSize, 50, "SquareTexture", 500, 25, 5, Color.SlateGray, true);
				ParticleSystemManager.AddSystem(system, true);
			}

            if (user.IsPossessedCreature())
            {
                Main.Game.TurnManager.PlayerIsReady();
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
using Microsoft.Xna.Framework;

namespace Pandorai.Tilemaps
{
    public static class MapExtensions
    {
        public static bool IsInRangeOfPlayer(this Point point)
		{
            var distanceToPlayer = Vector2.Distance(point.ToVector2(), Game1.game.Player.PossessedCreature.MapIndex.ToVector2());
			return distanceToPlayer < 7;
		}
    }
}
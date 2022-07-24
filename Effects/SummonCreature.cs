using System;
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.MapGeneration;
using Pandorai.Sounds;

namespace Pandorai.Effects
{
    public class SummonCreature : Effect
    {
        public string CreatureName;

        public override void SetAttribute(string name, string value)
        {
			if(name == "CreatureName")
			{
				CreatureName = value;
			}
        }

        public override void Use(Creature usingCreature)
        {
            var neighbours = GenHelper.Get8Neighbours(usingCreature.MapIndex);
            Point location = Point.Zero;
            foreach (var neighbour in neighbours)
            {
                if(!Main.Game.Map.GetTile(neighbour).CollisionFlag)
                {
                    location = neighbour;
                }
            }
            if(location == Point.Zero)
            {
                DisplayMessage(usingCreature, "No space available to spawn");
                return;
            }

			Creature creatureInstance = CreatureLoader.GetCreature(CreatureName);
            // make it friendly to the summoner and hostile against everyone else
            creatureInstance.Class = usingCreature.Class;
            creatureInstance.EnemyClasses.Clear();
            creatureInstance.EnemyClasses.AddRange(Enum.GetValues<CreatureClass>());
            creatureInstance.EnemyClasses.Remove(usingCreature.Class);

            // set the level to 0 so it doesn't give any experience
            creatureInstance.Stats.Level = 0;

			creatureInstance.MapIndex = location;
			creatureInstance.Position = creatureInstance.MapIndex.ToVector2() * Main.Game.Options.TileSize;
			var tile = Main.Game.Map.Tiles[location.X, location.Y];
			tile.CollisionFlag = true;
			Main.Game.CreatureManager.AddCreature(creatureInstance);

            SoundManager.PlaySound("teleport", usingCreature.Position);
            DisplayMessage(usingCreature);
        }

        protected override string GetMessage()
        {
            return $"You summon a {CreatureName}";
        }
    }
}
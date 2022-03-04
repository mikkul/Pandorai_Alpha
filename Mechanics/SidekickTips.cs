using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Items;
using Pandorai.Tilemaps;
using Pandorai.UI;
using Pandorai.Utility;

namespace Pandorai.Mechanics
{
    public class SidekickTips
    {
        public int SearchRange { get; set; } = 10;

        private Creature _creature;

        private List<Point> _lastSearchArea = new ();
        
        public void Update()
        {
            _creature = Game1.game.Player.PossessedCreature;
            SearchArea();
            //SearchValuableItems();
        }

        private void SearchArea()
        {
            List<Point> area = GetArea();
            //DebugShowSearchedArea(area);
            SearchValuableItems(area);
            SearchMonsters(area);
            SearchTraps(area);
            _lastSearchArea = area;
        }

        private void SearchTraps(List<Point> area)
        {
            foreach (var point in area)
            {
                var tile = Game1.game.Map.GetTile(point);
                if(tile.Modifier.HasFlag(TileModifier.Trap))
                {
                    MessageLog.DisplayMessage(@"\c[#9160bf]I sense a trap somewhere...");
                }
            }
        }

        private void SearchMonsters(List<Point> area)
        {
            var dangerLevelDifference = 2;
            foreach (var point in area)
            {
                var creature = Game1.game.CreatureManager.GetCreature(point);
                if(creature == null)
                {
                    continue;
                }

                if(creature.Stats.Level - _creature.Stats.Level > dangerLevelDifference)
                {
                    MessageLog.DisplayMessage(@"\c[#9160bf]There's something dangerous around...");
                }
            }
        }

        private void SearchValuableItems(List<Point> area)
        {
            var valuableItemTypes = new[] { ItemType.Spell, ItemType.Food };
            foreach (var point in area)
            {
                var tile = Game1.game.Map.GetTile(point);
                if(!tile.HasItem())
                {
                    continue;
                }

                foreach (var valuableType in valuableItemTypes)
                {
                    if(tile.MapObject.Item.Type.HasFlag(valuableType))
                    {
                        MessageLog.DisplayMessage(@"\c[#9160bf]There's a valuable object somwhere there...");
                        break;
                    }
                }
            }
        }

        private void DebugShowSearchedArea(List<Point> area)
        {
            foreach (var point in _lastSearchArea)
            {
                var tile = Game1.game.Map.GetTile(point);
                tile.IsHighlighted = false;
            }
            foreach (var point in area)
            {
                var tile = Game1.game.Map.GetTile(point);
                tile.IsHighlighted = true;
                tile.HighlightColor = Helper.GetColorFromHex("#deb1f2") * 0.125f;
            }
        }

        private List<Point> GetArea()
        {
            List<Point> area = new ();

            for (int x = -SearchRange; x <= SearchRange; x++)
            {
                for (int y = -SearchRange; y <= SearchRange; y++)
                {
                    var point = new Point(_creature.MapIndex.X + x, _creature.MapIndex.Y + y);
                    if(Game1.game.Map.Tiles.IsPointInBounds(point))
                    {
                        area.Add(point);
                    }
                }
            }

            return area;
        }
    }
}
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Items;
using Pandorai.Structures.Behaviours;
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

        private int _turnCounter;
        private int _messageFrequency = 20;
        
        public void Update()
        {
            _creature = Main.Game.Player.PossessedCreature;
            _turnCounter++;
            SearchArea();
            if(_turnCounter >= _messageFrequency)
            {
                _turnCounter = 0;
            }
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
                var tile = Main.Game.Map.GetTile(point);
                if(tile.Modifier.HasFlag(TileModifier.Trap))
                {
                    var dist = Vector2.Distance(point.ToVector2(), _creature.MapIndex.ToVector2());
                    var chance = 1 - dist / (SearchRange + 1);
                    chance = MathHelper.Clamp(chance, 0.15f, 0.75f);
                    if(Main.Game.MainRng.NextFloat() < chance)
                    {
                        DisplayMessage(@"\c[#9160bf]I sense a trap somewhere", point);
                    }
                }
            }
        }

        private void SearchMonsters(List<Point> area)
        {
            var dangerLevelDifference = 2;
            foreach (var point in area)
            {
                var creature = Main.Game.CreatureManager.GetCreature(point);
                if(creature == null)
                {
                    continue;
                }

                if(creature.Stats.Level - _creature.Stats.Level > dangerLevelDifference)
                {
                    var dist = Vector2.Distance(point.ToVector2(), _creature.MapIndex.ToVector2());
                    var chance = 1 - dist / (SearchRange + 1);
                    chance = MathHelper.Clamp(chance, 0.15f, 0.75f);
                    if(Main.Game.MainRng.NextFloat() < chance)
                    {
                        DisplayMessage(@"\c[#9160bf]There's something dangerous", point);
                    }
                }
            }
        }

        private void SearchValuableItems(List<Point> area)
        {
            var valuableItemTypes = new[] { ItemType.Spell, ItemType.Food };
            foreach (var point in area)
            {
                var tile = Main.Game.Map.GetTile(point);

                Item itemFound = null;

                if(tile.HasItem())
                {
                    itemFound = tile.MapObject.Item;
                }
                else if(tile.HasStructure() && tile.MapObject.Structure.GetBehaviour<Container>() != null)
                {
                    var container = tile.MapObject.Structure.GetBehaviour<Container>();
                    itemFound = container.Inventory.Items[0].Item;
                }
                else
                {
                    continue;
                }

                foreach (var valuableType in valuableItemTypes)
                {
                    if(itemFound.Type.HasFlag(valuableType))
                    {
                        var dist = Vector2.Distance(point.ToVector2(), _creature.MapIndex.ToVector2());
                        var chance = 1 - dist / (SearchRange + 1);
                        chance = MathHelper.Clamp(chance, 0.15f, 0.75f);
                        if(Main.Game.MainRng.NextFloat() < chance)
                        {
                            DisplayMessage(@"\c[#9160bf]There's a valuable object somwhere", point);
                            break;
                        }
                    }
                }
            }
        }

        private void DisplayMessage(string message, Point point)
        {
            // add direction to the message
            message += " at ";
            //int threshold = 3;
            var horizontalDist = point.X - _creature.MapIndex.X;
            var verticalDist = point.Y - _creature.MapIndex.Y;
            if(verticalDist < 0)
            {
                message += "North";
            }
            else if(verticalDist > 0)
            {
                message += "South";
            }
            if(horizontalDist < 0)
            {
                message += "West";
            }
            else if(horizontalDist > 0)
            {
                message += "East";
            }
            // if(Math.Abs(Math.Abs(horizontalDist) - Math.Abs(verticalDist)) >= threshold)
            // {
            //     // show both directions
            //     if(verticalDist < 0)
            //     {
            //         message += "North";
            //     }
            //     else if(verticalDist > 0)
            //     {
            //         message += "South";
            //     }
            //     if(horizontalDist < 0)
            //     {
            //         message += "West";
            //     }
            //     else if(horizontalDist > 0)
            //     {
            //         message += "East";
            //     }
            // }
            // else
            // {
            //     // show only one direction
            //     if(verticalDist < 0)
            //     {
            //         message += "North";
            //     }
            //     else if(verticalDist > 0)
            //     {
            //         message += "South";
            //     }
            //     else if(horizontalDist < 0)
            //     {
            //         message += "West";
            //     }
            //     else if(horizontalDist > 0)
            //     {
            //         message += "East";
            //     }
            // }

            //
            if(_turnCounter >= _messageFrequency)
            {
                MessageLog.DisplayMessage(message);
            }
        }

        private void DebugShowSearchedArea(List<Point> area)
        {
            foreach (var point in _lastSearchArea)
            {
                var tile = Main.Game.Map.GetTile(point);
                tile.IsHighlighted = false;
            }
            foreach (var point in area)
            {
                var tile = Main.Game.Map.GetTile(point);
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
                    if(Main.Game.Map.Tiles.IsPointInBounds(point))
                    {
                        area.Add(point);
                    }
                }
            }

            return area;
        }
    }
}
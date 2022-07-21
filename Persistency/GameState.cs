using System.Collections.Generic;

namespace Pandorai.Persistency
{
    class GameState
    {
        public TileState[,] Tiles { get; set; }
        public List<CreatureState> Creatures { get; set; } = new();
        public List<StructureState> Structures { get; set; } = new();
        public List<ItemState> Items { get; set; } = new();
    }
}
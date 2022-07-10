using System.Collections.Generic;

namespace Pandorai.Persistency
{
    class GameState
    {
        public TileState[,] Tiles { get; set; }
        public List<CreatureState> Creatures { get; set; }
        public List<StructureState> Structures { get; set; }
        public List<ItemState> Items { get; set; }
    }
}
using System;

namespace Pandorai.Tilemaps
{
    [Flags]
    public enum TileModifier
    {
        None = 0,
        Sticky = 1 << 0,
        Web = 1 << 1,
        Trap = 1 << 2,
        Water = 1 << 3,
    }
}
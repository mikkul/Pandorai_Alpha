using System;

namespace Pandorai.Items
{
	[Flags]
	public enum ItemType
	{
		Spell = 1,
		Food = 2,
		Offensive = 4,
		Clothes = 8,
		Other = 16,
		Ranged = 32,
		Summon = 64,
        Projectile = 128,
	}
}

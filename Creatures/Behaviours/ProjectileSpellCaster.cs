using Pandorai.Items;
using Pandorai.Tilemaps;
using Pandorai.Utility;
using System;
using Microsoft.Xna.Framework;

namespace Pandorai.Creatures.Behaviours
{
	public class ProjectileSpellCaster : Behaviour
	{
		public override Behaviour Clone()
		{
			return new ProjectileSpellCaster {};
		}

		public override void SetAttribute(string name, string value)
		{
		}

		public override void Bind()
		{
			Owner.TurnCame += Work;
		}

		private void Work()
		{
			if (Owner.Target == Owner.MapIndex) 
			{
				return;
			}

			var diffX = Owner.Target.X - Owner.MapIndex.X;
			var diffY = Owner.Target.Y - Owner.MapIndex.Y;

			if (diffX != 0 && diffY != 0) 
			{
				return;
			}

			var spells = Owner.Inventory.Items.FindAll(x => x.Item.Type.HasFlag(ItemType.Spell) && x.Item.Type.HasFlag(ItemType.Projectile));
			if (spells.Count <= 0) return;

			var spellToUse = spells.GetRandomElement(Owner.Game.MainRng).Item;

			spellToUse.Use(Owner);
			TileInteractionManager.EmulateTileClick(Owner.MapIndex + new Point(Math.Sign(diffX), Math.Sign(diffY)));
		}
	}
}

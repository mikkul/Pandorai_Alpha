using Pandorai.Items;
using Pandorai.Tilemaps;
using Pandorai.Utility;
using System;
using Microsoft.Xna.Framework;

namespace Pandorai.Creatures.Behaviours
{
	public class SpellCaster : Behaviour
	{
		public ItemType Priority;

		public override Behaviour Clone()
		{
			return new SpellCaster
			{
				Priority = Priority,
			};
		}

		public override void SetAttribute(string name, string value)
		{
			if(name == "Priority")
			{
				Priority = (ItemType)Enum.Parse(typeof(ItemType), value);
			}
		}

		public override void Bind()
		{
			Owner.TurnCame += Work;
		}

		private void Work()
		{
			// primitive way of cheking whether the user is ready to cast a spell, same as in ParalelPosition
			// TODO: change it so that it checks for different conditions that would satisfy a given spell
			if (Owner.Target == Owner.MapIndex) return;

			var diffX = Owner.Target.X - Owner.MapIndex.X;
			var diffY = Owner.Target.Y - Owner.MapIndex.Y;

			if (diffX != 0 && diffY != 0) return;

			//
			var spells = Owner.Inventory.Items.FindAll(x => (x.Item.Type & ItemType.Spell) == ItemType.Spell);
			if (spells.Count <= 0) return;

			var prioritySpells = spells.FindAll(x => (x.Item.Type & Priority) == Priority);
			Item spellToUse = null;
			if (prioritySpells.Count > 0)
			{
				spellToUse = prioritySpells.GetRandomElement(Owner.Game.MainRng).Item;
			}
			else
			{
				spellToUse = spells.GetRandomElement(Owner.Game.MainRng).Item;
			}

			spellToUse.Use(Owner);
			TileInteractionManager.EmulateTileClick(Owner.MapIndex + new Point(Math.Sign(diffX), Math.Sign(diffY)));
		}
	}
}

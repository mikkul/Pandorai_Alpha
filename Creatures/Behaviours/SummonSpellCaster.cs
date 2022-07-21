using Pandorai.Items;
using Pandorai.Utility;

namespace Pandorai.Creatures.Behaviours
{
	public class SummonSpellCaster : Behaviour
	{
		public int Frequency;

		private int _turnCounter;

		public override Behaviour Clone()
		{
			return new SummonSpellCaster 
			{
				Frequency = Frequency,
				_turnCounter = Frequency,
			};
		}

		public override void SetAttribute(string name, string value)
		{
			if(name == "Frequency")
			{
				Frequency = int.Parse(value);
				_turnCounter = Frequency;
			}
		}

		public override void Bind()
		{
			Owner.TurnCame += Work;
		}

		private void Work()
		{
			if(_turnCounter < Frequency)
			{
				_turnCounter++;
				return;
			}

			if (Owner.Target == Owner.MapIndex) 
			{
				return;
			}

			_turnCounter = 0;

			var spells = Owner.Inventory.Items.FindAll(x => x.Item.Type.HasFlag(ItemType.Spell) && x.Item.Type.HasFlag(ItemType.Summon));
			if (spells.Count <= 0) 
            {
                return;
            }

			var spellToUse = spells.GetRandomElement(Main.Game.MainRng).Item;
			spellToUse.Use(Owner);
		}
	}
}

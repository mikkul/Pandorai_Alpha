using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Effects;
using Pandorai.Sounds;
using System.Collections.Generic;

namespace Pandorai.Items
{
	public class Item : Entity
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Texture { get; set; }
		public string SoundEffectName { get; set; }
		public bool Equipped { get; set; } = false;
		public bool Consumable { get; set; }
		public Color TooltipColor { get; set; } = Color.Gray;
		public Color ColorTint { get; set; } = Color.White;
		public ItemType Type { get; set; }
		public List<Effect> Effects { get; set; } = new List<Effect>();

		public void Use(Creature creature)
		{
			if(!string.IsNullOrEmpty(SoundEffectName))
			{
				SoundManager.PlaySound(SoundEffectName);
			}

			foreach (var effect in Effects)
			{
				effect.Use(creature);
			}
			if(Consumable)
			{
				creature.Inventory.RemoveElement(this);
			}
		}

		public Item Clone()
		{
			return new Item
			{
				Id = Id,
				Name = Name,
				Description = Description,
				Texture = Texture,
				SoundEffectName = SoundEffectName,
				Consumable = Consumable,
				TooltipColor = TooltipColor,
				ColorTint = ColorTint,
				Type = Type,
				Effects = new List<Effect>(Effects),
			};
		}
	}

	public enum ItemType
	{
		Spell = 1,
		Food = 2,
		Offensive = 4,
		Clothes = 8,
		Other = 16,
	}
}

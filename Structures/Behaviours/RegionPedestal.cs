using Pandorai.Creatures;
using Pandorai.Items;
using System;
using System.Diagnostics;

namespace Pandorai.Structures.Behaviours
{
	public class RegionPedestal : Behaviour
	{
		private bool activated = false;

		public override void Bind()
		{
			Structure.Interacted += Interact;
			Structure.Tile.Tile.ItemUsed += ItemInteract;
			Structure.DisableBehaviour("LightEmitter");
		}

		public override void Unbind()
		{
			Structure.Interacted -= Interact;
			Structure.Tile.Tile.ItemUsed -= ItemInteract;
		}

		public override Behaviour Clone()
		{
			return new RegionPedestal()
			{
				
			};
		}

		public override void ForceHandler(ForceType force)
		{
		}

		public override void Interact(Creature creature)
		{
		}

		public override void SetAttribute(string name, string value)
		{
		}

		public void ItemInteract(Item item)
		{
			if (activated) return;

			if(item.Id == "RegionGem")
			{
				Console.WriteLine("gem dropped onto pedestal");
				Game1.game.Player.PossessedCreature.Inventory.RemoveElement(item);
				Structure.EnableBehaviour("LightEmitter");
				activated = true;
			}
		}
	}
}

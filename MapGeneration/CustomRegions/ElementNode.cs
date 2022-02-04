using System.Collections.Generic;

namespace Pandorai.MapGeneration.CustomRegions
{
	public class ElementNode
	{
		public NodeType Type;
		public List<ElementNode> ChildNodes = new List<ElementNode>();
		public float Chance = 1.0f;
		public int Weight = 10;
		public ObjectSpecifier Object;
	}

	public enum NodeType
	{
		Choice,
		Group,
		Creature,
		Structure,
		Item,
		Trigger,
		Entrance,
		ElementModifier,
		InventoryItem,
		ProximitySpec,
		LayerSpec,
		StripeSpec,
	}
}

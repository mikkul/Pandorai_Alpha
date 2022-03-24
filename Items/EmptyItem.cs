using System;

namespace Pandorai.Items
{
	public class EmptyItem : Item
	{
		public EmptyItem()
		{
			var id = Guid.NewGuid();
			Id = $"EmptyItem{id}";
			Texture = 0;
		}
	}
}

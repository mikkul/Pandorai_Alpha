using System;

namespace Pandorai.Items
{
	public class EmptyItem : Item
	{
		public EmptyItem()
		{
			var id = Guid.NewGuid();
			TemplateName = $"EmptyItem";
			Id = $"EmptyItem{id}";
			Texture = 0;
		}
	}
}

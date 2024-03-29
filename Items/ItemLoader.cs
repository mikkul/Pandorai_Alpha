﻿using Microsoft.Xna.Framework;
using Pandorai.Effects;
using Pandorai.Utility;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Pandorai.Items
{
	public static class ItemLoader
	{
		private static Dictionary<string, Item> _itemTemplates = new Dictionary<string, Item>();

		public static Item GetItem(string name)
		{
			if(!_itemTemplates.ContainsKey(name))
			{
				return _itemTemplates["Error"].Clone();
			}
			else
			{
				return _itemTemplates[name].Clone();
			}
		}

		public static void LoadItems(string spreadsheetPath)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(spreadsheetPath);
			foreach (XmlElement node in doc.DocumentElement.ChildNodes)
			{
				Item item = new Item();
				item.TemplateName = node.GetAttribute("id");
				item.Texture = int.Parse(node.GetAttribute("texture"));
				item.Consumable = bool.Parse(node.GetAttribute("consumable"));
				if (node.HasAttribute("name"))
					item.Name = node.GetAttribute("name");
				if (node.HasAttribute("tooltipColor"))
					item.TooltipColor = colorLegend[node.GetAttribute("tooltipColor")];
				if (node.HasAttribute("colorTint"))
					item.ColorTint = Helper.GetColorFromHex(node.GetAttribute("colorTint"));
				if (node.HasAttribute("description"))
					item.Description = node.GetAttribute("description");
				if (node.HasAttribute("sound"))
					item.SoundEffectName = node.GetAttribute("sound");
				if (node.HasAttribute("requiredMana"))
					item.RequiredMana = int.Parse(node.GetAttribute("requiredMana"));			

				var types = Helper.GetStringChoice(node.GetAttribute("type"));
				foreach (var type in types)
				{
					item.Type |= (ItemType)Enum.Parse(typeof(ItemType), type);
				}

				foreach (XmlElement effect in node.ChildNodes)
				{
					Type effectType = TypeLegends.Effects[effect.GetAttribute("name")];
					Effect effectInstance = (Effect)Activator.CreateInstance(effectType);
					foreach (XmlElement modifier in effect.ChildNodes)
					{
						effectInstance.SetAttribute(modifier.Name, modifier.InnerText);
					}
					item.Effects.Add(effectInstance);
				}

				_itemTemplates.Add(item.TemplateName, item);
			}
		}

		private static Dictionary<string, Color> colorLegend = new Dictionary<string, Color>
		{
			{ "green", Color.Green },
			{ "orange", Color.Orange },
			{ "purple", Color.Purple },
			{ "red", Color.Red },
			{ "blue", Color.Blue },
			{ "white", Color.Wheat }
		};
	}
}

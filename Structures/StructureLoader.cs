using Pandorai.Structures.Behaviours;
using Pandorai.Utility;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Pandorai.Structures
{
	public static class StructureLoader
	{
		private static Dictionary<string, Structure> structureTemplates = new Dictionary<string, Structure>();

		public static Structure GetStructure(string name)
		{
			var clone = structureTemplates[name].Clone();
			return clone;
		}

		public static void LoadStructures(string spreadsheetPath)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(spreadsheetPath);
			foreach (XmlElement node in doc.DocumentElement.ChildNodes)
			{
				Structure structure = new Structure();
				structure.TemplateName = node.GetAttribute("id");
				structure.Texture = int.Parse(node.GetAttribute("texture"));
				if (node.HasAttribute("colorTint"))
					structure.ColorTint = Helper.GetColorFromHex(node.GetAttribute("colorTint"));

				foreach (XmlElement behaviour in node.ChildNodes)
				{
					Type effectType = TypeLegends.StructureBehaviours[behaviour.GetAttribute("name")];
					Behaviour behaviourInstance = (Behaviour)Activator.CreateInstance(effectType);
					foreach (XmlElement modifier in behaviour.ChildNodes)
					{
						behaviourInstance.SetAttribute(modifier.Name, modifier.InnerText);
					}
					structure.Behaviours.Add(behaviourInstance);
				}

				structureTemplates.Add(structure.TemplateName, structure);
			}
		}
	}
}

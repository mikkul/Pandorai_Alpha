using Pandorai.Triggers;
using Pandorai.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Pandorai.MapGeneration.CustomRegions
{
	public class CustomRegionLoader
	{
		public static List<CustomRegion> RegionTemplates = new List<CustomRegion>();

		public List<(CustomRegion template, int number)> Regions;

		public CustomRegionLoader(string pathToSpreadsheet)
		{
			Regions = new List<(CustomRegion template, int number)>();
			XmlDocument xml = new XmlDocument();
			xml.Load(pathToSpreadsheet);
			var root = xml.DocumentElement;
			foreach (XmlElement child in root)
			{
				var regName = child.GetAttribute("name");
				var regNumber = int.Parse(child.GetAttribute("number"));
				var reg = RegionTemplates.Find(r => r.Name == regName);
				Regions.Add((reg, regNumber));
			}
		}

		public static void LoadRegionTemplates(string folderLocation)
		{
			string[] files = Directory.GetFiles(folderLocation);
			foreach (var file in files)
			{
				RegionTemplates.Add(LoadRegionTemplate(file));
			}
		}

		private static CustomRegion LoadRegionTemplate(string path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			return ParseRegion(new CustomRegion(), doc.DocumentElement);
		}

		private static CustomRegion ParseRegion(CustomRegion region, XmlElement xml)
		{
			var root = xml;

			region.Name = root.GetAttribute("name");
			region.SampleName = root.GetAttribute("sample");

			if(root.HasAttribute("floorColor"))
			{
				region.FloorColor = Helper.GetColorFromHex(root.GetAttribute("floorColor"));
				region.FloorColor.A = 255;
			}

			if (root.HasAttribute("borderColor"))
			{
				region.BorderColor = Helper.GetColorFromHex(root.GetAttribute("borderColor"));
				region.BorderColor.A = 255;
			}

			foreach (XmlElement locSpec in root.SelectSingleNode("//customRegion/location").ChildNodes)
			{
				LocationSpecifier spec = null;

				if (locSpec.Name == "proximity")
				{
					spec = new ProximitySpec
					{
						ProximityTo = locSpec.GetAttribute("to"),
						Distance = ProximitySpec.DistanceKeys[locSpec.GetAttribute("distance")],
					};
				}

				if (spec != null)
				{
					region.LocationSpecifiers.Add(spec);
				}
			}

			int idCounter = 0;
			foreach (XmlElement dimSpec in root.SelectNodes("//customRegion/dimensions//*"))
			{
				DimensionSpecifier spec = null;

				if (dimSpec.Name == "rectangle")
				{
					var widthRange = Helper.GetNumberRangeFromString(dimSpec.GetAttribute("width"));
					var heightRange = Helper.GetNumberRangeFromString(dimSpec.GetAttribute("height"));

					spec = new RectangularArea
					{
						Width = widthRange,
						Height = heightRange,
					};
					RectangularArea rectSpec = (RectangularArea)spec;

					if (dimSpec.HasAttribute("rotationChance"))
					{
						rectSpec.RotationChance = float.Parse(dimSpec.GetAttribute("rotationChance")) / 100;
					}
					if(dimSpec.HasAttribute("fillInterior"))
					{
						rectSpec.FillInterior = false;
					}
				}

				if (dimSpec.HasAttribute("placement"))
				{
					var possiblePlacements = Helper.GetStringChoice(dimSpec.GetAttribute("placement"));
					foreach (var plac in possiblePlacements)
					{
						switch (plac)
						{
							case "left":
								spec.PossiblePlacements.Add(Placement.Left);
								break;
							case "right":
								spec.PossiblePlacements.Add(Placement.Right);
								break;
							case "top":
								spec.PossiblePlacements.Add(Placement.Top);
								break;
							case "bottom":
								spec.PossiblePlacements.Add(Placement.Bottom);
								break;
							case "center":
								spec.PossiblePlacements.Add(Placement.Center);
								break;
						}
					}
				}

				if(dimSpec.HasAttribute("offsetX"))
				{
					spec.OffsetX = Helper.GetNumberRangeFromString(dimSpec.GetAttribute("offsetX"));
				}
				if (dimSpec.HasAttribute("offsetY"))
				{
					spec.OffsetY = Helper.GetNumberRangeFromString(dimSpec.GetAttribute("offsetY"));
				}

				// creating unique ids for each node for linking purposes
				var id = idCounter++;
				spec.Id = id;
				dimSpec.SetAttribute("id", id.ToString());

				if (spec != null)
				{
					region.DimensionSpecifiers.Add(spec);
				}
			}

			// linking to parents
			foreach (XmlElement dimSpec in root.SelectNodes("//customRegion/dimensions//*"))
			{
				if(dimSpec.ParentNode.Name != "dimensions")
				{
					XmlElement parent = (XmlElement)dimSpec.ParentNode;
					var spec = region.DimensionSpecifiers.Find(i => i.Id == int.Parse(dimSpec.GetAttribute("id")));
					spec.Parent = region.DimensionSpecifiers.Find(i => i.Id == int.Parse(parent.GetAttribute("id")));
					spec.Parent.Children.Add(spec);
				}
			}

			XmlElement content = (XmlElement)root.SelectSingleNode("//customRegion/content");
			foreach (XmlElement contentEl in content.ChildNodes)
			{
				region.Content.Add(FillInObjects(contentEl));
			}

			return region;
		}

		private static ElementNode FillInObjects(XmlElement element, ElementNode parent = null, ElementNode currentlyModifiedElement = null)
		{
			ElementNode node = new ElementNode();

			switch (element.Name)
			{
				case "choice":
					node.Type = NodeType.Choice;
					break;
				case "group":
					node.Type = NodeType.Group;
					break;
				case "creature":
					node.Type = NodeType.Creature;
					node.Object = new EntitySpecifier();
					break;
				case "structure":
					node.Type = NodeType.Structure;
					node.Object = new EntitySpecifier();
					break;
				case "item":
					node.Type = NodeType.Item;
					node.Object = new EntitySpecifier();
					break;
				case "trigger":
					node.Type = NodeType.Trigger;
					node.Object = new TriggerSpecifier();
					break;
				case "entrance":
					node.Type = NodeType.Entrance;
					node.Object = new EntitySpecifier();
					break;
				// element modifiers
				case "location":
					node.Type = NodeType.ElementModifier;
					break;
				case "inventory":
					node.Type = NodeType.ElementModifier;
					break;
				case "invItem":
					node.Type = NodeType.InventoryItem;
					node.Object = new EntitySpecifier();
					break;
				case "proximity":
					node.Type = NodeType.ProximitySpec;
					break;
				case "layer":
					node.Type = NodeType.LayerSpec;
					break;
				case "stripe":
					node.Type = NodeType.StripeSpec;
					break;
			}

			if (element.HasAttribute("chance"))
			{
				node.Chance = float.Parse(element.GetAttribute("chance")) / 100;
			}
			if (element.HasAttribute("weight"))
			{
				node.Weight = int.Parse(element.GetAttribute("weight"));
			}

			if (node.Type == NodeType.Creature || node.Type == NodeType.Structure || node.Type == NodeType.Item || node.Type == NodeType.Entrance || node.Type == NodeType.InventoryItem)
			{
				var nodeType = (EntitySpecifier)node.Object;
				nodeType.Name = element.GetAttribute("name");

				if (element.HasAttribute("number"))
				{
					nodeType.Number = Helper.GetNumberRangeFromString(element.GetAttribute("number"));
				}
			}
			else if(node.Type == NodeType.Trigger)
			{
				var nodeType = (TriggerSpecifier)node.Object;
				nodeType.Name = element.GetAttribute("name");
				switch(nodeType.Name)
				{
					case "StoneGuardianAwake":
						nodeType.Handler = Trigger.StoneGuardianAwake;
						break;
					case "LeaveLibraryTrigger":
						nodeType.Handler = Trigger.LeaveLibraryTrigger;
						break;
				}
			}
			else if(node.Type == NodeType.ElementModifier)
			{
				currentlyModifiedElement = parent;
			}
			else if(node.Type == NodeType.ProximitySpec)
			{
				var spec = new ProximitySpec
				{
					ProximityTo = element.GetAttribute("to"),
					Distance = ProximitySpec.DistanceKeys[element.GetAttribute("distance")],
				};
				if(element.HasAttribute("tolerance"))
				{
					spec.Tolerance = int.Parse(element.GetAttribute("tolerance"));
				}

				node.Object = new Modifier
				{
					Parent = currentlyModifiedElement,
					Spec = spec,
				};
			}
			else if(node.Type == NodeType.LayerSpec)
			{
				var spec = new LayerSpec
				{
					Location = LayerSpec.LocationKeys[element.GetAttribute("where")],
				};

				if (element.HasAttribute("thickness"))
					spec.Thickness = int.Parse(element.GetAttribute("thickness"));
				if (element.HasAttribute("offset"))
					spec.Offset = int.Parse(element.GetAttribute("offset"));

				node.Object = new Modifier
				{
					Parent = currentlyModifiedElement,
					Spec = spec,
				};
			}
			else if(node.Type == NodeType.StripeSpec)
			{
				var spec = new StripeSpec
				{
					Direction = StripeSpec.DirectionKeys[element.GetAttribute("direction")],
				};

				if (element.HasAttribute("thickness"))
					spec.Thickness = int.Parse(element.GetAttribute("thickness"));
				if (element.HasAttribute("position"))
					spec.Position = int.Parse(element.GetAttribute("position")) / 100f;

				node.Object = new Modifier
				{
					Parent = currentlyModifiedElement,
					Spec = spec,
				};
			}

			if (element.HasChildNodes)
			{
				foreach (XmlElement child in element.ChildNodes)
				{
					node.ChildNodes.Add(FillInObjects(child, node, currentlyModifiedElement));
				}
			}

			return node;
		}
	}
}

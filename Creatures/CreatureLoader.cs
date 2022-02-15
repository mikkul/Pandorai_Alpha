using Pandorai.Creatures.Behaviours;
using Pandorai.Items;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Pandorai.Creatures
{
	public static class CreatureLoader
	{
		private static Dictionary<string, Creature> creatureTemplates = new Dictionary<string, Creature>();

		public static Creature GetCreature(string name)
		{
			var newCreature = creatureTemplates[name].Clone();
			return newCreature;
		}

		public static void LoadCreatures(string spreadsheetPath, Game1 game)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(spreadsheetPath);
			foreach (XmlElement node in doc.DocumentElement.ChildNodes)
			{
				Creature creature = new Creature(game);
				creature.Stats = new CreatureStats(creature);
				creature.Id = node.GetAttribute("id");
				creature.TextureIndex = int.Parse(node.GetAttribute("texture"));
				creature.Stats.MaxHealth = int.Parse(node.GetAttribute("maxHP"));
				creature.Stats.Health = int.Parse(node.GetAttribute("health"));
				creature.Stats.Strength = int.Parse(node.GetAttribute("strength"));
				creature.Stats.Speed = int.Parse(node.GetAttribute("speed"));
				creature.Stats.Level = int.Parse(node.GetAttribute("level"));
				creature.Class = (CreatureClass)Enum.Parse(typeof(CreatureClass), node.GetAttribute("class"));
				creature.CorpseTextureIndex = int.Parse(node.GetAttribute("corpseTexture"));
				if (node.HasAttribute("mana") && node.HasAttribute("maxMana"))
				{
					creature.Stats.Mana = int.Parse(node.GetAttribute("mana"));
					creature.Stats.MaxMana = int.Parse(node.GetAttribute("maxMana"));
				}
				else if(node.HasAttribute("maxMana"))
				{
					creature.Stats.MaxMana = int.Parse(node.GetAttribute("maxMana"));
				}
				if (node.HasAttribute("stealth"))
				{
					creature.Stats.Stealth = int.Parse(node.GetAttribute("stealth"));
				}
				if (node.HasAttribute("fireResistance"))
				{
					creature.Stats.FireResistance = int.Parse(node.GetAttribute("fireResistance"));
				}

				foreach (XmlElement modifier in node.SelectSingleNode("./modifiers").ChildNodes)
				{
					int counter = 0;
					foreach (XmlElement modValue in modifier.ChildNodes)
					{
						switch (modifier.Name)
						{
							case "EnemyClasses":
								creature.EnemyClasses.Add((CreatureClass)Enum.Parse(typeof(CreatureClass), modValue.InnerText));
								break;
							case "MovingTextureIndices":
								creature.MovingTextureIndices[counter++] = int.Parse(modValue.InnerText);
								break;
							case "IdleTextureIndices":
								creature.IdleTextureIndices[counter++] = int.Parse(modValue.InnerText);
								break;
							case "Inventory":
								creature.Inventory.AddElement(ItemLoader.GetItem(modValue.GetAttribute("name")), int.Parse(modValue.GetAttribute("number")));
								break;
							default:
								break;
						}
					}
				}

				foreach (XmlElement behaviour in node.SelectSingleNode("./behaviours").ChildNodes)
				{
					Type behaviourType = TypeLegends.CreatureBehaviours[behaviour.GetAttribute("name")];
					Behaviour behaviourInstance = (Behaviour)Activator.CreateInstance(behaviourType);
					foreach (XmlElement modifier in behaviour.ChildNodes)
					{
						behaviourInstance.SetAttribute(modifier.Name, modifier.InnerText);
					}
					creature.Behaviours.Add(behaviourInstance);
				}

				foreach (XmlElement sound in node.SelectSingleNode("./sounds").ChildNodes)
				{
					switch (sound.Name)
					{
						case "Attack":
							creature.Sounds.AttackSounds.Add(sound.InnerText);
							break;
						case "Hurt":
							creature.Sounds.HurtSounds.Add(sound.InnerText);
							break;
						case "Death":
							creature.Sounds.DeathSounds.Add(sound.InnerText);
							break;
						case "Aggro":
							creature.Sounds.AggroSounds.Add(sound.InnerText);
							break;
						case "Ambient":
							creature.Sounds.AmbientSounds.Add(sound.InnerText);
							break;
						case "Footstep":
							creature.Sounds.FootstepSounds.Add(sound.InnerText);
							break;							
						default:
							break;
					}
				}

				creatureTemplates.Add(creature.Id, creature);
			}
		}
	}
}
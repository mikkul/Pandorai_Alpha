using Newtonsoft.Json;
using Pandorai.Persistency.Converters;

namespace Pandorai.Creatures.Behaviours
{
	[JsonConverter(typeof(CreatureBehaviourConverter))]
	public abstract class Behaviour
	{
		public string TypeName => GetType().Name;

		protected Creature owner;
		[JsonIgnore]
		public Creature Owner { 
			get => owner; 
			set
			{
				owner = value;
				Bind();
			}
		}

		public abstract Behaviour Clone();

		public abstract void Bind();

		public abstract void SetAttribute(string name, string value);
	}
}

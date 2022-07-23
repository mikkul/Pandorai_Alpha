using Newtonsoft.Json;
using Pandorai.Creatures;
using Pandorai.Persistency.Converters;

namespace Pandorai.Structures.Behaviours
{
	[JsonConverter(typeof(StructureBehaviourConverter))]
	public abstract class Behaviour
	{
		public string TypeName => GetType().Name;

		[JsonIgnore]
		public Structure Structure { get; set; }

		public abstract void Bind();

		public abstract void Unbind();

		public abstract Behaviour Clone();

		public abstract void SetAttribute(string name, string value);

		public abstract void Interact(Creature creature);

		public abstract void ForceHandler(ForceType force);
	}
}

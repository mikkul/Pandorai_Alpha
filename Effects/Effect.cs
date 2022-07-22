using Newtonsoft.Json;
using Pandorai.Creatures;
using Pandorai.Persistency.Converters;
using Pandorai.UI;

namespace Pandorai.Effects
{
	[JsonConverter(typeof(EffectConverter))]
	public abstract class Effect
	{
		public string Id => GetType().Name;

		public abstract void Use(Creature usingCreature);

		public abstract void SetAttribute(string name, string value);

		protected virtual void DisplayMessage(Creature usingCreature)
		{
			if(usingCreature.IsPossessedCreature())
			{
				MessageLog.DisplayMessage(GetMessage());
			}
		}

		protected virtual void DisplayMessage(Creature usingCreature, string message)
		{
			if(usingCreature.IsPossessedCreature())
			{
				MessageLog.DisplayMessage(message);
			}
		}

		protected abstract string GetMessage();
	}
}

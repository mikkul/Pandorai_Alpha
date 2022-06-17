using Pandorai.Creatures;
using Pandorai.UI;

namespace Pandorai.Effects
{
	public abstract class Effect
	{
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

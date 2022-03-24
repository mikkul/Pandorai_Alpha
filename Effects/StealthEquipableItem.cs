using Pandorai.Creatures;
using Pandorai.Tilemaps;

namespace Pandorai.Effects
{
    public class StealthEquipableItem : Effect
    {
        public static float Opacity = 0.15f;

        public int Amount;

        private bool _equipped = false;

        public override void SetAttribute(string name, string value)
        {
            if(name == "Amount")
            {
                Amount = int.Parse(value);
            }
        }

        public override void Use(Creature user)
        {
            _equipped ^= true;
			user.Stats.Stealth += _equipped ? Amount : -Amount;

			if (_equipped)
            {
                user.Color *= Opacity;
            }
			else
            {
                user.Color *= 1 / Opacity;
            }

			CreatureIncomingHandler revertUse = null;
			revertUse = creature =>
			{
				_equipped = false;
				user.Stats.Stealth -= Amount;
				user.Color *= 1 / Opacity;
				user.GotHit -= revertUse;
                revertUse = null;
                DisplayMessage(user);
			};
            user.GotHit += revertUse;
            DisplayMessage(user);
        }

		protected override string GetMessage()
        {
            if(_equipped)
            {
                return "You equip the stealth item";
            }
            else
            {
                return "You dequip the stealth item";
            }
        }
    }
}
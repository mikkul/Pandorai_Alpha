using Pandorai.Creatures;
using Pandorai.Effects;
using Pandorai.Tilemaps;

namespace Pandorai.Effects
{
    public class StealthEquipableItem : Effect
    {
        public static float Opacity = 0.15f;

        public int Amount;

        private bool equiped = false;

        public override void SetAttribute(string name, string value)
        {
            if(name == "Amount")
            {
                Amount = int.Parse(value);
            }
        }

        public override void Use(Creature user)
        {
            equiped ^= true;
			user.Stealth += equiped ? Amount : -Amount;

			if (equiped)
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
				equiped = false;
				user.Stealth -= Amount;
				user.Color *= 1 / Opacity;
				user.GotHit -= revertUse;
			};
            user.GotHit += revertUse;
        }
    }
}
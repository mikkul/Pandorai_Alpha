using Pandorai.Creatures;

namespace Pandorai.Effects
{
    public class SpawnSpikes : Effect
    {
        public int Damage;

        public override void SetAttribute(string name, string value)
        {
			if(name == "Damage")
			{
				Damage = int.Parse(value);
			}
        }

        public override void Use(Creature usingCreature)
        {
            throw new System.NotImplementedException();
        }
    }
}
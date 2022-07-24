using Pandorai.Creatures;
using Pandorai.Sounds;
using Pandorai.Utility;

namespace Pandorai.Structures.Behaviours
{
    public class Campfire : Behaviour
    {
        public int Damage;

        private static Creature _campfireCreature = new Creature()
        {
            TemplateName = "Fire",
        };

        public override void Bind()
        {
            _campfireCreature.Stats = new CreatureStats(_campfireCreature);
            Structure.Tile.Tile.CollisionFlag = false;
            Structure.Tile.Tile.CreatureCame += Interact;
        }

        public override void Unbind()
        {
            Structure.Tile.Tile.CreatureCame -= Interact;
        }

        public override void SetAttribute(string name, string value)
        {
            if (name == "Damage")
            {
                Damage = int.Parse(value);
            }
        }

        public override Behaviour Clone()
        {
            return new Campfire()
            {
                Damage = Damage,
            };
        }

        public override void Interact(Creature creature)
        {
            creature.GetHit(Damage, _campfireCreature);
            SoundManager.PlaySound("fireball0", Structure.Tile.Index.IndexToWorldPosition(), 0.5f);
        }

        public override void ForceHandler(ForceType force)
        {
        }
    }
}

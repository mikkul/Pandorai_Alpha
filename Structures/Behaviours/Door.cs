using Pandorai.Creatures;

namespace Pandorai.Structures.Behaviours
{
    public class Door : Behaviour
    {
		public int ClosedTexture;
		public int OpenTexture;
        public string RequiredKey;
		public float OpacityMultiplier = 0.25f;
		public bool IsOpened = false;

		public override void SetAttribute(string name, string value)
		{
			if (name == "ClosedTexture")
			{
				ClosedTexture = int.Parse(value);
			}
			else if (name == "OpenTexture")
			{
				OpenTexture = int.Parse(value);
			}
            else if (name == "RequiredKey")
			{
				RequiredKey = value;
			}
		}

        public override Behaviour Clone()
        {
            return new Door
            {
                OpenTexture = OpenTexture,
                ClosedTexture = ClosedTexture,
                RequiredKey = RequiredKey,
            };
        }

		public override void Bind()
		{
			Structure.Interacted += Interact;
		}

		public override void Unbind()
		{
			Structure.Interacted -= Interact;
		}

        public override void Interact(Creature creature)
        {
            if(!IsOpened)
			{
				if (creature.Inventory.ContainsItem(RequiredKey))
				{
					creature.Inventory.RemoveElement(RequiredKey);
					Game1.game.Map.RequestTileCollisionFlagChange(Structure.Tile.Index, false);
					Structure.ColorTint *= OpacityMultiplier;
					IsOpened = true;
				}
				else
				{
					// play sound or something
				}
			}
			else
			{
				IsOpened = false;
				Structure.ColorTint *= 1 / OpacityMultiplier;
				Game1.game.Map.RequestTileCollisionFlagChange(Structure.Tile.Index, true);
			}
        }

        public override void ForceHandler(ForceType force)
        {
            
        }
    }
}
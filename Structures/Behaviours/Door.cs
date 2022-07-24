using Pandorai.Creatures;
using Pandorai.Sounds;
using Pandorai.Utility;

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
					Main.Game.Map.RequestTileCollisionFlagChange(Structure.Tile.Index, false);
					Structure.ColorTint *= OpacityMultiplier;
					IsOpened = true;
					SoundManager.PlaySound("door", Structure.Tile.Index.IndexToWorldPosition(), 0.5f);
				}
				else
				{
					SoundManager.PlaySound("interface6", Structure.Tile.Index.IndexToWorldPosition(), 0.5f);
				}
			}
			else
			{
				IsOpened = false;
				Structure.ColorTint *= 1 / OpacityMultiplier;
				Main.Game.Map.RequestTileCollisionFlagChange(Structure.Tile.Index, true);
				SoundManager.PlaySound("door_close", Structure.Tile.Index.IndexToWorldPosition(), 0.8f);
			}
        }

        public override void ForceHandler(ForceType force)
        {
            
        }
    }
}
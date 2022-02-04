using Pandorai.Creatures;

namespace Pandorai.Structures.Behaviours
{
    public class Door : Behaviour
    {
		public int ClosedTexture;
		public int OpenTexture;
        public string RequiredKey;
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
            throw new System.NotImplementedException();
        }

        public override void ForceHandler(ForceType force)
        {
            
        }
    }
}
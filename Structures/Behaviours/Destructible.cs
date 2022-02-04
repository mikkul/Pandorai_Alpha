using Pandorai.Creatures;
using System;

namespace Pandorai.Structures.Behaviours
{
	public class Destructible : Behaviour
	{
		public ForceType Force = ForceType.None;

		public override void SetAttribute(string name, string value)
		{
			if(name == "Force")
			{
				Force |= (ForceType)Enum.Parse(typeof(ForceType), value);
			}	
		}

		public override void Bind()
		{
			Structure.UsedForce += ForceHandler;
		}

		public override void Unbind()
		{
			Structure.UsedForce -= ForceHandler;
		}

		public override Behaviour Clone()
		{
			return new Destructible
			{
				Force = Force,
			};
		}

		public override void ForceHandler(ForceType force)
		{
			if((force & Force) == force)
			{
				Structure.Destroy();
			}
		}

		public override void Interact(Creature creature)
		{
			
		}
	}
}

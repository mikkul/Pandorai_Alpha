using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.ParticleSystems;
using Pandorai.Sounds;
using Pandorai.Utility;
using System.Collections.Generic;

namespace Pandorai.Structures.Behaviours
{
	public class Armor : Behaviour
	{
		public int Hits;

		private List<Behaviour> _behavioursHeld = new List<Behaviour>();

		public override void Bind()
		{
			Structure.UsedForce += ForceHandler;
			for(int i = Structure.Behaviours.Count - 1; i >= 0; i--)
			{
				var behaviour = Structure.Behaviours[i];

				if (behaviour.GetType() == this.GetType()) continue;

				_behavioursHeld.Add(behaviour);
				behaviour.Unbind();
			}
		}

		public override void Unbind()
		{
			Structure.UsedForce -= ForceHandler;
			for (int i = Structure.Behaviours.Count - 1; i >= 0; i--)
			{
				var behaviour = Structure.Behaviours[i];

				if (behaviour.GetType() == this.GetType()) continue;

				_behavioursHeld.Add(behaviour);
				behaviour.Bind();
			}
		}

		public override Behaviour Clone()
		{
			var clone = new Armor
			{
				Hits = Hits,
			};
			return clone;
		}

		public override void SetAttribute(string name, string value)
		{
			if(name == "Hits")
			{
				Hits = int.Parse(value);
			}
		}

		public override void ForceHandler(ForceType force)
		{
			int damage = 0;
			if((force & ForceType.Physical) == ForceType.Physical)
			{
				damage += 1;
				SoundManager.PlaySound("crack10", Structure.Tile.Index.IndexToWorldPosition(), 0.5f);
			}
			else if((force & ForceType.Fire) == ForceType.Fire)
			{
				damage += 3;
			}
			else
			{
				return;
			}

			ParticleSystemManager.AddSystem(new PSExplosion(Structure.Tile.Index.ToVector2() * Main.Game.Options.TileSize, 25, "SmokeParticleTexture", 350, 90, 30, Color.Gray, true), true);

			Hits -= damage;
			if (Hits <= 0)
			{
				SoundManager.PlaySound("impactwood22", Structure.Tile.Index.IndexToWorldPosition(), 0.25f);
				foreach (var behaviour in _behavioursHeld)
				{
					Structure.Interacted += behaviour.Interact;
					Structure.UsedForce += behaviour.ForceHandler;

					if(behaviour.GetType() == typeof(Container))
					{
						behaviour.Interact(new Creature());
					}
				}

				Structure.UsedForce -= ForceHandler;
			}
		}

		public override void Interact(Creature creature)
		{

		}
	}
}

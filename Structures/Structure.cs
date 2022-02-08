using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.ParticleSystems;
using Pandorai.Structures.Behaviours;
using Pandorai.Tilemaps;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pandorai.Structures
{
	public delegate void ForceUseHandler(ForceType force);

	public class Structure : Entity
	{
		public event CreatureIncomingHandler Interacted;
		public event ForceUseHandler UsedForce;

		public string Id { get; set; }

		public TileInfo Tile { get; set; }

		public int Texture { get; set; }

		public Color ColorTint { get; set; } = Color.White;

		public List<Behaviour> Behaviours = new List<Behaviour>();

		public ForceResult UsedForceResult = ForceResult.None;

		private Game1 game;

		public Structure(Game1 _game)
		{
			game = _game;
		}

		public Structure Clone()
		{
			var clone = new Structure(game)
			{
				Id = Id,
				Texture = Texture,
				ColorTint = ColorTint,
				Behaviours = new List<Behaviour>(Behaviours),
			};
			for (int i = 0; i < clone.Behaviours.Count; i++)
			{
				clone.Behaviours[i] = clone.Behaviours[i].Clone();
				clone.Behaviours[i].Structure = clone;
			}
			return clone;
		}

		public void BindBehaviours()
		{
			foreach (var behaviour in Behaviours)
			{
				behaviour.Bind();
			}
		}

		private Behaviour GetBehaviour(string name)
		{
			return Behaviours.Find(x => x.GetType().Name == name);
		}

		public void EnableBehaviour(string name)
		{
			GetBehaviour(name)?.Bind();
		}

		public void DisableBehaviour(string name)
		{
			GetBehaviour(name)?.Unbind();
		}

		public void Interact(Creature creature)
		{
			Interacted?.Invoke(creature);
		}

		public ForceResult UseForce(ForceType force)
		{
			UsedForce?.Invoke(force);

			return UsedForceResult;
		}

		public void Destroy()
		{
			Tile.Tile.MapObject = null;
			Tile.Tile.CollisionFlag = false;
			ParticleSystemManager.AddSystem(new PSExplosion(Tile.Index.ToVector2() * game.Options.TileSize, 25, game.smokeParticleTexture, 1000, 90, 30, Color.Gray, true, game), true);
		}
	}

	[Flags]
	public enum ForceType
	{
		None = 1,
		Fire = 2,
		Physical = 4,
		All = Fire | Physical,
	}

	public enum ForceResult
	{
		None,
		Damaged,
		Destroyed,
		Retaliated,
	}
}

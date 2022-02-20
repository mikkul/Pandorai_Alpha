﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Sprites;
using Pandorai.Tilemaps;
using Pandorai.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using Pandorai.Structures;
using Pandorai.Creatures.Behaviours;
using System.Linq;
using Pandorai.Sounds;

namespace Pandorai.Creatures
{
	public enum CreatureClass
	{
		Human,
		Monster,
		Neutral,
		Spirit,
		Temple
	}

	public delegate void MovementRequestHandler(Creature creature, Point desiredPoint);

	public class Creature : Entity
	{
		public event EmptyEventHandler Died;
		public event EmptyEventHandler TurnCame;
		public event EmptyEventHandler TurnEnded;
		public event CreatureIncomingHandler GotHit;
		public event CreatureIncomingHandler Interacted;

		public string Id;

		public Vector2 Position;
		public int TextureIndex;

		public int CorpseTextureIndex = 68;

		public bool IsAlive = true;

		public bool NoClip = false;

		public Color Color = new Color(1f, 1f, 1f, 1f);

		public bool ShowHPBar = true;

		public int Energy;

		public CreatureStats Stats;

		public CreatureSounds Sounds = new CreatureSounds();

		public Inventory Inventory;

		public ActiveMap LevelPresent;

		public CreatureClass Class;
		public List<CreatureClass> EnemyClasses = new List<CreatureClass>();

		public Point MapIndex;

		public bool IsMoving = false;

		public Game1 game;

		public Point Target;

		public List<Behaviour> Behaviours = new List<Behaviour>();

		public int[] MovingTextureIndices = new int[4];
		public int[] IdleTextureIndices = new int[4];

		public Vector2 StartPosition;
		public Vector2 TargetPosition;

		private byte damageFlashSpeed = 15;
		private Timer damageFlashTimer1;
		private Timer damageFlashTimer2;

		public Creature(Game1 _game)
		{
			game = _game;
			Inventory = new Inventory(this);

			Died += () =>
			{
				if(MapIndex.IsInRangeOfPlayer())
				{
					SoundManager.PlaySound(Sounds.Death);
				}
				var corpse = StructureLoader.GetStructure("Corpse");
				corpse.Tile = TileInfo.GetInfo(MapIndex, game);
				var container = corpse.GetBehaviour<Pandorai.Structures.Behaviours.Container>();
				container.Inventory.AddElements(Inventory.Items);
				game.Map.GetTile(MapIndex).MapObject = new MapObject(ObjectType.Interactive, CorpseTextureIndex)
				{
					Structure = corpse,
				};
				corpse.BindBehaviours();
			};
		}

		public Creature Clone()
		{
			var clone = new Creature(game)
			{
				Id = Id,
				TextureIndex = TextureIndex,
				CorpseTextureIndex = CorpseTextureIndex,
				MovingTextureIndices = MovingTextureIndices,
				IdleTextureIndices = IdleTextureIndices,
				Class = Class,
				EnemyClasses = EnemyClasses,
				Sounds = Sounds.Clone(),
				NoClip = NoClip,
			};
			clone.Stats = Stats.Clone(clone);
			clone.Behaviours = new List<Behaviour>(Behaviours);
			for (int i = 0; i < clone.Behaviours.Count; i++)
			{
				clone.Behaviours[i] = clone.Behaviours[i].Clone();
				clone.Behaviours[i].Owner = clone;
			}
			clone.Inventory.AddElements(Inventory.Items);
			return clone;
		}

		public Behaviour GetBehaviour(string name)
		{
			return Behaviours.Find(x => x.GetType().Name == name);
		}

		public Behaviour GetBehaviour<T>()
		{
			return Behaviours.Find(x => x is T);
		}

		public void RequestMovement(Point desiredPosition)
		{
			if(!IsMoving)
			{
				SetIdleTexture(desiredPosition);
			}
			game.CreatureManager.RequestCreatureMovement(this, desiredPosition);
		}

		public virtual bool ReadyForTurn()
		{
			Target = MapIndex;

			TurnCame?.Invoke();

			return Target != MapIndex;
		}

		public virtual void ReadyForTurn(Vector2 input)
		{
			var desiredPosition = Position + (input * game.Map.TileSize);

			Point targetTile = game.Map.GetTileIndexByPosition(desiredPosition);

			RequestMovement(targetTile);
		}

		public virtual void EndTurn()
		{
			Position = game.Map.SnapToGrid(Position);

			var newIndex = game.Map.GetTileIndexByPosition(Position);

			if(newIndex == MapIndex)
			{
				return;
			}

			MapIndex = newIndex;

			IsMoving = false;

			SetIdleTexture();

			TurnEnded?.Invoke();

			game.CreatureManager.FinishCreatureMovement(this);
		}

		public void OnDeath()
		{
			Died?.Invoke();
		}

		public void OnGotHit(Creature creature)
		{
			GotHit?.Invoke(creature);
		}

		public void Hit(Creature incomingCreature)
		{
			SoundManager.PlaySound(incomingCreature.Sounds.Attack);
			GotHit?.Invoke(incomingCreature);
		}

		public void Interact(Creature incomingCreature)
		{
			Interacted?.Invoke(incomingCreature);
		}

		public void Update()
		{
			if (game.TurnManager.TurnState == Mechanics.TurnState.EnemyTurn)
			{
				Vector2 lerpVector2 = Vector2.Lerp(StartPosition, TargetPosition, game.TurnManager.PercentageCompleted);
				Position = new Vector2(lerpVector2.X, lerpVector2.Y);
			}
		}

		public void UpdatePossessed()
		{
			if (game.TurnManager.TurnState == Mechanics.TurnState.PlayerTurn)
			{
				Vector2 lerpVector2 = Vector2.Lerp(StartPosition, TargetPosition, game.TurnManager.PercentageCompleted);
				Position = new Vector2(lerpVector2.X, lerpVector2.Y);
			}
		}

		public bool IsPossessedCreature()
		{
			return this == game.Player.PossessedCreature;
		}

		public void SetMovementTexture(Point targetPos)
		{
			Point movementDir = targetPos - MapIndex;

			if(movementDir.Y > 0) // faced down
			{
				TextureIndex = MovingTextureIndices[0];
			}
			else if (movementDir.Y < 0) // faced up
			{
				TextureIndex = MovingTextureIndices[1];
			}
			else if (movementDir.X < 0) // faced left
			{
				TextureIndex = MovingTextureIndices[2];
			}
			else if (movementDir.X > 0) // faced right
			{
				TextureIndex = MovingTextureIndices[3];
			}
		}

		public void SetIdleTexture()
		{
			if (TextureIndex == MovingTextureIndices[0])
			{
				TextureIndex = IdleTextureIndices[0];
			}
			else if (TextureIndex == MovingTextureIndices[1])
			{
				TextureIndex = IdleTextureIndices[1];
			}
			else if (TextureIndex == MovingTextureIndices[2])
			{
				TextureIndex = IdleTextureIndices[2];
			}
			else if (TextureIndex == MovingTextureIndices[3])
			{
				TextureIndex = IdleTextureIndices[3];
			}
		}

		public void SetIdleTexture(Point targetPos)
		{
			Point movementDir = targetPos - MapIndex;

			if (movementDir.Y > 0) // faced down
			{
				TextureIndex = IdleTextureIndices[0];
			}
			else if (movementDir.Y < 0) // faced up
			{
				TextureIndex = IdleTextureIndices[1];
			}
			else if (movementDir.X < 0) // faced left
			{
				TextureIndex = IdleTextureIndices[2];
			}
			else if (movementDir.X > 0) // faced right
			{
				TextureIndex = IdleTextureIndices[3];
			}
		}

		public void GetHit(float damage, Creature byWhom)
		{
			if(MapIndex.IsInRangeOfPlayer())
			{
				SoundManager.PlaySound(Sounds.Hurt);
			}
			Stats.Health -= (int)damage;
			DamageFlash();
			if (Stats.Health <= 0)
			{
				byWhom.Stats.Experience += CreatureStats.GetKillExperience(Stats.Level);
				Stats.Health = 0;
				IsAlive = false;
				OnDeath();
			}
		}

		public void Move(Point index)
		{
			MapIndex = index;
			Position = MapIndex.ToVector2() * game.Map.TileSize;
		}

		public void DamageFlash()
		{
			if ((damageFlashTimer1 != null && damageFlashTimer1.Enabled) || (damageFlashTimer2 != null && damageFlashTimer2.Enabled)) return;
			bool wasHPBarVisible = ShowHPBar;
			ShowHPBar = true;
			damageFlashTimer1 = new Timer(10);
			damageFlashTimer1.Elapsed += (o, e) =>
			{
				Color.G -= damageFlashSpeed;
				Color.B -= damageFlashSpeed;
				if(Color.G <= 128 + damageFlashSpeed)
				{
					damageFlashTimer1.Stop();
					damageFlashTimer1.Dispose();
					damageFlashTimer2 = new Timer(10);
					damageFlashTimer2.Elapsed += (o2, e2) =>
					{
						Color.G += damageFlashSpeed;
						Color.B += damageFlashSpeed;
						if(Color.G >= 255 - damageFlashSpeed)
						{
							Color.G = 255;
							Color.B = 255;
							ShowHPBar = wasHPBarVisible;
							damageFlashTimer2.Stop();
							damageFlashTimer2.Dispose();
						}
					};
					damageFlashTimer2.Enabled = true;
				}
			};
			damageFlashTimer1.Enabled = true;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			var hpBarRect = new Rectangle(game.Camera.GetViewportPosition(Position - new Vector2(game.Map.TileSize * 0.375f, game.Map.TileSize * 0.5f)).ToPoint(), new Point((int)((float)Stats.Health / (float)Stats.MaxHealth * game.Map.TileSize * 0.75f), game.Map.TileSize / 10));
			var destRect = new Rectangle(game.Camera.GetViewportPosition(Position - new Vector2(game.Map.TileSize / 2, game.Map.TileSize / 2)).ToPoint(), new Point(game.Map.TileSize));
			if(ShowHPBar)
			{
				spriteBatch.Draw(game.squareTexture, hpBarRect, Color.Red);
			}
			spriteBatch.Draw(TilesheetManager.CreatureSpritesheetTexture, destRect, TilesheetManager.CreatureSpritesheet[TextureIndex].Rect, this.Color);
			// if(Id == "Spider")
			// {
			// 	spriteBatch.Draw(TilesheetManager.MapSpritesheetTexture, destRect, TilesheetManager.MapObjectSpritesheet[0].Rect, Color.Purple * 0.6f);
			// }
		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Sprites;
using Pandorai.Tilemaps;
using Pandorai.Items;
using System.Collections.Generic;
using System.Timers;
using Pandorai.Structures;
using Pandorai.Creatures.Behaviours;
using Pandorai.Sounds;
using Pandorai.UI;
using Pandorai.ParticleSystems;
using System;

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

		public Main Game;

		public Point Target;

		public List<Behaviour> Behaviours = new List<Behaviour>();

		public int[] MovingTextureIndices = new int[4];
		public int[] IdleTextureIndices = new int[4];

		public Vector2 StartPosition;
		public Vector2 TargetPosition;

		private byte _damageFlashSpeed = 15;
		private Timer _damageFlashTimer1;
		private Timer _damageFlashTimer2;

		private bool _savedByGod = false;

		public Creature(Main game)
		{
			Game = game;
			Inventory = new Inventory(this);

			try
			{
				var stonesCount = Main.Game.MainRng.Next(0, 3);
				Inventory.RemoveElement("Stone", 9999);
				Inventory.AddElement(ItemLoader.GetItem("Stone"), stonesCount);
			}
			catch (Exception) { }

			Died += () =>
			{
				if(MapIndex.IsInRangeOfPlayer())
				{
					SoundManager.PlaySound(Sounds.Death);
				}
				var corpse = StructureLoader.GetStructure("Corpse");
				corpse.Tile = TileInfo.GetInfo(MapIndex, Game);
				var container = corpse.GetBehaviour<Pandorai.Structures.Behaviours.Container>();
				container.Inventory.AddElements(Inventory.Items);
				Game.Map.GetTile(MapIndex).MapObject = new MapObject(ObjectType.Interactive, CorpseTextureIndex)
				{
					Structure = corpse,
				};
				corpse.BindBehaviours();
			};
		}

		public Creature Clone()
		{
			var clone = new Creature(Game)
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
			clone.Inventory.RemoveElement("Stone", 9999);
			clone.Inventory.AddElements(Inventory.Items);
			return clone;
		}

		public Behaviour GetBehaviour(string name)
		{
			return Behaviours.Find(x => x.GetType().Name == name);
		}

		public T GetBehaviour<T>() where T : Behaviour
		{
			return Behaviours.Find(x => x is T) as T;
		}

		public void RequestMovement(Point desiredPosition)
		{
			if(!IsMoving)
			{
				SetIdleTexture(desiredPosition);
			}
			Game.CreatureManager.RequestCreatureMovement(this, desiredPosition);
		}

		public virtual bool ReadyForTurn()
		{
			Target = MapIndex;

			TurnCame?.Invoke();

			return Target != MapIndex;
		}

		public virtual void ReadyForTurn(Vector2 input)
		{
			var desiredPosition = Position + (input * Game.Map.TileSize);

			Point targetTile = Game.Map.GetTileIndexByPosition(desiredPosition);

			RequestMovement(targetTile);
		}

		public virtual void EndTurn()
		{
			Position = Game.Map.SnapToGrid(Position);

			var newIndex = Game.Map.GetTileIndexByPosition(Position);

			if(newIndex == MapIndex)
			{
				return;
			}

			MapIndex = newIndex;

			IsMoving = false;

			SetIdleTexture();

			TurnEnded?.Invoke();

			Game.CreatureManager.FinishCreatureMovement(this);
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
			if (Game.TurnManager.TurnState == Mechanics.TurnState.EnemyTurn)
			{
				Vector2 lerpVector2 = Vector2.Lerp(StartPosition, TargetPosition, Game.TurnManager.PercentageCompleted);
				Position = new Vector2(lerpVector2.X, lerpVector2.Y);
			}
		}

		public void UpdatePossessed()
		{
			if (Game.TurnManager.TurnState == Mechanics.TurnState.PlayerTurn)
			{
				Vector2 lerpVector2 = Vector2.Lerp(StartPosition, TargetPosition, Game.TurnManager.PercentageCompleted);
				Position = new Vector2(lerpVector2.X, lerpVector2.Y);
			}
		}

		public bool IsPossessedCreature()
		{
			return this == Game.Player.PossessedCreature;
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
			if(byWhom.IsPossessedCreature())
			{
				MessageLog.DisplayMessage($"You hit the {this.Id} for {damage} damage", Color.Green);
			}
			else if(this.IsPossessedCreature())
			{
				MessageLog.DisplayMessage($"You got hit by a {byWhom.Id} for {damage} damage", Color.Red);
			}

			if(MapIndex.IsInRangeOfPlayer())
			{
				SoundManager.PlaySound(Sounds.Hurt);
			}
			Stats.Health -= (int)damage;
			DamageFlash();
			if (Stats.Health <= 0)
			{
				if(this.IsPossessedCreature() && !_savedByGod)
				{
					_savedByGod = true;
					var chance = Main.Game.MainRng.NextDouble();
					if(chance < 0.5)
					{
						Stats.Health = 1;
						MessageLog.DisplayMessage("You have miraculously avoided death", Color.Pink);
						SoundManager.PlaySound("FX148");
						var particleSystemEffect = new PSImplosion(this.Position, 100, Main.Game.fireParticleTexture, 2000, Main.Game.Map.TileSize, 40, Color.Green, true, Main.Game);
						ParticleSystemManager.AddSystem(particleSystemEffect, true);
					}
					else
					{
						NormalHit(byWhom);
					}
				}
				else
                {
                    NormalHit(byWhom);
                }
            }

            void NormalHit(Creature byWhom)
            {
                if (byWhom.IsPossessedCreature())
                {
                    MessageLog.DisplayMessage($"You killed the {this.Id}", Color.DarkGreen);
                }
                else if (this.IsPossessedCreature())
                {
                    MessageLog.DisplayMessage($"You got killed by a {byWhom.Id}!", Color.DarkRed);
                }
                byWhom.Stats.Experience += CreatureStats.GetKillExperience(Stats.Level);
                Stats.Health = 0;
                IsAlive = false;
                OnDeath();
            }
        }

		public void Move(Point index)
		{
			MapIndex = index;
			Position = MapIndex.ToVector2() * Game.Map.TileSize;
		}

		public void DamageFlash()
		{
			if ((_damageFlashTimer1 != null && _damageFlashTimer1.Enabled) || (_damageFlashTimer2 != null && _damageFlashTimer2.Enabled)) return;
			bool wasHPBarVisible = ShowHPBar;
			ShowHPBar = true;
			_damageFlashTimer1 = new Timer(10);
			_damageFlashTimer1.Elapsed += (o, e) =>
			{
				Color.G -= _damageFlashSpeed;
				Color.B -= _damageFlashSpeed;
				if(Color.G <= 128 + _damageFlashSpeed)
				{
					_damageFlashTimer1.Stop();
					_damageFlashTimer1.Dispose();
					_damageFlashTimer2 = new Timer(10);
					_damageFlashTimer2.Elapsed += (o2, e2) =>
					{
						Color.G += _damageFlashSpeed;
						Color.B += _damageFlashSpeed;
						if(Color.G >= 255 - _damageFlashSpeed)
						{
							Color.G = 255;
							Color.B = 255;
							ShowHPBar = wasHPBarVisible;
							_damageFlashTimer2.Stop();
							_damageFlashTimer2.Dispose();
						}
					};
					_damageFlashTimer2.Enabled = true;
				}
			};
			_damageFlashTimer1.Enabled = true;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			var hpBarRect = new Rectangle(Game.Camera.GetViewportPosition(Position - new Vector2(Game.Map.TileSize * 0.375f, Game.Map.TileSize * 0.5f)).ToPoint(), new Point((int)((float)Stats.Health / (float)Stats.MaxHealth * Game.Map.TileSize * 0.75f), Game.Map.TileSize / 10));
			var destRect = new Rectangle(Game.Camera.GetViewportPosition(Position - new Vector2(Game.Map.TileSize / 2, Game.Map.TileSize / 2)).ToPoint(), new Point(Game.Map.TileSize));
			if(ShowHPBar)
			{
				spriteBatch.Draw(Game.squareTexture, hpBarRect, Color.Red);
			}
			spriteBatch.Draw(TilesheetManager.CreatureSpritesheetTexture, destRect, TilesheetManager.CreatureSpritesheet[TextureIndex].Rect, this.Color);
			// if(Id == "Spider")
			// {
			// 	spriteBatch.Draw(TilesheetManager.MapSpritesheetTexture, destRect, TilesheetManager.MapObjectSpritesheet[0].Rect, Color.Purple * 0.6f);
			// }
		}
	}
}

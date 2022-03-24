using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Tilemaps;
using Pandorai.Utility;
using System.Collections.Generic;

namespace Pandorai.Creatures
{
    public delegate void CreatureOnTileHandler(Creature creature, TileInfo info);

	public class CreatureManager
	{
		public event MovementRequestHandler CreatureRequestsMovement;
		public event CreatureOnTileHandler CreatureFinishedMovement;

		public List<Creature> Creatures = new List<Creature>();

		public List<Creature> UndergroundCreatures = new List<Creature>();
		public List<Creature> SurfaceCreatures = new List<Creature>();

		private Main _game;

		private RenderTarget2D _renderTarget;

		public CreatureManager(Main game)
		{
			_game = game;
            _renderTarget = new RenderTarget2D(this._game.GraphicsDevice, this._game.Camera.Viewport.Width, this._game.Camera.Viewport.Height);
		}

		public void RefreshRenderTarget()
		{
			_renderTarget = new RenderTarget2D(_game.GraphicsDevice, _game.Camera.Viewport.Width, _game.Camera.Viewport.Height);
		}

		public void CheckCreatureInteraction(Creature incomingCreature, TileInfo info)
		{
			var targetCreature = GetCreature(info.Index);
			if(targetCreature != null)
			{
				if(incomingCreature.EnemyClasses.Contains(targetCreature.Class))
				{
					targetCreature.Hit(incomingCreature);
				}
				else
				{
					targetCreature.Interact(incomingCreature);
				}

				if(incomingCreature == _game.Player.PossessedCreature)
				{
					_game.TurnManager.PlayerIsReady();
				}
			}
		}

		public void RequestCreatureMovement(Creature creature, Point desiredPoint)
		{
			CreatureRequestsMovement?.Invoke(creature, desiredPoint);
		}

		public void FinishCreatureMovement(Creature creature)
		{
			Point tileIndex = creature.MapIndex;
			CreatureFinishedMovement?.Invoke(creature, new TileInfo(tileIndex, _game.Map.Tiles[tileIndex.X, tileIndex.Y]));
			_game.Map.Tiles[tileIndex.X, tileIndex.Y].OnCreatureCame(creature);
		}

		public void AdjustPositionsToTileSize(int oldSize, int newSize)
		{
			Adjust(UndergroundCreatures);
			Adjust(SurfaceCreatures);

			void Adjust(List<Creature> list)
			{
				foreach (var creature in list)
				{
					creature.Position *= (float)newSize / (float)oldSize;
				}
			}
		}

		public void MapSwitchHandler(ActiveMap level)
		{
			if(level == ActiveMap.Underground)
			{
				Creatures = UndergroundCreatures;
			}
			else if(level == ActiveMap.Surface)
			{
				Creatures = SurfaceCreatures;
			}
		}

		public Creature GetCreature(Point index)
		{
			return Creatures.Find(item => item.MapIndex == index);
		}

		public void AddCreature(Creature creature)
		{
			Creatures.Add(creature);
		}

		public void RemoveCreature(Point index)
		{
			RemoveCreature(GetCreature(index));
		}

		public void RemoveCreature(Creature creature)
		{
			Creatures.Remove(creature);
			_game.Map.RequestTileCollisionFlagChange(creature.MapIndex, false);
		}

		public void UpdateCreatures()
		{
			for (int i = Creatures.Count - 1; i >= 0; i--)
			{
				if (!Creatures[i].IsAlive)
				{
					RemoveCreature(Creatures[i]);
					continue;
				}

				if(!Creatures[i].IsMoving)
				{
					continue;
				}

				if (Creatures[i] == _game.Player.PossessedCreature)
				{
					Creatures[i].UpdatePossessed();
				}
				else
				{
					Creatures[i].Update();
				}
			}
		}

		public RenderTarget2D DrawCreatures(SpriteBatch spriteBatch)
		{
			_game.GraphicsDevice.SetRenderTarget(_renderTarget);
			_game.GraphicsDevice.Clear(Color.Transparent);
			spriteBatch.Begin();
			for (int i = Creatures.Count - 1; i >= 0; i--)
			{
				if(_game.Player.TilesInFOV.Contains(Creatures[i].MapIndex))
				{
					Creatures[i].Draw(spriteBatch);
				}
			}
			spriteBatch.End();
			_game.GraphicsDevice.SetRenderTarget(null);
			return _renderTarget;
		}

		public void MakeCreaturesThink()
		{
			for (int i = Creatures.Count - 1; i >= 0; i--)
			{
				if(Creatures[i].IsPossessedCreature())
				{
					continue;
				}

				Creatures[i].Energy += Creatures[i].Stats.Speed;

				if(Creatures[i].Energy >= Main.Game.TurnManager.EnergyThreshold)
				{
					Creatures[i].Energy -= Main.Game.TurnManager.EnergyThreshold;
					Creatures[i].ReadyForTurn();
				}
			}

			_game.TurnManager.EnemyIsReady();
		}

		public void EndCreaturesTurn()
		{
			for (int i = Creatures.Count - 1; i >= 0; i--)
			{
				if (Creatures[i] == _game.Player.PossessedCreature) continue;

				Creatures[i].EndTurn();
			}
		}

		public void FirstLoadCreatures()
		{
			for (int i = Creatures.Count - 1; i >= 0; i--)
			{
				Creatures[i].EndTurn();
			}
		}
	}
}

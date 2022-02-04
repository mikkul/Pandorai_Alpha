using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pandorai.Mechanics;
using Pandorai.Tilemaps;
using Pandorai.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

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

		Game1 game;

		RenderTarget2D renderTarget;

		public CreatureManager(Game1 _game)
		{
			game = _game;
			renderTarget = new RenderTarget2D(game.GraphicsDevice, game.Camera.Viewport.Width, game.Camera.Viewport.Height);
		}

		public void RefreshRenderTarget()
		{
			renderTarget = new RenderTarget2D(game.GraphicsDevice, game.Camera.Viewport.Width, game.Camera.Viewport.Height);
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

				if(incomingCreature == game.Player.PossessedCreature)
				{
					game.TurnManager.PlayerIsReady();
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
			CreatureFinishedMovement?.Invoke(creature, new TileInfo(tileIndex, game.Map.Tiles[tileIndex.X, tileIndex.Y]));
			game.Map.Tiles[tileIndex.X, tileIndex.Y].OnCreatureCame(creature);
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
			game.Map.RequestTileIgnoreFlagChange(creature.MapIndex, false);
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

				if (Creatures[i] == game.Player.PossessedCreature)
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
			game.GraphicsDevice.SetRenderTarget(renderTarget);
			game.GraphicsDevice.Clear(Color.Transparent);
			spriteBatch.Begin();
			for (int i = Creatures.Count - 1; i >= 0; i--)
			{
				if(game.Player.TilesInFOV.Contains(Creatures[i].MapIndex))
					Creatures[i].Draw(spriteBatch);
			}
			spriteBatch.End();
			game.GraphicsDevice.SetRenderTarget(null);
			return renderTarget;
		}

		public void MakeCreaturesThink()
		{
			bool isSomeoneReady = false;
			for (int i = Creatures.Count - 1; i >= 0; i--)
			{
				bool inRange = game.Camera.Viewport.Enlarge(100, 100).Contains(game.Camera.GetViewportPosition(Creatures[i].Position));
				if (Creatures[i] != game.Player.PossessedCreature && inRange && Creatures[i].ReadyForTurn())
				{
					isSomeoneReady = true;
				}
			}

			if (isSomeoneReady)
			{
				game.TurnManager.EnemyIsReady();
			}
			else
			{
				game.TurnManager.SkipEnemyTurn();
			}
		}

		public void EndCreaturesTurn()
		{
			for (int i = Creatures.Count - 1; i >= 0; i--)
			{
				if (Creatures[i] == game.Player.PossessedCreature) continue;

				Creatures[i].EndTurn();
			}
		}
	}
}

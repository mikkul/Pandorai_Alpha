using System;
using Microsoft.Xna.Framework;
using Pandorai.Creatures;

namespace Pandorai.Mechanics
{
    public enum TurnState
	{
		WaitingForPlayer,
		PlayerTurn,
		WaitingForEnemy,
		EnemyTurn,
		OnHold
	}

	public class TurnManager
	{
		public event EmptyEventHandler PlayerTurnCame;
		public event EmptyEventHandler PlayerActionStarted;
		public event EmptyEventHandler PlayerTurnEnded;
		public event EmptyEventHandler EnemyTurnCame;
		public event EmptyEventHandler EnemyTurnEnded;
		public event MovementRequestHandler SomeoneIsReadyToMove;

		public float PercentageCompleted;

		public int TurnCount = 0;

		public int heroTurnTime = 100;
		public int enemyTurnTime = 50;

		public int EnergyThreshold = 100;

		float timeSinceTurnStart;

		Game1 game;

		TurnState previousState = TurnState.OnHold;
		public TurnState TurnState = TurnState.WaitingForPlayer;

		public TurnManager(Game1 _game)
		{
			game = _game;
		}

		public void Update(float dt)
		{
			if(TurnState == TurnState.WaitingForPlayer)
			{
				if(TurnState != previousState)
				{
					previousState = TurnState;
					PlayerTurnCame?.Invoke();
					CheckPlayerEnergy();
				}
			}
			
			if(TurnState == TurnState.PlayerTurn)
			{
				if(TurnState != previousState)
				{
					previousState = TurnState;
					PercentageCompleted = 0;
					timeSinceTurnStart = 0;
					PlayerActionStarted?.Invoke();
				}

				timeSinceTurnStart += dt * 1000;
				PercentageCompleted = timeSinceTurnStart / heroTurnTime;

				if (PercentageCompleted >= 1.0f)
				{
					TurnState = TurnState.WaitingForEnemy;
					PlayerTurnEnded?.Invoke();
				}
			}

			if(TurnState == TurnState.WaitingForEnemy)
			{
				if(TurnState != previousState)
				{
					previousState = TurnState;
					EnemyTurnCame?.Invoke();
					timeSinceTurnStart = 0;
					PercentageCompleted = 0;
				}
			}

			if(TurnState == TurnState.EnemyTurn)
			{
				timeSinceTurnStart += dt * 1000;
				PercentageCompleted = timeSinceTurnStart / enemyTurnTime;

				if (PercentageCompleted >= 1.0f)
				{
					TurnState = TurnState.WaitingForPlayer;
					EnemyTurnEnded?.Invoke();
					TurnCount++;
					game.BasicTrivia.DisplayRandomTrivia(game);
				}
			}
		}

        private void CheckPlayerEnergy()
        {
			var possessedCreature = Game1.game.Player.PossessedCreature;
			if(possessedCreature == null)
			{
				return;
			}
			possessedCreature.Energy += possessedCreature.Stats.Speed;
			if(possessedCreature.Energy >= EnergyThreshold)
			{
				possessedCreature.Energy -= EnergyThreshold;
			}
			else
			{
				TurnState = TurnState.WaitingForEnemy;
			}
        }

        public void HandleCreatureMovementRequest(Creature creature, Point desiredPoint)
		{
			if(creature == game.Player.PossessedCreature)
			{
				if(TurnState == TurnState.WaitingForPlayer)
				{
					SomeoneIsReadyToMove?.Invoke(creature, desiredPoint);
				}
			}
			else if(TurnState == TurnState.WaitingForEnemy)
			{
				SomeoneIsReadyToMove?.Invoke(creature, desiredPoint);
			}
		}

		public void PlayerIsReady()
		{
			if(TurnState == TurnState.WaitingForPlayer)
			{
				TurnState = TurnState.PlayerTurn;
			}
		}

		public void EnemyIsReady()
		{
			if(TurnState == TurnState.WaitingForEnemy)
			{
				TurnState = TurnState.EnemyTurn;
			}
		}

		public void SkipHeroTurn()
		{
			TurnState = TurnState.WaitingForEnemy;
		}

		public void SkipEnemyTurn()
		{
			TurnState = TurnState.WaitingForPlayer;
			previousState = TurnState.EnemyTurn;
			TurnCount++;
			game.BasicTrivia.DisplayRandomTrivia(game);
		}
	}
}

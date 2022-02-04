using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using Pandorai.Cheats;
using Pandorai.Creatures;
using Pandorai.Tilemaps;
using System;
using System.Diagnostics;

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

		public int heroTurnTime = 150;
		public int enemyTurnTime = 120;

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
					PlayerTurnCame?.Invoke();
				}
			}

			if(TurnState == TurnState.PlayerTurn)
			{
				if(TurnState != previousState)
				{
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

			previousState = TurnState;
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
			TurnCount++;
			game.BasicTrivia.DisplayRandomTrivia(game);
		}
	}
}

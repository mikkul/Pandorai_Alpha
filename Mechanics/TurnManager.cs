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

		public bool PlayerCanMove;

		public float PercentageCompleted;

		public int TurnCount = 0;

		public int DayDurationTurns = 750;

		public float DayNightValue = 0.5f;

		public int heroTurnTime = 60;
		public int enemyTurnTime = 20;

		public int EnergyThreshold = 100;

		private float _timeSinceTurnStart;

		private TurnState _previousState = TurnState.OnHold;
        private TurnState _turnState = TurnState.WaitingForPlayer;

        public TurnManager()
		{
		}

        public TurnState TurnState 
		{ 
			get => _turnState; 
			set 
			{
				//Console.WriteLine($"{turnState} -> {value}");
				_turnState = value;
		 	} 
		}

		public void Update(float dt)
		{
			if(TurnState == TurnState.WaitingForPlayer)
			{
				if(TurnState != _previousState)
				{
					_previousState = TurnState;
					PlayerTurnCame?.Invoke();
					CheckPlayerEnergy();
				}
			}
			
			if(TurnState == TurnState.PlayerTurn)
			{
				if(TurnState != _previousState)
				{
					_previousState = TurnState;
					PercentageCompleted = 0;
					_timeSinceTurnStart = 0;
					PlayerActionStarted?.Invoke();
				}

				_timeSinceTurnStart += dt * 1000;
				PercentageCompleted = _timeSinceTurnStart / heroTurnTime;

				if (PercentageCompleted >= 1.0f)
				{
					TurnState = TurnState.WaitingForEnemy;
					PlayerCanMove = false;
					PlayerTurnEnded?.Invoke();
				}

				if((_timeSinceTurnStart + dt * 1000) / heroTurnTime >= 1.0f)
				{
					PercentageCompleted = 1.0f;
				}
			}

			if(TurnState == TurnState.WaitingForEnemy)
			{
				if(TurnState != _previousState)
				{
					_previousState = TurnState;
					EnemyTurnCame?.Invoke();
					_timeSinceTurnStart = 0;
					PercentageCompleted = 0;
				}
			}

			if(TurnState == TurnState.EnemyTurn)
			{
				_timeSinceTurnStart += dt * 1000;
				PercentageCompleted = _timeSinceTurnStart / enemyTurnTime;

				if (PercentageCompleted >= 1.0f)
				{
					TurnState = TurnState.WaitingForPlayer;
					EnemyTurnEnded?.Invoke();
					TurnCount++;
					Main.Game.BasicTrivia.DisplayRandomTrivia(Main.Game);

					// day-night cycle
					DayNightValue += 1f / DayDurationTurns;
					if(DayNightValue >= 1f)
					{
						DayNightValue = 0f;
					}
					Rendering.LightingManager.LightingMaskEffect.Parameters["timeOfDay"].SetValue(DayNightValue);
				}
			}
		}

        private void CheckPlayerEnergy()
        {
			var possessedCreature = Main.Game.Player.PossessedCreature;
			if(possessedCreature == null)
			{
				return;
			}
			possessedCreature.Energy += possessedCreature.Stats.Speed;
			if(possessedCreature.Energy >= EnergyThreshold)
			{
				possessedCreature.Energy -= EnergyThreshold;
				PlayerCanMove = true;
			}
			else
			{
				TurnState = TurnState.WaitingForEnemy;
				PlayerCanMove = false;
			}
        }

        public void HandleCreatureMovementRequest(Creature creature, Point desiredPoint)
		{
			if(creature == Main.Game.Player.PossessedCreature)
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
			_previousState = TurnState.EnemyTurn;
			TurnCount++;
			Main.Game.BasicTrivia.DisplayRandomTrivia(Main.Game);
		}
	}
}

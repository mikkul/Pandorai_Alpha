using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Pandorai.Cheats;
using System;
using System.Collections.Generic;

namespace Pandorai.Mechanics
{
	public class GameStateManager
	{
		public event MouseHandler MouseOverViewport;
		public event MouseHandler LMBClickInViewport;
		public event MouseHandler RMBClickInViewport;

		private Queue<Action> _synchronizedActionsQueue = new Queue<Action>();

		private Main _game;

		public GameStateManager(Main game)
		{
			_game = game;
		}

		public void AddSynchronizedAction(Action action)
		{
			lock(_synchronizedActionsQueue)
				_synchronizedActionsQueue.Enqueue(action);
		}

		public void ExecuteSynchronizedActions()
		{
			lock (_synchronizedActionsQueue)
			{
				while (_synchronizedActionsQueue.Count > 0)
				{
					_synchronizedActionsQueue.Dequeue()?.Invoke();
				}
			};
		}

		public void CheckIfMouseOverViewport(Vector2 pos)
		{
			if(_game.Camera.IsInViewport(pos))
			{
				MouseOverViewport?.Invoke(pos);
			}
		}

		public void CheckIfLMBClickInViewport(Vector2 pos)
		{
			if (_game.Camera.IsInViewport(pos))
			{
				LMBClickInViewport?.Invoke(pos);
			}
		}

		public void CheckIfRMBClickInViewport(Vector2 pos)
		{
			if (_game.Camera.IsInViewport(pos))
			{
				RMBClickInViewport?.Invoke(pos);
			}
		}

		public void HandleInput(Keys key)
		{
			if(key == Keys.Escape)
			{
				PauseHandler();
				DisableMapInteractionHandler();
			}

			if(key == Keys.Space)
			{
				PausePlayerMovementHandler();
			}
		}

		public void PausePlayerMovementHandler()
		{
			if (!CheatConsole.IsActive && !_game.Player.IsInteractingWithSomeone)
			{
				if (_game.Player._isMovingByMouse)
				{
					_game.Player._isMovingByMouse = false;
				}
				else if(_game.TurnManager.PercentageCompleted <= 0 || _game.TurnManager.PercentageCompleted >= 1)
				{
					_game.TurnManager.SkipHeroTurn();
				}
			}
		}

		public void DisableMapInteractionHandler()
		{
			if(_game.Map.AreTilesInteractive)
			{
				_game.Map.DisableTileInteraction();
			}
		}

		public void PauseHandler()
		{
			if(!_game.Map.AreTilesInteractive && !CheatConsole.IsActive && !_game.Player.IsInteractingWithSomeone && _game.IsGameStarted)
			{
				_game.TogglePauseGame();
			}
		}
	}
}

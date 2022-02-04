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

		private Queue<Action> synchronizedActionsQueue = new Queue<Action>();

		Game1 game;

		public GameStateManager(Game1 _game)
		{
			game = _game;
		}

		public void AddSynchronizedAction(Action action)
		{
			lock(synchronizedActionsQueue)
				synchronizedActionsQueue.Enqueue(action);
		}

		public void ExecuteSynchronizedActions()
		{
			lock (synchronizedActionsQueue)
			{
				while (synchronizedActionsQueue.Count > 0)
				{
					synchronizedActionsQueue.Dequeue()?.Invoke();
				}
			};
		}

		public void CheckIfMouseOverViewport(Vector2 pos)
		{
			if(game.Camera.IsInViewport(pos))
			{
				MouseOverViewport?.Invoke(pos);
			}
		}

		public void CheckIfLMBClickInViewport(Vector2 pos)
		{
			if (game.Camera.IsInViewport(pos))
			{
				LMBClickInViewport?.Invoke(pos);
			}
		}

		public void CheckIfRMBClickInViewport(Vector2 pos)
		{
			if (game.Camera.IsInViewport(pos))
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
			if (!CheatConsole.IsActive && !game.Player.IsInteractingWithSomeone)
			{
				if (game.Player.isMovingByMouse)
				{
					game.Player.isMovingByMouse = false;
				}
				else if(game.TurnManager.PercentageCompleted <= 0 || game.TurnManager.PercentageCompleted >= 1)
				{
					game.TurnManager.SkipHeroTurn();
				}
			}
		}

		public void DisableMapInteractionHandler()
		{
			if(game.Map.AreTilesInteractive)
			{
				game.Map.DisableTileInteraction();
			}
		}

		public void PauseHandler()
		{
			if(!game.Map.AreTilesInteractive && !CheatConsole.IsActive && !game.Player.IsInteractingWithSomeone && game.IsGameStarted)
			{
				game.TogglePauseGame();
			}
		}
	}
}

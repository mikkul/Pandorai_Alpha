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


		public GameStateManager()
		{
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
			if(Main.Game.Camera.IsInViewport(pos))
			{
				MouseOverViewport?.Invoke(pos);
			}
		}

		public void CheckIfLMBClickInViewport(Vector2 pos)
		{
			if (Main.Game.Camera.IsInViewport(pos))
			{
				LMBClickInViewport?.Invoke(pos);
			}
		}

		public void CheckIfRMBClickInViewport(Vector2 pos)
		{
			if (Main.Game.Camera.IsInViewport(pos))
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
			if (!CheatConsole.IsActive && !Main.Game.Player.IsInteractingWithSomeone)
			{
				if (Main.Game.Player._isMovingByMouse)
				{
					Main.Game.Player._isMovingByMouse = false;
				}
				else if(Main.Game.TurnManager.PercentageCompleted <= 0 || Main.Game.TurnManager.PercentageCompleted >= 1)
				{
					Main.Game.TurnManager.SkipHeroTurn();
				}
			}
		}

		public void DisableMapInteractionHandler()
		{
			if(Main.Game.Map.AreTilesInteractive)
			{
				Main.Game.Map.DisableTileInteraction();
			}
		}

		public void PauseHandler()
		{
			if(!Main.Game.Map.AreTilesInteractive && !CheatConsole.IsActive && !Main.Game.Player.IsInteractingWithSomeone && Main.Game.IsGameStarted)
			{
				Main.Game.TogglePauseGame();
			}
		}
	}
}

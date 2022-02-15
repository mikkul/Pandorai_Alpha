using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Pandorai.Creatures;
using Pandorai.Creatures.Behaviours;
using System;
using Pandorai.Cheats;
using Pandorai.Tilemaps;
using Pandorai.AStarSearchAlgorithm;
using System.Diagnostics;
using System.Collections.Generic;
using Pandorai.Utility;
using Pandorai.UI;

namespace Pandorai.Mechanics
{
	public class PlayerController
	{
		private Creature possessedCreature;

		public Creature PossessedCreature
		{
			get
			{
				return possessedCreature;
			}
			set
			{
				if(possessedCreature != null)
				{
					possessedCreature.Died -= SpawnGhost;
				}

				possessedCreature = value;
				possessedCreature.Died += SpawnGhost;
				possessedCreature.Inventory.DisplayAsMainInventory();
			}
		}

		public Vector2 movementInput;

		public bool IsInteractingWithSomeone = false;

		public bool IsDead = false;

		public bool HoldingShift = false;
		public bool HoldingControl = false;

		public List<Point> TilesInFOV = new List<Point>();

		public bool isMovingByMouse = false;
		private List<Point> mousePath = new List<Point>();

		Game1 game;

		public PlayerController(Game1 _game)
		{
			game = _game;
		}

		public void Update()
		{
			if(game.InputManager.IsHoldingKey(Keys.LeftShift))
			{
				HoldingShift = true;
			}
			else
			{
				HoldingShift = false;
			}

			if (game.InputManager.IsHoldingKey(Keys.LeftControl))
			{
				HoldingControl = true;
			}
			else
			{
				HoldingControl = false;
			}

			if(isMovingByMouse && !CheatConsole.IsActive && !IsInteractingWithSomeone && !HoldingControl)
			{
				if(mousePath.Count > 0)
				{
					movementInput = (mousePath[0] - PossessedCreature.MapIndex).ToVector2();

					//game.TurnManager.PlayerIsReady();
					StartTurn();
				}
			}
			else
			{
				var input = game.InputManager.GetPlayerInput();

				if ((input.Movement != Vector2.Zero || input.Action != InputAction.None) && !CheatConsole.IsActive && !IsInteractingWithSomeone && !PossessedCreature.IsMoving)
				{
					movementInput = input.Movement;
					//game.TurnManager.PlayerIsReady();
					StartTurn();
				}
			}
		}

		public void MoveByMouse(TileInfo tileInfo)
		{
			if (CheatConsole.IsActive || game.Player.IsInteractingWithSomeone || game.Map.AreTilesInteractive || HoldingControl || HoldingShift || PossessedCreature.IsMoving) return;

			mousePath = AStarCreatures.GetShortestPath(game.Map.Tiles, PossessedCreature.MapIndex, tileInfo.Index, false, true, true);

			if(mousePath.Count > 0)
			{
				/*var lastPointInPath = mousePath[mousePath.Count - 1];
				if(game.Map.Tiles[lastPointInPath.X, lastPointInPath.Y].CollisionFlag)
				{
					mousePath.Remove(lastPointInPath);
				}*/

				if(mousePath.Count > 0)
				{
					isMovingByMouse = true;
				}
			}
		}

		public void CheckStopMouseMovement(Creature creature, TileInfo info)
		{
			if(creature == PossessedCreature)
			{
				if (mousePath.Count > 0)
				{
					mousePath.RemoveAt(0);
					if (mousePath.Count == 0)
					{
						isMovingByMouse = false;
					}
				}
			}
		}

		private void SpawnGhost()
		{
			Creature spirit = new Creature(game);
			spirit.Position = PossessedCreature.Position;
			//game.CreatureManager.AddCreature(spirit);
			PossessedCreature = spirit;
			//IsDead = true;
		}

		public void StartTurn()
		{
			game.Map.DisableTileInteraction();

			if(!PossessedCreature.IsAlive)
			{
				return;
			}

			PossessedCreature.ReadyForTurn(movementInput);
		}

		public void FinishTurn()
		{
			PossessedCreature.EndTurn();

			if (mousePath.Count > 0)
			{
				mousePath.RemoveAt(0);
				if (mousePath.Count == 0)
				{
					isMovingByMouse = false;
				}
			}


			if(PossessedCreature.GetBehaviour<Vision>() != null)
			{
				TilesInFOV = (PossessedCreature.GetBehaviour<Vision>() as Vision).VisibleTiles;

				foreach (var tile in TilesInFOV)
				{
					if (game.Map.GetTile(tile) != null)
                    {
						game.Map.GetTile(tile).Visited = true;
					}
				}
			}
		}
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Pandorai.Creatures;
using Pandorai.Creatures.Behaviours;
using Pandorai.Cheats;
using Pandorai.Tilemaps;
using Pandorai.AStarSearchAlgorithm;
using System.Collections.Generic;
using System.Linq;
using Pandorai.Persistency;

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
				possessedCreature.Died += PersistencyLoader.RemoveSaveFile;
				possessedCreature.Died += () =>
				{
					Main.Game.desktop.Root.FindWidgetById("saveGameButton").Enabled = false;
				};
				possessedCreature.Inventory.DisplayAsMainInventory();
			}
		}

		public Vector2 movementInput;

		public bool IsInteractingWithSomeone = false;

		public bool IsDead = false;

		public bool HoldingShift = false;
		public bool HoldingControl = false;

		public List<Point> TilesInFOV = new List<Point>();

		public bool _isMovingByMouse = false;
		private List<Point> _mousePath = new List<Point>();


		public PlayerController()
		{
		}

		public void Update()
		{
			if(Main.Game.InputManager.IsHoldingKey(Keys.LeftShift))
			{
				HoldingShift = true;
			}
			else
			{
				HoldingShift = false;
			}

			if (Main.Game.InputManager.IsHoldingKey(Keys.LeftControl))
			{
				HoldingControl = true;
			}
			else
			{
				HoldingControl = false;
			}

			if(_isMovingByMouse && !CheatConsole.IsActive && !IsInteractingWithSomeone && !HoldingControl)
			{
				if(_mousePath.Count > 0)
				{
					movementInput = (_mousePath[0] - PossessedCreature.MapIndex).ToVector2();

					//game.TurnManager.PlayerIsReady();
					StartTurn();
				}
			}
			else
			{
				var input = Main.Game.InputManager.GetPlayerInput();

				if ((input.Movement != Vector2.Zero || input.Action != InputAction.None) && !CheatConsole.IsActive && !IsInteractingWithSomeone && !PossessedCreature.IsMoving)
				{
					movementInput = input.Movement;
					//game.TurnManager.PlayerIsReady();
					StartTurn();
				}
			}
		}

        public void HandleKeyInput(Keys key)
        {
            if(key == Keys.R) // use ranged weapon
			{
				var rangedWeapon = Main.Game.Player.PossessedCreature.Inventory.Items.FirstOrDefault(x => x.Item.Type.HasFlag(Items.ItemType.Ranged));
				if(rangedWeapon != null)
				{
					rangedWeapon.Item.Use(Main.Game.Player.PossessedCreature);
				}
			}
        }		

		public void MoveByMouse(TileInfo tileInfo)
		{
			if (CheatConsole.IsActive || Main.Game.Player.IsInteractingWithSomeone || Main.Game.Map.AreTilesInteractive || HoldingControl || HoldingShift || PossessedCreature.IsMoving || Main.Game.TurnManager.TurnState != TurnState.WaitingForPlayer)
			{
				return;
			} 

			_mousePath = AStarCreatures.GetShortestPath(Main.Game.Map.Tiles, PossessedCreature.MapIndex, tileInfo.Index, false, true, true);

			if(_mousePath.Count > 0)
			{
				/*var lastPointInPath = mousePath[mousePath.Count - 1];
				if(game.Map.Tiles[lastPointInPath.X, lastPointInPath.Y].CollisionFlag)
				{
					mousePath.Remove(lastPointInPath);
				}*/

				if(_mousePath.Count > 0)
				{
					_isMovingByMouse = true;
				}
			}
		}

		public void CheckStopMouseMovement(Creature creature, TileInfo info)
		{
			if(creature == PossessedCreature)
			{
				if (_mousePath.Count > 0)
				{
					_mousePath.RemoveAt(0);
					if (_mousePath.Count == 0)
					{
						_isMovingByMouse = false;
					}
				}
			}
		}

		public void StartTurn()
		{
			if(!Main.Game.TurnManager.PlayerCanMove)
			{
				return;
			}

			Main.Game.Map.DisableTileInteraction();

			if(!PossessedCreature.IsAlive)
			{
				return;
			}

			PossessedCreature.ReadyForTurn(movementInput);
		}

		public void FinishTurn()
		{
			PossessedCreature.EndTurn();

			if (_mousePath.Count > 0)
			{
				_mousePath.RemoveAt(0);
				if (_mousePath.Count == 0)
				{
					_isMovingByMouse = false;
				}
			}


			if(PossessedCreature.GetBehaviour<Vision>() != null)
			{
				TilesInFOV = (PossessedCreature.GetBehaviour<Vision>() as Vision).VisibleTiles;

				foreach (var tile in TilesInFOV)
				{
					if (Main.Game.Map.GetTile(tile) != null)
                    {
						Main.Game.Map.GetTile(tile).Visited = true;
					}
				}
			}
		}

		private void SpawnGhost()
		{
			Creature spirit = CreatureLoader.GetCreature("Spirit");
			spirit.Position = PossessedCreature.Position;
			Main.Game.CreatureManager.AddCreature(spirit);
			PossessedCreature = spirit;
			IsDead = true;
		}
    }
}

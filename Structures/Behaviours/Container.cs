using Microsoft.Xna.Framework.Input;
using Pandorai.Creatures;
using Pandorai.Items;

namespace Pandorai.Structures.Behaviours
{
	public class Container : Behaviour
	{
		public int ClosedTexture;
		public int OpenTexture;
		public Inventory Inventory;
		public bool IsOpened = false;

		public override void SetAttribute(string name, string value)
		{
			if (name == "ClosedTexture")
			{
				ClosedTexture = int.Parse(value);
			}
			else if (name == "OpenTexture")
			{
				OpenTexture = int.Parse(value);
			}
		}

		public override Behaviour Clone()
		{
			var clone = new Container
			{
				ClosedTexture = ClosedTexture,
				OpenTexture = OpenTexture,
				Inventory = new Inventory(new Creature()),
			};
			return clone;
		}

		public override void Bind()
		{
			Structure.Interacted += Interact;
		}

		public override void Unbind()
		{
			Structure.Interacted -= Interact;
		}

		public override void Interact(Creature creature)
		{
			if (!IsOpened)
			{
				IsOpened = true;
				Structure.Texture = OpenTexture;
			}

			if (!creature.IsPossessedCreature()) return;

			var game = Main.Game;

			game.Player.IsInteractingWithSomeone = true;

			if (!IsOpened)
			{
				IsOpened = true;
				Structure.Texture = OpenTexture;
			}

			Myra.Graphics2D.UI.Window popupWindow = new Myra.Graphics2D.UI.Window()
			{
				Width = 200,
				Height = 250,
			};
			popupWindow.CloseButton.Visible = false;

			void closeWindowOnSpace(Keys k)
			{
				if(k == Keys.Space)
				{
					popupWindow.Close();
				}
			}

			Main.Game.InputManager.SingleKeyPress += closeWindowOnSpace;

			popupWindow.Closed += (s, a) =>
			{
				if (creature == game.Player.PossessedCreature)
				{
					game.Player.IsInteractingWithSomeone = false;
				}

				Main.Game.InputManager.SingleKeyPress -= closeWindowOnSpace;
			};

			int cellSize = 50;

			ItemClickHandler transferItem = null;
			transferItem = (item, user, button) =>
			{
				int amount = game.Player.HoldingShift ? Inventory.FindItem(item).Amount : 1;
				creature.Inventory.AddElement(item, amount);
				this.Inventory.RemoveElement(item, amount);
				popupWindow.Content = Inventory.RefreshGUI();
			};

			popupWindow.Content = Inventory.ConstructGUI(new Myra.Graphics2D.UI.Proportion(Myra.Graphics2D.UI.ProportionType.Pixels, cellSize),
														 new Myra.Graphics2D.UI.Proportion(Myra.Graphics2D.UI.ProportionType.Pixels, cellSize),
														 null, transferItem);

			popupWindow.Content = Inventory.RefreshGUI();

			popupWindow.ShowModal(game.desktop);
		}

		public override void ForceHandler(ForceType force)
		{
			
		}
	}
}

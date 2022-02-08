using Myra.Graphics2D.UI;
using System.Collections.Generic;
using System;
using Pandorai.Creatures;
using Myra.Graphics2D.TextureAtlases;
using Pandorai.Sprites;
using Pandorai.Utility;
using Pandorai.Mechanics;
using Myra.Graphics2D.Brushes;
using Microsoft.Xna.Framework;
using System.Timers;
using Myra.Graphics2D;
using Pandorai.UI;
using System.Linq;

namespace Pandorai.Items
{
    public delegate void ItemClickHandler(Item item, Creature user, ImageTextButton button);

    public class InventoryEntry
	{
        public Item Item;
        public int Amount;

        public InventoryEntry(Item item, int amount) => (Item, Amount) = (item, amount);
	}

	public class Inventory
	{
        public static event ItemClickHandler ItemSelected;
        public static event ItemClickHandler ItemReleased;

        public List<InventoryEntry> Items = new List<InventoryEntry>();

        public Creature Owner;

        public int Collumns = 4;
        public int Rows = 20;
        public int MaxElements = 80;

        public string Id = "inventory";

        Panel currentTooltip = null;
        bool isTooltipVisible = false;

        Game1 game;

        Proportion columnProp;
        Proportion rowProp;
        ItemClickHandler clickHandler;
        ItemClickHandler releaseHandler;

        public Inventory(Creature owner, int maxElements = 80)
		{
            Owner = owner;
            game = Owner.game;
            MaxElements = maxElements;

            for (int i = 0; i < MaxElements; i++)
            {
                Items.Add(new InventoryEntry(new EmptyItem(), 1));
            }
        }

        public void OnItemSelected(Item item, ImageTextButton button)
		{
            ItemSelected?.Invoke(item, Owner, button);
		}

        public void OnItemRelease(Item item, ImageTextButton button)
        {
            ItemReleased?.Invoke(item, Owner, button);
        }

        public void DisplayAsMainInventory()
		{
            Proportion columnProp = new Proportion
            {
                Type = ProportionType.Part,
            };
            Proportion rowProp = new Proportion
            {
                Type = ProportionType.Pixels,
                Value = Options.oldResolution.X / 16,
            };
			GUI.DisplayInventory(ConstructGUI(columnProp, rowProp, (i, c, b) =>
            {
                OnItemSelected(i, b);
            }, (i, c, b) =>
            {
                OnItemRelease(i, b);
                if(!Sidekick.WasItemDragged) i.Use(c);
            }));
		}

        public Panel RefreshGUI()
		{
            var gui = ConstructGUI(columnProp, rowProp, clickHandler, releaseHandler);
            Grid invGrid = (Grid)gui.FindWidgetById(Id);
            foreach (ImageTextButton item in invGrid.Widgets)
            {
                item.Width = (int)columnProp.Value;
                item.Height = (int)rowProp.Value;
            }
            return gui;
        }

        public Panel ConstructGUI(Proportion columnProportion, Proportion rowProportion, ItemClickHandler itemClickHandler, ItemClickHandler itemReleaseHandler)
		{
            columnProp = columnProportion;
            rowProp = rowProportion;
            clickHandler = itemClickHandler;
            releaseHandler = itemReleaseHandler;

            Panel panel = new Panel
            {
            };

            Grid addedElementsHolder = new Grid
            {
                ShowGridLines = true,
                Id = Id,
            };

            ScrollViewer inventoryScroll = new ScrollViewer
            {
                Id = "inventoryScroll",
                ShowVerticalScrollBar = false
            };
            inventoryScroll.Content = addedElementsHolder;

            for (int i = 0; i < Collumns; i++)
            {
                addedElementsHolder.ColumnsProportions.Add(columnProportion);
            }

            for (int i = 0; i < Rows; i++)
            {
                addedElementsHolder.RowsProportions.Add(rowProportion);
            }

            panel.Widgets.Add(inventoryScroll);

            int elementCount = 0;

			foreach (var itemEntry in Items)
			{
                int amount = itemEntry.Amount;
                var item = itemEntry.Item;

                ImageTextButton element;
                if(item.GetType() != typeof(EmptyItem))
				{
                    var icon = new TextureRegion(TilesheetManager.MapSpritesheetTexture.ExtractSubtexture(TilesheetManager.MapObjectSpritesheet[item.Texture].Rect, game.GraphicsDevice));
                    // TODO: make the icon tinted with its item's ColorTint property
                    element = new ImageTextButton
                    {
                        Image = icon,
                        Text = amount.ToString(),
                        Height = addedElementsHolder.GetCellRectangle(0, 0).Height,
                        Width = addedElementsHolder.GetCellRectangle(0, 0).Width,
                    };
                    if(item.Equipped)
					{
                        // ideally should be element.SetButtonStyle()
                        element.Background = new SolidBrush(Color.DarkRed);
					}
                }
                else
				{
                    element = new ImageTextButton
                    {
                        Height = addedElementsHolder.GetCellRectangle(0, 0).Height,
                        Width = addedElementsHolder.GetCellRectangle(0, 0).Width,
                    };
                }

                EventHandler releaseClickHandler = (s, a) =>
                {
                    itemReleaseHandler?.Invoke(item, Owner, element);
                };

                element.TouchDown += (s, a) =>
                {
                    itemClickHandler?.Invoke(item, Owner, element);
                };

                element.TouchUp += releaseClickHandler;

                // tooltips
                if(item.GetType() != typeof(EmptyItem))
				{
                    MouseHandler moveTooltip = pos =>
                    {
                        if (!isTooltipVisible) return;

                        currentTooltip.Left = (int)pos.X;
                        currentTooltip.Top = (int)pos.Y;
                    };

                    void removeTooltip(Panel tooltip)
                    {
                        if (tooltip == null) return;
                        isTooltipVisible = false;
                        tooltip.Visible = false;
                        if(tooltip.Desktop != null)
                            tooltip.RemoveFromDesktop();
                        tooltip = null;
                        game.InputManager.MouseMove -= moveTooltip;
                    };                    

                    element.MouseEntered += (s, a) =>
                    {
                        if (isTooltipVisible) return;

                        isTooltipVisible = true;

                        currentTooltip = new Panel
                        {
                            Left = (int)game.InputManager.MousePos.X,
                            Top = (int)game.InputManager.MousePos.Y,
                            Background = new SolidBrush(Color.Black * 0.5f),
                            Width = 180,
                            Height = 120,
                            Enabled = false,
                            Border = new SolidBrush(item.TooltipColor),
                            BorderThickness = new Thickness(5),
                        };

                        var stackPanel = new VerticalStackPanel();
                        stackPanel.Proportions.Add(new Proportion());

                        stackPanel.Widgets.Add(new Label
                        {
                            Text = item.Name,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Wrap = true,
                        });
                        stackPanel.Widgets.Add(new Label
                        {
                            Text = item.Description,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Wrap = true,
                        });
                        currentTooltip.Widgets.Add(stackPanel);

                        game.desktop.Widgets.Add(currentTooltip);

                        game.InputManager.MouseMove += moveTooltip;

                        var lastTooltip = currentTooltip;
                        // safety measure
                        // if the tooltip didnt disappear for some reason, automatically remove it after 5 seconds
                        // to prevent tooltips that are stuck
                        Timer removalTimer = new Timer(5000);
                        removalTimer.Elapsed += (s1, e1) =>
                        {
                            if(lastTooltip != null)
                            {
                                removeTooltip(lastTooltip);
                            }
                            removalTimer.Stop();
                            removalTimer.Dispose();
                        };
                        removalTimer.Start();
                    };

                    element.MouseLeft += (s, a) =>
                    {
                        removeTooltip(currentTooltip);
                    };

                    element.Click += (s, a) =>
                    {
                        removeTooltip(currentTooltip);
                    };
                }

                if (elementCount % Collumns == 0)
                {
                    element.GridColumn = elementCount % Collumns;
                    element.GridRow = elementCount / Collumns;
                }
                else
                {
                    element.GridColumn = elementCount % Collumns;
                    element.GridRow = (elementCount - (elementCount % Collumns)) / Collumns;
                }

                elementCount++;

                addedElementsHolder.Widgets.Add(element);

                inventoryScroll.Content = addedElementsHolder;
			}

            return panel;
        }

		public void AddElement(Item item, int amount = 1)
		{
            if(!ContainsItem(item))
			{
                ReplaceSlot(item, amount, Items.IndexOf(Items.Find(i => i.Item.GetType() == typeof(EmptyItem))));
			}
            else
			{
                FindItem(item).Amount += amount;
			}

            if (!game.IsGameStarted) return;

            if(Owner == Owner.game.Player.PossessedCreature)
			{
                DisplayAsMainInventory();
			}

            Sidekick.DisplaySlots();
            game.Options.AdjustGUI();
        }

        public void AddElement(Item item, int amount, int slotIndex)
        {
            if (!ContainsItem(item))
            {
                ReplaceSlot(item, amount, slotIndex);
            }
            else
            {
                FindItem(item).Amount += amount;
            }

            if (!game.IsGameStarted) return;

            if (Owner == Owner.game.Player.PossessedCreature)
            {
                DisplayAsMainInventory();
            }
            Sidekick.DisplaySlots();
            game.Options.AdjustGUI();
        }

        public void AddElements(List<Item> items)
		{
			foreach (var item in items)
			{
                AddElement(item);
			}
		}

        public void AddElements(List<InventoryEntry> items)
        {
            foreach (var item in items)
            {
                AddElement(item.Item, item.Amount);
            }
        }

        public void RemoveElement(Item item, int amount = 1)
		{
            if (!ContainsItem(item)) return;

            var itemEntry = FindItem(item);

            itemEntry.Amount -= amount;
            int index = Items.IndexOf(itemEntry);

            if (itemEntry.Amount <= 0)
			{
                ReplaceSlot(new EmptyItem(), 1, index);
			}

            if (Owner == Owner.game.Player.PossessedCreature)
            {
                DisplayAsMainInventory();
            }
            Sidekick.DisplaySlots();
            game.Options.AdjustGUI();
        }

        public void RemoveElement(string itemName, int amount = 1)
		{
            if (!ContainsItem(itemName)) return;

            var itemEntry = FindItem(itemName);

            itemEntry.Amount -= amount;
            int index = Items.IndexOf(itemEntry);

            if (itemEntry.Amount <= 0)
			{
                ReplaceSlot(new EmptyItem(), 1, index);
			}

            if (Owner == Owner.game.Player.PossessedCreature)
            {
                DisplayAsMainInventory();
            }
            Sidekick.DisplaySlots();
            game.Options.AdjustGUI();
        }

        public void ReplaceSlot(Item item, int amount, int slotIndex)
		{
            Items[slotIndex].Item = item;
            Items[slotIndex].Amount = amount;

            if (!game.IsGameStarted) return;

            if (Owner == Owner.game.Player.PossessedCreature)
            {
                DisplayAsMainInventory();
            }
            Sidekick.DisplaySlots();
            game.Options.AdjustGUI();
		}

        public bool ContainsItem(Item item)
		{
            return Items.Any(x => x.Item.Id == item.Id);
		}

        public bool ContainsItem(string itemName)
		{
            return Items.Any(x => x.Item.Id == itemName);
		}

        public InventoryEntry FindItem(Item item)
		{
            return Items.Find(i => i.Item.Id == item.Id);
		}

        public InventoryEntry FindItem(string itemName)
		{
            return Items.Find(i => i.Item.Id == itemName);
		}
	}
}

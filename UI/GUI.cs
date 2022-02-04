using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI.Styles;
using System;
using Pandorai.Cheats;
using Pandorai.Dialogues;
using Myra.Graphics2D.UI.Properties;

namespace Pandorai.UI
{
	static class GUI
	{
        static Game1 game;
        static Desktop desktop;

        public static Grid inventorySlotsGrid;

        static Panel oldInventoryPanel = null;

		public static Widget LoadGUI(Game1 thisGame, Desktop thisDesktop)
		{
            Stylesheet.Current.ButtonStyle.DisabledBackground = new SolidBrush(new Color(0.6f, 0.6f, 0.6f, 1f));

            game = thisGame;
            desktop = thisDesktop;

            Panel rootPanel = new Panel();

            var centerButtons = MainMenu();

            var gameScreen = GameScreen();
            gameScreen.Visible = false;

            rootPanel.Widgets.Add(centerButtons);
            rootPanel.Widgets.Add(gameScreen);

            return rootPanel;
        }

        public static void DisplayInventory(Panel inventory)
		{
            if (oldInventoryPanel != null) inventorySlotsGrid.Widgets.Remove(oldInventoryPanel);

            inventory.GridColumn = 0;
            inventory.GridRow = 1;

            inventorySlotsGrid.Widgets.Add(inventory);
            oldInventoryPanel = inventory;

            game.Options.AdjustGUI();
        }

        static Widget MainMenu()
		{
            VerticalStackPanel centerButtonsHolder = new VerticalStackPanel
            {
                Id = "mainMenu",
                Spacing = 5,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidBrush(new Color(0.6f, 0.5f, 0.45f, 1f)),
                Padding = new Thickness(15, 25)
            };

            var continueButton = new TextButton
            {
                Id = "continueButton",
                Text = "Continue",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 80,
                Height = 25,
                Enabled = false
            };

            continueButton.Click += (s, a) =>
            {
                game.TogglePauseGame();
            };

            var playButton = new TextButton
            {
                Text = "New game",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 90,
                Height = 25
            };

            playButton.Click += (s, a) =>
            {
                game.StartGame();
            };

            var optionsButton = new TextButton
            {
                Text = "Options",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 75,
                Height = 25
            };

            optionsButton.Click += (s, a) =>
            {
                Dialog optionsWindow = OptionsWindow();

                optionsWindow.ShowModal(desktop);
            };

            var exitButton = new TextButton
            {
                Text = "Exit",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 50,
                Height = 25
            };

            exitButton.Click += (s, a) =>
            {
                game.Exit();
            };

            centerButtonsHolder.Widgets.Add(continueButton);
            centerButtonsHolder.Widgets.Add(playButton);
            centerButtonsHolder.Widgets.Add(optionsButton);
            centerButtonsHolder.Widgets.Add(exitButton);

            return centerButtonsHolder;
        }

        static Widget GameScreen()
		{
            Panel gameScreen = new Panel
            {
                Id = "gameScreen"
            };

            Grid grid = new Grid
            {
                Id = "viewportGrid",
                ShowGridLines = true,
                ColumnSpacing = 0,
                RowSpacing = 0
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 3));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));

            grid.RowsProportions.Add(new Proportion(ProportionType.Part, 3));
            grid.RowsProportions.Add(new Proportion(ProportionType.Part, 1));

            var messageLogPanel = MessageLog.GUI();
            messageLogPanel.GridColumn = 0;
            messageLogPanel.GridRow = 1;
            grid.Widgets.Add(messageLogPanel);

            var dialoguePanel = DialoguePanel();
            dialoguePanel.GridColumn = 0;
            dialoguePanel.GridRow = 1;
            grid.Widgets.Add(dialoguePanel);

            var debugPanel = DebugPanel();
            debugPanel.GridColumn = 1;
            debugPanel.GridRow = 1;
            grid.Widgets.Add(debugPanel);

            Grid slotsGrid = new Grid
            {
                Id = "inventorySlotsGrid",
                ShowGridLines = true,
                GridColumn = 1,
                GridRow = 0,
            };

            slotsGrid.RowsProportions.Add(new Proportion(ProportionType.Part, 1));
            slotsGrid.RowsProportions.Add(new Proportion(ProportionType.Part, 5));

            inventorySlotsGrid = slotsGrid;

            grid.Widgets.Add(slotsGrid);

            gameScreen.Widgets.Add(grid);

            return gameScreen;
		}

        static Widget DialoguePanel()
		{
            Panel mainPanel = new Panel
            {
                Id = "DialoguePanel"
            };

            Label nameLabel = new Label
            {
                Id = "DialogueNameLabel",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidBrush(new Color(Color.Black, 0.5f))
            };

            Label textLabel = new Label
            {
                Id = "DialogueTextLabel",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidBrush(new Color(Color.Black, 0.5f)),
                Wrap = true
            };

            VerticalStackPanel dialogueOptions = new VerticalStackPanel
            {
                Id = "DialogueOptionsStack",
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = (int)(Options.oldResolution.X * 0.75f)
            };

            DialogueManager.NameLabel = nameLabel;
            DialogueManager.TextLabel = textLabel;
            DialogueManager.OptionsStack = dialogueOptions;

            mainPanel.Widgets.Add(nameLabel);
            mainPanel.Widgets.Add(textLabel);
            mainPanel.Widgets.Add(dialogueOptions);

            return mainPanel;
		}

        static Widget DebugPanel()
		{
            Panel panel = new Panel
            {
            };

            ScrollViewer scroll = new ScrollViewer
            {
                Id = "TilePropertGridScroll"
            };

            PropertyGrid propertyGrid = new PropertyGrid
            {
                Id = "TilePropertyGrid"
            };

            scroll.Content = propertyGrid;

            panel.Widgets.Add(scroll);

            return panel;
		}

        static Dialog OptionsWindow()
        {
            Dialog window = new Dialog
            {
                Id = "optionsWindow",
                Title = "Options",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                //IsDraggable = true,
                Width = 500,
                Height = 300,
                Opacity = 1f
            };

            window.Closed += (s, e) =>
            {
                if(window.Result)
				{
                    game.Options.ApplyChanges();
				}
                else
				{
                    game.Options.RevertChanges();
				}
            };

            Grid windowPanel = new Grid
            {
                Background = new SolidBrush(Color.Gray)
            };

            windowPanel.ColumnsProportions.Add(new Proportion());
            windowPanel.ColumnsProportions.Add(new Proportion());

            VerticalStackPanel verticalStackPanel1 = new VerticalStackPanel
            {
                Spacing = 5,
                Margin = new Thickness(5),
                GridColumn = 0
            };
            //verticalStackPanel1.Proportions.Add(new Proportion());

            Label resolutionChoice = new Label
            {
                Text = "Resolution:"
            };

            ComboBox resolutionList = new ComboBox
            {
            };
            resolutionList.Items.Add(new ListItem("1440x900", null, new Point(1440, 900)));
            resolutionList.Items.Add(new ListItem("800x600", null, new Point(800, 600)));

			foreach (var item in resolutionList.Items)
			{
                if((Point)item.Tag == Options.oldResolution)
				{
                    resolutionList.SelectedItem = item;
                    break;
				}
			}

            TextButton applyResolutionButton = new TextButton
            {
                Text = "Apply"
            };

            applyResolutionButton.Click += (s, e) =>
            {
                game.Options.ChangeResolution((Point)resolutionList.SelectedItem.Tag, window);
            };

            HorizontalSlider zoomSlider = new HorizontalSlider
            {
                Minimum = 8,
                Maximum = 128,
                Value = game.Options.TileSize,
            };

            zoomSlider.ValueChanged += (s, a) =>
            {
                game.Options.TileSize = (int)a.NewValue;
            };

            VerticalStackPanel verticalStackPanel2 = new VerticalStackPanel
            {
                Spacing = 5,
                Margin = new Thickness(5),
                GridColumn = 1
            };

            Label fullScreenLabel = new Label
            {
                Text = "Fullscreen"
            };

            CheckBox fullScreenCheckbox = new CheckBox
            {
            };

            fullScreenCheckbox.IsPressed = game.Options.oldIsFullScreen;

            fullScreenCheckbox.Click += (s, e) =>
            {
                game.Options.ToggleFullscreen();
            };

            verticalStackPanel1.Widgets.Add(resolutionChoice);
            verticalStackPanel1.Widgets.Add(resolutionList);
            verticalStackPanel1.Widgets.Add(applyResolutionButton);
            verticalStackPanel1.Widgets.Add(zoomSlider);

            verticalStackPanel2.Widgets.Add(fullScreenLabel);
            verticalStackPanel2.Widgets.Add(fullScreenCheckbox);

            windowPanel.Widgets.Add(verticalStackPanel1);

            windowPanel.Widgets.Add(verticalStackPanel2);

            window.Content = windowPanel;

            return window;
        }
	}
}

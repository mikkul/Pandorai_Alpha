using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI.Styles;
using Pandorai.Dialogues;
using Myra.Graphics2D.UI.Properties;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.TextureAtlases;
using System.Linq;
using Pandorai.Sounds;
using System.Globalization;

namespace Pandorai.UI
{
	static class GUI
	{
        public static Grid InventorySlotsGrid;

        private static Main _game;
        private static Desktop _desktop;

        private static Panel _oldInventoryPanel = null;

        private static bool _isCharacterStatsWindowOpen = false;
        private static Window _characterStatsWindow;

        public static Widget LoadGUI(Main thisGame, Desktop thisDesktop)
		{
            Stylesheet.Current.ButtonStyle.DisabledBackground = new SolidBrush(new Color(0.6f, 0.6f, 0.6f, 1f));
            Stylesheet.Current.ButtonStyle.Height = 25;
            Stylesheet.Current.ButtonStyle.Background = new SolidBrush(Color.Black);
            Stylesheet.Current.ButtonStyle.DisabledBackground = new SolidBrush(Color.Black);
            Stylesheet.Current.WindowStyle.Background = new SolidBrush(Color.Black);
            Stylesheet.Current.WindowStyle.Border = new SolidBrush(Color.White);
            Stylesheet.Current.WindowStyle.BorderThickness = new Thickness(1);
            Stylesheet.Current.HorizontalSeparatorStyle.Thickness = 1;
            Stylesheet.Current.HorizontalSeparatorStyle.Image = new TextureRegion(Main.Game.squareTexture);
            Stylesheet.Current.HorizontalSeparatorStyle.Margin = new Thickness(0, 5);

            _game = thisGame;
            _desktop = thisDesktop;

            InitCharacterStatsWindow();

            Panel rootPanel = new Panel();

            var mainMenu = MainMenu();

            var difficultyChoiceScreen = DifficultyChoiceScreen();
            difficultyChoiceScreen.Visible = false;

            var loadingScreen = LoadingScreen();
            loadingScreen.Visible = false;

            var gameScreen = GameScreen();
            gameScreen.Visible = false;

            rootPanel.Widgets.Add(mainMenu);
            rootPanel.Widgets.Add(difficultyChoiceScreen);
            rootPanel.Widgets.Add(gameScreen);
            rootPanel.Widgets.Add(loadingScreen);

            return rootPanel;
        }

        public static void DisplayInventory(Panel inventory)
		{
            if (_oldInventoryPanel != null) InventorySlotsGrid.Widgets.Remove(_oldInventoryPanel);

            inventory.GridColumn = 0;
            inventory.GridRow = 1;

            InventorySlotsGrid.Widgets.Add(inventory);
            _oldInventoryPanel = inventory;

            _game.Options.AdjustGUI();
        }

        public static void ShowCheatsModifyStatsWindow()
        {
            _characterStatsWindow = new Window
            {
                Id = "cheatsModifycharacterStatsWindow",
                Title = "Stats",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = 500,
                Height = 600,
            };
            
            PropertyGrid contentGrid = new PropertyGrid
            {
                Id = "cheatsModifyCharacterStatsPropertyGrid",
            };
            contentGrid.Object = Main.Game.Player.PossessedCreature.Stats;

            _characterStatsWindow.Content = contentGrid;

            _characterStatsWindow.ShowModal(_desktop);
        }

        private static void InitCharacterStatsWindow()
        {
            _characterStatsWindow = new Window
            {
                Id = "characterStatsWindow",
                Title = "Stats",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = 700,
                Height = 650,
            };
            _characterStatsWindow.CloseButton.Visible = false;
            
            PropertyGrid contentGrid = new PropertyGrid
            {
                Id = "characterStatsPropertyGrid",
            };

            _characterStatsWindow.Content = contentGrid;

            Main.Game.InputManager.SingleKeyPress += k =>
            {
                if(k == Keys.C)
                {
                    if(_isCharacterStatsWindowOpen)
                    {
                        _isCharacterStatsWindowOpen = false;
                        _characterStatsWindow.Close();
                    }
                    else
                    {
                        _isCharacterStatsWindowOpen = true;
                        contentGrid = new PropertyGrid
                        {
                            Id = "characterStatsPropertyGrid",
                        };
                        var characterStats = new CharacterStats(Main.Game.Player.PossessedCreature.Stats);
                        contentGrid.Object = characterStats;
                        _characterStatsWindow.Content = contentGrid;
                        _characterStatsWindow.ShowModal(_desktop);
                    }
                }
            };
        }

        private static Widget LoadingScreen()
        {
            var mainPanel = new Panel
            {
                Id = "loadingScreen",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var loadingLabel = new Label
            {
                Id = "loadingTextLabel",
                Text = "Loading",
            };

            mainPanel.Widgets.Add(loadingLabel);

            return mainPanel;
        }

        private static Widget DifficultyChoiceScreen()
        {
            var mainPanel = new VerticalStackPanel
            {
                Id = "difficultyChoiceScreen",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var chooseDifficultyLabel = new Label
            {
                Text = "Choose difficulty:",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 25),
            };

            var easier = new TextButton
            {
                Text = "Easier",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            var normal = new TextButton
            {
                Text = "Normal",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            var challenging = new TextButton
            {
                Text = "Challenging",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            easier.Click += (s, a) =>
            {
                _game.ExperienceMultiplier = 1.25f;
                Task.Run(() => _game.StartGame());
            };
            normal.Click += (s, a) =>
            {
                _game.ExperienceMultiplier = 1.0f;
                Task.Run(() => _game.StartGame());
            };
            challenging.Click += (s, a) =>
            {
                _game.ExperienceMultiplier = 0.75f;
                Task.Run(() => _game.StartGame());
            };

            mainPanel.Widgets.Add(chooseDifficultyLabel);
            mainPanel.Widgets.Add(challenging);
            mainPanel.Widgets.Add(normal);
            mainPanel.Widgets.Add(easier);

            return mainPanel;
        }

        private static Widget MainMenu()
		{
            Grid mainGrid = new Grid()
            {
                Id = "mainMenu",
            };
            mainGrid.RowsProportions.Add(new Proportion(ProportionType.Part, 1));
            mainGrid.RowsProportions.Add(new Proportion(ProportionType.Part, 3));
            mainGrid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 3));
            mainGrid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 4));

            Image logo = new Image
            {
                Background = new TextureRegion(_game.LogoTexture),
                GridRow = 0,
                GridColumnSpan = 2,
                Width = 800,
                Height = 300,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            Image sampleImage = new Image
            {
                Background = new TextureRegion(_game.MainMenuImage),
                GridRow = 1,
                GridColumn = 1,
                Width = 1325,
                Height = 820,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            VerticalStackPanel buttonsStackPanel = new VerticalStackPanel
            {
                Id = "mainMenuButtons",
                Spacing = 5,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(25, 50),
                Width = 300,
                GridRow = 1,
                GridColumn = 0,
                Border = new SolidBrush(Color.White),
                BorderThickness = new Thickness(1),
                Background = new SolidBrush(Color.Black),
                Top = -75,
            };

            var continueButton = new TextButton
            {
                Id = "continueButton",
                Text = "Continue",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Enabled = false,
            };

            continueButton.Click += (s, a) =>
            {
                _game.TogglePauseGame();
            };

            var playButton = new TextButton
            {
                Text = "New game",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            playButton.Click += (s, a) =>
            {
                _game.desktop.Root.FindWidgetById("mainMenu").Visible = false;
                _game.desktop.Root.FindWidgetById("difficultyChoiceScreen").Visible = true;
            };

            var tutorialButton = new TextButton
            {
                Text = "Tutorial",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            tutorialButton.Click += (s, a) =>
            {
                Window tutorialWindow = TutorialWindow();

                tutorialWindow.ShowModal(_desktop);
            };

            var optionsButton = new TextButton
            {
                Text = "Options",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            optionsButton.Click += (s, a) =>
            {
                Dialog optionsWindow = OptionsWindow();

                optionsWindow.ShowModal(_desktop);
            };

            var creditsButton = new TextButton
            {
                Text = "Credits",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            creditsButton.Click += (s, a) =>
            {
                Window creditsWindow = CreditsWindow();

                creditsWindow.ShowModal(_desktop);
            };

            var exitButton = new TextButton
            {
                Text = "Quit",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            exitButton.Click += (s, a) =>
            {
                _game.Exit();
            };

            buttonsStackPanel.Widgets.Add(continueButton);
            buttonsStackPanel.Widgets.Add(playButton);
            buttonsStackPanel.Widgets.Add(new HorizontalSeparator());
            buttonsStackPanel.Widgets.Add(tutorialButton);
            buttonsStackPanel.Widgets.Add(optionsButton);
            buttonsStackPanel.Widgets.Add(creditsButton);
            buttonsStackPanel.Widgets.Add(new HorizontalSeparator());
            buttonsStackPanel.Widgets.Add(exitButton);

            mainGrid.Widgets.Add(logo);
            mainGrid.Widgets.Add(sampleImage);
            mainGrid.Widgets.Add(buttonsStackPanel);

            return mainGrid;
        }

        private static Widget GameScreen()
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

            slotsGrid.RowsProportions.Add(new Proportion(ProportionType.Part, 0));
            slotsGrid.RowsProportions.Add(new Proportion(ProportionType.Part, 5));

            InventorySlotsGrid = slotsGrid;

            grid.Widgets.Add(slotsGrid);

            gameScreen.Widgets.Add(grid);

            return gameScreen;
		}

        private static Widget DialoguePanel()
		{
            Panel mainPanel = new Panel
            {
                Id = "DialoguePanel",
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
                Width = (int)(Options.OldResolution.X * 0.75f)
            };

            ScrollViewer dialogueOptionsScrollViewer = new ScrollViewer
            {
                Content = dialogueOptions,
            };

            DialogueManager.NameLabel = nameLabel;
            DialogueManager.TextLabel = textLabel;
            DialogueManager.OptionsStack = dialogueOptions;

            mainPanel.Widgets.Add(nameLabel);
            mainPanel.Widgets.Add(textLabel);
            mainPanel.Widgets.Add(dialogueOptionsScrollViewer);

            return mainPanel;
		}

        private static Widget DebugPanel()
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

        private static Window TutorialWindow()
        {
            Window window = new Window
            {
                Id = "tutorialWindow",
                Title = "Tutorial",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 900,
                Height = 600,
                Opacity = 1f,
            };

            VerticalStackPanel stackPanel = new VerticalStackPanel
            {
                Spacing = 5,
                Margin = new Thickness(5),
                GridColumn = 0,
            };

            stackPanel.Widgets.Add(new Label
            {
                Text = "[Insert some corny story about the hero and his goal]",
                Wrap = true,
                Margin = new Thickness(0, 0, 0, 20),
            });

            stackPanel.Widgets.Add(new Label
            {
                Text = "8-way movement with WASD, Arrow keys or Numpad. You can also move clicking on the target tile with your mouse.",
                Wrap = true,
                Margin = new Thickness(0, 0, 0, 10),
            });

            stackPanel.Widgets.Add(new Label
            {
                Text = "Rest of the stuff is pretty self-explanatory. If you move onto a tile with an item, you collect it. If you bump into a structure or a creature, you interact with it. In case of monsters the default action is attack.",
                Wrap = true,
                Margin = new Thickness(0, 0, 0, 10),
            });

            stackPanel.Widgets.Add(new Label
            {
                Text = "Have fun!",
                Wrap = true,
                Margin = new Thickness(0, 0, 0, 0),
            });

            window.Content = stackPanel;

            return window;
        }

        private static Window CreditsWindow()
        {
            Window window = new Window
            {
                Id = "creditsWindow",
                Title = "Credits",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 900,
                Height = 600,
                Opacity = 1f,
            };

            VerticalStackPanel stackPanel = new VerticalStackPanel
            {
                Spacing = 5,
                Margin = new Thickness(5),
                GridColumn = 0,
            };

            stackPanel.Widgets.Add(new Label
            {
                Text = "Game made by Nikolaj Kuleszow (https://github.com/mikkul)",
                Wrap = true,
                Margin = new Thickness(0, 0, 0, 20),
            });

            stackPanel.Widgets.Add(new Label
            {
                Text = "Music used:",
                Margin = new Thickness(0, 0, 0, 5),
            });

            stackPanel.Widgets.Add(creditLabel("Jordan Hake (https://opengameart.org/users/vwolfdog)", "https://opengameart.org/content/soft-mysterious-harp-loop"));
            stackPanel.Widgets.Add(creditLabel("Irrational Machines", "https://opengameart.org/content/rpg-title-screen-music-pack"));
            stackPanel.Widgets.Add(creditLabel("Alexandr Zhelanov (https://soundcloud.com/alexandr-zhelanov)", "hhttps://opengameart.org/content/ancient-temple", "https://opengameart.org/content/mystery-forest"));

            stackPanel.Widgets.Add(new Label
            {
                Text = "Sounds used:",
                Margin = new Thickness(0, 10, 0, 5),
            });

            stackPanel.Widgets.Add(creditLabel("Jute", "https://opengameart.org/content/foot-walking-step-sounds-on-stone-water-snow-wood-and-dirt"));
            stackPanel.Widgets.Add(creditLabel("https://opengameart.org/users/arcadeparty", "https://opengameart.org/content/zombie-skeleton-monster-voice-effects"));
            stackPanel.Widgets.Add(creditLabel("jukeri (https://opengameart.org/users/jukeri12)", "https://opengameart.org/content/dog-sounds-0"));
            stackPanel.Widgets.Add(creditLabel("Little Robot Sound Factory (www.littlerobotsoundfactory.com)", "https://opengameart.org/content/fantasy-sound-effects-library"));
            stackPanel.Widgets.Add(creditLabel("Jesús Lastra (https://opengameart.org/users/jalastram)", "https://opengameart.org/content/sound-effects-sfx015"));
            stackPanel.Widgets.Add(creditLabel("https://opengameart.org/users/ogrebane", "https://opengameart.org/content/monster-sound-pack-volume-1"));

            window.Content = stackPanel;

            return window;

            Widget creditLabel(string author, params string[] assetLinks)
            {
                Label label = new Label
                {
                    Text = $"{author} : {string.Join(", ", assetLinks)}",
                    Wrap = true,
                };
                return label;
            }
        }

        private static Dialog OptionsWindow()
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
                Opacity = 1f,
            };

            window.Closed += (s, e) =>
            {
                if(window.Result)
				{
                    _game.Options.ApplyChanges();
				}
                else
				{
                    _game.Options.RevertChanges();
				}
            };

            Grid windowPanel = new Grid
            {
                Background = new SolidBrush(Color.Black),
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

            HorizontalStackPanel resolutionInputPanel = new HorizontalStackPanel
            {
                Spacing = 5,
            };

            TextBox resolutionWidthTextBox = new TextBox
            {
                Text = "800",
                Width = 50,
            };

            Label resolutionDividerLabel = new Label
            {
                Text = "x",
            };

            TextBox resolutionHeightTextBox = new TextBox
            {
                Text = "600",
                Width = 50,
            };

            TextButton applyResolutionButton = new TextButton
            {
                Text = "Apply"
            };
            
            resolutionInputPanel.Widgets.Add(resolutionWidthTextBox);
            resolutionInputPanel.Widgets.Add(resolutionDividerLabel);
            resolutionInputPanel.Widgets.Add(resolutionHeightTextBox);
            resolutionInputPanel.Widgets.Add(applyResolutionButton);

            applyResolutionButton.Click += (s, e) =>
            {
                int resolutionWidth = int.Parse(resolutionWidthTextBox.Text);
                int resolutionHeight = int.Parse(resolutionHeightTextBox.Text);
                _game.Options.ChangeResolution(new Point(resolutionWidth, resolutionHeight), window);
            };

            Label musicVolumeLabel = new Label
            {
                Text = "Music volume:",
            };

            HorizontalSlider musicVolumeSlider = new HorizontalSlider
            {
                Minimum = 0,
                Maximum = 100,
                Value = SoundManager.MusicVolume,
            };

            musicVolumeSlider.ValueChanged += (s, a) =>
            {
                SoundManager.MusicVolume = (int)a.NewValue;
            };
        
            Label soundsVolumeLabel = new Label
            {
                Text = "Sounds volume:",
            };

            HorizontalSlider soundsVolumeSlider = new HorizontalSlider
            {
                Minimum = 0,
                Maximum = 100,
                Value = SoundManager.SoundsVolume,
            };

            soundsVolumeSlider.ValueChanged += (s, a) =>
            {
                SoundManager.SoundsVolume = (int)a.NewValue;
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

            fullScreenCheckbox.IsPressed = _game.Options.OldIsFullScreen;

            fullScreenCheckbox.Click += (s, e) =>
            {
                _game.Options.ToggleFullscreen();
            };

            verticalStackPanel1.Widgets.Add(resolutionChoice);
            verticalStackPanel1.Widgets.Add(resolutionInputPanel);
            verticalStackPanel1.Widgets.Add(musicVolumeLabel);
            verticalStackPanel1.Widgets.Add(musicVolumeSlider);
            verticalStackPanel1.Widgets.Add(soundsVolumeLabel);
            verticalStackPanel1.Widgets.Add(soundsVolumeSlider);

            verticalStackPanel2.Widgets.Add(fullScreenLabel);
            verticalStackPanel2.Widgets.Add(fullScreenCheckbox);

            windowPanel.Widgets.Add(verticalStackPanel1);

            windowPanel.Widgets.Add(verticalStackPanel2);

            window.Content = windowPanel;

            return window;
        }
	}
}

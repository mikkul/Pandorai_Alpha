using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Pandorai.Tilemaps;
using System;
using Pandorai.MapGeneration;
using Pandorai.Creatures;
using Pandorai.Mechanics;
using Pandorai.Sprites;
using FPSCounter;
using Pandorai.Dialogues;
using Pandorai.Cheats;
using Myra.Graphics2D.UI.Properties;
using Pandorai.Items;
using Pandorai.Structures;
using Pandorai.Rendering;
using Pandorai.ParticleSystems;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Pandorai.Utility;
using Pandorai.Tooltips;
using Pandorai.UI;
using Pandorai.MapGeneration.CustomRegions;
using Pandorai.Sounds;
using Microsoft.Xna.Framework.Media;
using System.Timers;
using System.Linq;

namespace Pandorai
{
    public delegate void EmptyEventHandler();
    public delegate void InputEventHandler(CustomInput input);

    public class Main : Game
    {
        public static Main Game;

        public GraphicsDeviceManager Graphics;

        public Desktop desktop;

        public Options Options;

        public Camera Camera;

        public Texture2D squareTexture;
        public Texture2D smokeParticleTexture;

        public bool IsGameStarted = false;
        public bool IsGamePaused = true;

        public Map Map;
        public PlayerController Player;
        public CreatureManager CreatureManager;
        public InputManager InputManager;
        public TurnManager TurnManager;
        public GameStateManager GameStateManager;
        public Trivia BasicTrivia;

        public RenderTarget2D ViewportTarget;

        public Texture2D fireParticleTexture;
        
        public Texture2D LogoTexture;
        public Texture2D MainMenuImage;

        public Random MainRng = new Random();

        public Effect spiritWorldEffectBackground;
        public Effect spiritWorldEffectForeground;
        public Effect distortionEffect;

        public Texture2D dayNightColorMaskTexture;

        public event EmptyEventHandler GameStarted;
        
        private SmartFramerate _fpsCounter;

        private PSSparkles _mouseSparkle;

        private SpriteBatch _spriteBatch;

        private SpriteFont _defaultFont;

        private Timer _loadingAnimationTimer;

        private RenderHelper _viewportRenderer;

        private bool _firstGameLoad = true;
        
        public Main()
        {
            Graphics = new GraphicsDeviceManager(this);

            Graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = true;
            IsMouseVisible = true;
            Window.AllowUserResizing = false;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Game = this;

            CheatConsole.Game = this;
            CheatConsole.InitCommands();

            DialogueManager.game = this;

            CheatShortcuts.Game = this;

            TileInteractionManager.Game = this;

            ParticleSystemManager.game = this;

            MapTooltip.Init(this);

            Options = new Options(this);

            Camera = new Camera();

            // hacky way to set viewport dimensions
            // TODO: do it proper way accessing grid viewport cell dimensions
            Camera.UpdateViewport(Options);
            ViewportTarget = new RenderTarget2D(GraphicsDevice, Camera.Viewport.Width, Camera.Viewport.Height);

            CreatureManager = new CreatureManager(this); 

            Camera.UpdateViewport(Options);
            ViewportTarget = new RenderTarget2D(GraphicsDevice, Camera.Viewport.Width, Camera.Viewport.Height);

            InputManager = new InputManager(this);

            TurnManager = new TurnManager(this);

            GameStateManager = new GameStateManager(this);

            Player = new PlayerController(this);

            BasicTrivia = new Trivia("defaultTrivia.txt");

            _viewportRenderer = new RenderHelper(this, () => ViewportTarget.Width, () => ViewportTarget.Height);

            TurnManager.PlayerTurnEnded += Player.FinishTurn;
            TurnManager.PlayerTurnEnded += Sidekick.ConsiderTips;

            TurnManager.EnemyTurnCame += CreatureManager.MakeCreaturesThink;
            TurnManager.EnemyTurnEnded += CreatureManager.EndCreaturesTurn;

            InputManager.SingleKeyPress += GameStateManager.HandleInput;
            InputManager.SingleKeyPress += CheatConsole.ActivationHandler;
            InputManager.SingleKeyPress += CheatShortcuts.HandleKeyInput;
            InputManager.SingleKeyPress += k =>
            {
                if(k == Keys.NumPad0)
                {
                    Options.TileSize = Options.DefaultUnitSize;
                }
            };
            InputManager.SingleKeyPress += Player.HandleKeyInput;

            InputManager.MouseMove += GameStateManager.CheckIfMouseOverViewport;
            InputManager.LMBClick += GameStateManager.CheckIfLMBClickInViewport;
            InputManager.RMBClick += GameStateManager.CheckIfRMBClickInViewport;
            InputManager.LMBRelease += Sidekick.DeselectItem;

            GameStateManager.MouseOverViewport += TileInteractionManager.HandleMouseHover;
            GameStateManager.LMBClickInViewport += TileInteractionManager.HandleLMBClick;
            GameStateManager.RMBClickInViewport += TileInteractionManager.HandleRMBClick;

            TileInteractionManager.TileHover += CheatShortcuts.HandleTileHover;
            TileInteractionManager.TileClick += CheatShortcuts.HandleTileClick;

            TileInteractionManager.TileRightClick += ContextMenuManager.ShowContextMenu;

            CreatureManager.CreatureRequestsMovement += TurnManager.HandleCreatureMovementRequest;

            TurnManager.SomeoneIsReadyToMove += TileInteractionManager.CheckMovementPosibility;

            TileInteractionManager.AcceptedMovement += MovementManager.BeginCreatureMovement;

            TileInteractionManager.RunIntoCollision += CreatureManager.CheckCreatureInteraction;
            TileInteractionManager.RunIntoCollision += StructureManager.CheckStructureInteraction;
            TileInteractionManager.RunIntoCollision += Player.CheckStopMouseMovement;

            CreatureManager.CreatureFinishedMovement += ItemManager.CheckItemInteraction;

            Inventory.ItemSelected += Sidekick.HandleItemSelection;
            Inventory.ItemReleased += Sidekick.HandleItemRelease;

            GameStarted += Sidekick.InitLate;

            Options.TileSizeChanged += CreatureManager.AdjustPositionsToTileSize;
            Options.TileSizeChanged += LightingManager.AdjustToTileSize;
            Options.TileSizeChanged += ParticleSystemManager.AdjustToTileSize;
            Options.TileSizeChanged += Sidekick.AdjustToTileSize;

            Options.SettingsApplied += CreatureManager.RefreshRenderTarget;
            Options.SettingsApplied += _viewportRenderer.RefreshRenderTargets;

            _fpsCounter = new SmartFramerate(20);

            base.Initialize();
        }

		protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            squareTexture = Content.Load<Texture2D>("fullSquareTexture");
            LogoTexture = Content.Load<Texture2D>("logo");
            MainMenuImage = Content.Load<Texture2D>("mainMenuImage");

            MyraEnvironment.Game = this;
            desktop = new Desktop();
            var rootPanel = GUI.LoadGUI(this, desktop);
            desktop.Root = rootPanel;

            Map = new Map(_spriteBatch, this);

            Map.ActiveMapSwitched += LightingManager.MapSwitchHandler;
            Map.ActiveMapSwitched += ParticleSystemManager.MapSwitchHandler;
            Map.ActiveMapSwitched += CreatureManager.MapSwitchHandler;

            Options.SettingsApplied += Map.RefreshRenderTarget;

            Options.TileSizeChanged += Map.UpdateMapRenderingOptions;

            Map.TileSize = Options.TileSize;
            Map.SetAmountTilesRendered(Camera.Viewport.Width / Map.TileSize / 2 + 2, Camera.Viewport.Height / Map.TileSize / 2 + 2);
			Map.OffsetToMiddle = new Vector2(Camera.Viewport.X + Camera.Viewport.Width / 2 - Map.TileSize / 2, Camera.Viewport.Y + Camera.Viewport.Height / 2 - Map.TileSize / 2);

            TileInteractionManager.TileHover += Map.HighlightHoveredTile;
            TileInteractionManager.TileClick += Map.InteractWithMapObjects;
            TileInteractionManager.TileClick += Player.MoveByMouse;
            TileInteractionManager.TileItemReleased += Map.HandleItemRelease;

            TilesheetManager.LoadMapSpritesheet();
            TilesheetManager.LoadCreatureSpritesheet();

            Options.TilePropGrid = (PropertyGrid)desktop.Root.FindWidgetById("TilePropertyGrid");
            Options.TilePropGridScroll = (ScrollViewer)desktop.Root.FindWidgetById("TilePropertGridScroll");

            Options.ChangeResolution(Options.ResolutionList[0], null);
            Options.ApplyChanges();

            SoundManager.LoadSounds("Music", "Sounds");
            MediaPlayer.IsRepeating = true;

            SoundManager.PlayMusic("Title_screen");

            _loadingAnimationTimer = new Timer(500);
            _loadingAnimationTimer.Elapsed += (s, a) =>
            {
                var loadingTextLabel = desktop.Root.FindWidgetById("loadingTextLabel") as Label;
                if(loadingTextLabel.Text.EndsWith("..."))
                {
                    loadingTextLabel.Text = "Loading";
                }
                else
                {
                    loadingTextLabel.Text += ".";
                }
            };

            _mouseSparkle = new PSSparkles(Vector2.Zero, 10, squareTexture, 500, 40, 5, 750, Color.Yellow, false, this);

            Sidekick.Init();
        }

        private void LoadGameContent()
        {
            _firstGameLoad = false;
            ItemLoader.LoadItems(Path.Combine(Content.RootDirectory, "Items/item_spreadsheet.xml"));
            CreatureLoader.LoadCreatures(Path.Combine(Content.RootDirectory, "Creatures/creatures_spreadsheet.xml"), this);
            StructureLoader.LoadStructures(Path.Combine(Content.RootDirectory, "Structures/structures_spreadsheet.xml"), this);

            LightingManager.LightSourceMask = Content.Load<Texture2D>("lightSource");
            LightingManager.LightingMaskEffect = Content.Load<Effect>("Shaders/lightingMask");

            Sidekick.Sprite = Content.Load<Texture2D>("sidekick");

            fireParticleTexture = Content.Load<Texture2D>("fireParticleTexture");
            smokeParticleTexture = Content.Load<Texture2D>("smokeParticle");

            _defaultFont = Content.Load<SpriteFont>("defaultFont");

            BasicTrivia.Load(this);

            TilesheetManager.MapSpritesheetTexture = Content.Load<Texture2D>("tilesheet2");
            TilesheetManager.CreatureSpritesheetTexture = Content.Load<Texture2D>("creatureTilesheet");

            spiritWorldEffectBackground = Content.Load<Effect>("Shaders/spiritEffectBackground");
            spiritWorldEffectForeground = Content.Load<Effect>("Shaders/spiritEffectForeground");
            distortionEffect = Content.Load<Effect>("Shaders/distortionEffect");
            
            dayNightColorMaskTexture = Content.Load<Texture2D>("DayNightCycleMask");
            LightingManager.LightingMaskEffect.Parameters["dayNightColorMask"].SetValue(dayNightColorMaskTexture);

            LightingManager.RefreshRenderTarget(GraphicsDevice, Camera);

            CustomRegionLoader.LoadRegionTemplates(Path.Combine(Content.RootDirectory, "CustomRegions"));
            WFCSampleLoader.InitSamples(Path.Combine(Content.RootDirectory, "WFCSamples"), Path.Combine(Content.RootDirectory, "wfcSamplesSpreadsheet.xml"), this);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _fpsCounter.Update(gameTime.ElapsedGameTime.TotalSeconds);

            GameStateManager.ExecuteSynchronizedActions();

            InputManager.Update();

            if(!IsGamePaused)
			{
                if (InputManager.IsHoldingKey(Keys.Add))
                    Options.TileSize += 1;
                else if (InputManager.IsHoldingKey(Keys.Subtract))
                    Options.TileSize -= 1;

                Player.Update();

                TurnManager.Update(dt);

                foreach (var sprite in TilesheetManager.MapObjectSpritesheet)
				{
                    sprite.Update(dt);
				}
                foreach (var sprite in TilesheetManager.CreatureSpritesheet)
                {
                    sprite.Update(dt);
                }

                CreatureManager.UpdateCreatures();

                Camera.Follow(Player.PossessedCreature.Position);
                Camera.Update(dt);

                Map.UpdateScroll(Player.PossessedCreature.Position);

                Map.ClearTileChanges();

                Sidekick.Update(gameTime);

                _mouseSparkle.CentralPosition = InputManager.MousePos;
                _mouseSparkle.Update(gameTime);

                ParticleSystemManager.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Options.ClearColor);

            if(!IsGamePaused)
			{
                // TODO: make it more neat
                GraphicsDevice.Clear(Options.ClearColor);

                var backgroundRender = Map.Render();

                var foregroundRender = CreatureManager.DrawCreatures(_spriteBatch);

                GraphicsDevice.SetRenderTarget(ViewportTarget);

                if (Player.IsDead)
                {
                    _spriteBatch.Begin(effect: spiritWorldEffectBackground);
                }
                else
                {
                    _spriteBatch.Begin();
                }
                _spriteBatch.Draw(backgroundRender, ViewportTarget.Bounds, Color.White);
                _spriteBatch.End();

                if (Player.IsDead)
                {
                    _spriteBatch.Begin(effect: spiritWorldEffectForeground);
                }
                else
                {
                    _spriteBatch.Begin();
                }
                _spriteBatch.Draw(foregroundRender, ViewportTarget.Bounds, Color.White);
                Sidekick.Draw(_spriteBatch);
                _spriteBatch.End();

                ParticleSystemManager.Render(_spriteBatch);

                GraphicsDevice.SetRenderTarget(null);

                GraphicsDevice.Clear(Options.ClearColor);

                if (Player.IsDead)
                {
                    _viewportRenderer.ApplyEffect(distortionEffect);
                }

                var lightingMask = LightingManager.CreateLightingMask(GraphicsDevice, _spriteBatch, Camera);
                LightingManager.LightingMaskEffect.Parameters["intensityMask"].SetValue(lightingMask.IntensityMask);
                LightingManager.LightingMaskEffect.Parameters["colorMask"].SetValue(lightingMask.ColorMask);
                LightingManager.LightingMaskEffect.Parameters["ambientLight"].SetValue(LightingManager.AmbientLight);
                LightingManager.LightingMaskEffect.Parameters["dayNightIntensity"].SetValue(LightingManager.DayNightIntensity);
                distortionEffect.Parameters["distortionScale"].SetValue(0.1f);
                distortionEffect.Parameters["animationOffset"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

                _viewportRenderer.ApplyEffect(LightingManager.LightingMaskEffect);
                    
                var postProcessTexture = _viewportRenderer.RenderTexture(_spriteBatch, ViewportTarget);

                _spriteBatch.Begin();
                _spriteBatch.Draw(postProcessTexture, ViewportTarget.Bounds.Displace(Camera.ShakeX.Value, Camera.ShakeY.Value), Color.White);
                _spriteBatch.End();

                _spriteBatch.Begin();
                _spriteBatch.DrawString(_defaultFont, TurnManager.TurnCount.ToString(), new Vector2(0, Camera.Viewport.Bottom - 20), Color.White);
                _spriteBatch.End();
            }
            else if(IsGameStarted)
			{
                _spriteBatch.Begin();
                _spriteBatch.Draw(ViewportTarget, ViewportTarget.Bounds, Color.White);
                _spriteBatch.Draw(squareTexture, new Rectangle(Point.Zero, Options.OldResolution), new Color(0, 0, 0, 0.75f));
                _spriteBatch.End();
            }

			try // some weird error popped up once so i put it in try catch block
			{
				desktop.Render();
			}
			catch (Exception) { }

			_mouseSparkle.Draw(_spriteBatch);

            if (Options.EnableFPSCounter)
			{
                _spriteBatch.Begin();
                _spriteBatch.DrawString(_defaultFont, Math.Round(_fpsCounter.Framerate).ToString(), Vector2.Zero, Color.White);
                _spriteBatch.End();
			}
        }

        public void StartGame()
		{
            IsGamePaused = true;
            IsGameStarted = false;
            desktop.Root.FindWidgetById("continueButton").Enabled = true;
            Player.IsDead = false;

            // display loading sreeen and music
            SoundManager.PlayMusic("Loading");
            desktop.Root.FindWidgetById("mainMenu").Visible = false;
            desktop.Root.FindWidgetById("loadingScreen").Visible = true;
            _loadingAnimationTimer.Start();

            //
            if(_firstGameLoad)
            {
                LoadGameContent();
            }

            //
            LightingManager.RefreshRenderTarget(GraphicsDevice, Camera);

            // load surface
            Map.SwitchActiveMap(ActiveMap.Surface);

            // first clear everything
            CreatureManager.Creatures.Clear();
            StructureManager.Structures.Clear();
            ItemManager.Items.Clear();
            LightingManager.ClearLightSources();
            ParticleSystemManager.Clear();
            MessageLog.Clear();

            //
            var mapGenerator = new MapGenerator();
            Map.Tiles = mapGenerator.GenerateMap(this, Path.Combine(Content.RootDirectory, "customRegions_spreadsheet.xml"));

            //Map.SwitchActiveMap(ActiveMap.Surface);
            Map.UpdateTileTextures();

            var hero = CreatureManager.Creatures.Single(c => c.Id == "Hero");
            Player.PossessedCreature = hero;

            //
            Sidekick.Init();

            Sidekick.DisplaySlots();
            Options.AdjustGUI();

            //CreatureManager.FirstLoadCreatures();
            Player.FinishTurn();

            GameStarted?.Invoke();

            SoundManager.PlayMusic("Main_theme");
            desktop.Root.FindWidgetById("loadingScreen").Visible = false;
            desktop.Root.FindWidgetById("gameScreen").Visible = true;
            _loadingAnimationTimer.Stop();

            IsGameStarted = true;
            IsGamePaused = false;
        }

        public void TogglePauseGame()
		{
            ToggleMainMenu();
            IsGamePaused ^= true;
		}

        private void ToggleMainMenu()
		{
            desktop.Root.FindWidgetById("mainMenu").Visible ^= true;
            desktop.Root.FindWidgetById("gameScreen").Visible ^= true;
        }
    }
}

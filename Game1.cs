using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Pandorai.Tilemaps;
using System;
using System.Diagnostics;
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
using System.Threading.Tasks;
using System.Timers;

namespace Pandorai
{
    public delegate void EmptyEventHandler();
    public delegate void InputEventHandler(CustomInput input);
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public static Game1 game;

        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Desktop desktop;

        public Options Options;

        public Camera Camera;

        public Texture2D squareTexture;
        public Texture2D smokeParticleTexture;
        Texture2D fogTexture;

        SpriteFont defaultFont;

        Timer _loadingAnimationTimer;

        public Map Map;

        public bool IsGameStarted = false;
        public bool IsGamePaused = true;

        public PlayerController Player;

        public CreatureManager CreatureManager;

        public InputManager InputManager;

        public TurnManager TurnManager;

        public GameStateManager GameStateManager;

        public Trivia BasicTrivia;

        public RenderTarget2D ViewportTarget;

        public Texture2D fireParticleTexture;

        SmartFramerate fpsCounter;

        public Effect spiritWorldEffectBackground;
        public Effect spiritWorldEffectForeground;
        public Effect distortionEffect;

        PSSparkles mouseSparkle;

        public event EmptyEventHandler GameStarted;

        public Random mainRng = new Random();

        RenderHelper viewportRenderer;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = true;
            IsMouseVisible = true;
            Window.AllowUserResizing = false;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            game = this;

            CheatConsole.game = this;
            CheatConsole.InitCommands();

            DialogueManager.game = this;

            CheatShortcuts.game = this;

            TileInteractionManager.game = this;

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
            //Console.WriteLine(Camera.Viewport);

            InputManager = new InputManager(this);

            TurnManager = new TurnManager(this);

            GameStateManager = new GameStateManager(this);

            Player = new PlayerController(this);

            BasicTrivia = new Trivia("defaultTrivia.txt");

            viewportRenderer = new RenderHelper(this, () => ViewportTarget.Width, () => ViewportTarget.Height);

            TurnManager.PlayerActionStarted += Player.StartTurn;
            TurnManager.PlayerTurnEnded += Player.FinishTurn;

            TurnManager.EnemyTurnCame += CreatureManager.MakeCreaturesThink;
            TurnManager.EnemyTurnEnded += CreatureManager.EndCreaturesTurn;

            InputManager.SingleKeyPress += GameStateManager.HandleInput;
            InputManager.SingleKeyPress += CheatConsole.ActivationHandler;
            InputManager.SingleKeyPress += CheatShortcuts.HandleKeyInput;
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
            Options.SettingsApplied += viewportRenderer.RefreshRenderTargets;

            fpsCounter = new SmartFramerate(20);

            /*InputManager.SingleKeyPress += k =>
            {
                if (k == Keys.Add)
                    Options.TileSize += 4;
                else if (k == Keys.Subtract)
                    Options.TileSize -= 4;
            };*/

            base.Initialize();
        }

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //

            MyraEnvironment.Game = this;

            desktop = new Desktop();

            var rootPanel = GUI.LoadGUI(this, desktop);

            desktop.Root = rootPanel;

            //

            ItemLoader.LoadItems(Path.Combine(Content.RootDirectory, "Items/item_spreadsheet.xml"));

            CreatureLoader.LoadCreatures(Path.Combine(Content.RootDirectory, "Creatures/creatures_spreadsheet.xml"), this);

            StructureLoader.LoadStructures(Path.Combine(Content.RootDirectory, "Structures/structures_spreadsheet.xml"), this);

            //
            squareTexture = Content.Load<Texture2D>("fullSquareTexture");
            fogTexture = Content.Load<Texture2D>("fog");
            LightingManager.LightSourceMask = Content.Load<Texture2D>("lightSource");
            LightingManager.LightingMaskEffect = Content.Load<Effect>("Shaders/lightingMask");

            Sidekick.Sprite = Content.Load<Texture2D>("sidekick");

            fireParticleTexture = Content.Load<Texture2D>("fireParticleTexture");
            smokeParticleTexture = Content.Load<Texture2D>("smokeParticle");

            defaultFont = Content.Load<SpriteFont>("defaultFont");

            BasicTrivia.Load(this);

            TilesheetManager.MapSpritesheetTexture = Content.Load<Texture2D>("tilesheet2");
            TilesheetManager.CreatureSpritesheetTexture = Content.Load<Texture2D>("creatureTilesheet");

            Map = new Map(spriteBatch, this);

            Map.ActiveMapSwitched += LightingManager.MapSwitchHandler;
            Map.ActiveMapSwitched += ParticleSystemManager.MapSwitchHandler;
            Map.ActiveMapSwitched += CreatureManager.MapSwitchHandler;

            Options.SettingsApplied += Map.RefreshRenderTarget;

            Options.TileSizeChanged += Map.UpdateMapRenderingOptions;

            TileInteractionManager.TileHover += MapTooltip.DisplayMapTooltip;

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

            spiritWorldEffectBackground = Content.Load<Effect>("Shaders/spiritEffectBackground");
            spiritWorldEffectForeground = Content.Load<Effect>("Shaders/spiritEffectForeground");
            distortionEffect = Content.Load<Effect>("Shaders/distortionEffect");

            LightingManager.RefreshRenderTarget(GraphicsDevice, Camera);

            Sidekick.Init();

            Options.ChangeResolution(Options.ResolutionList[0], null);
            Options.ApplyChanges();

            mouseSparkle = new PSSparkles(Vector2.Zero, 10, squareTexture, 500, 40, 5, 750, Color.Yellow, false, this);

            CustomRegionLoader.LoadRegionTemplates(Path.Combine(Content.RootDirectory, "CustomRegions"));
            WFCSampleLoader.InitSamples(Path.Combine(Content.RootDirectory, "WFCSamples"), Path.Combine(Content.RootDirectory, "wfcSamplesSpreadsheet.xml"), this);

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
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            fpsCounter.Update(gameTime.ElapsedGameTime.TotalSeconds);

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

                mouseSparkle.CentralPosition = InputManager.MousePos;
                mouseSparkle.Update(gameTime);

                ParticleSystemManager.Update(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Options.ClearColor);

            if(!IsGamePaused)
			{
                // TODO: make it more neat
                GraphicsDevice.Clear(Options.ClearColor);

                var backgroundRender = Map.Render();

                var foregroundRender = CreatureManager.DrawCreatures(spriteBatch);

                GraphicsDevice.SetRenderTarget(ViewportTarget);

                if (Player.IsDead)
                    spriteBatch.Begin(effect: spiritWorldEffectBackground);
                else
                    spriteBatch.Begin();
                spriteBatch.Draw(backgroundRender, ViewportTarget.Bounds, Color.White);
                spriteBatch.End();

                if (Player.IsDead)
                    spriteBatch.Begin(effect: spiritWorldEffectForeground);
                else
                    spriteBatch.Begin();
                spriteBatch.Draw(foregroundRender, ViewportTarget.Bounds, Color.White);
                Sidekick.Draw(spriteBatch);
                spriteBatch.End();

                ParticleSystemManager.Render(spriteBatch);

                GraphicsDevice.SetRenderTarget(null);

                GraphicsDevice.Clear(Options.ClearColor);

                var lightingMask = LightingManager.CreateLightingMask(GraphicsDevice, spriteBatch, Camera);
                LightingManager.LightingMaskEffect.Parameters["intensityMask"].SetValue(lightingMask.IntensityMask);
                LightingManager.LightingMaskEffect.Parameters["colorMask"].SetValue(lightingMask.ColorMask);
                LightingManager.LightingMaskEffect.Parameters["ambientLight"].SetValue(LightingManager.AmbientLight);
                distortionEffect.Parameters["distortionScale"].SetValue(0.1f);
                distortionEffect.Parameters["animationOffset"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

                viewportRenderer.ApplyEffect(LightingManager.LightingMaskEffect);
                if (Player.IsDead)
                    viewportRenderer.ApplyEffect(distortionEffect);
                var postProcessTexture = viewportRenderer.RenderTexture(spriteBatch, ViewportTarget);

                spriteBatch.Begin();
                spriteBatch.Draw(postProcessTexture, ViewportTarget.Bounds.Displace(Camera.ShakeX.Value, Camera.ShakeY.Value), Color.White);
                spriteBatch.End();
            }
            else if(IsGameStarted)
			{
                spriteBatch.Begin();
                spriteBatch.Draw(ViewportTarget, ViewportTarget.Bounds, Color.White);
                spriteBatch.Draw(squareTexture, new Rectangle(Point.Zero, Options.oldResolution), new Color(0, 0, 0, 0.75f));
                spriteBatch.End();
            }

			try // some weird error popped up once so i put it in try catch block
			{
				desktop.Render();
			}
			catch (Exception) { }

			mouseSparkle.Draw(spriteBatch);

            if (Options.enableFPSCounter)
			{
                spriteBatch.Begin();
                spriteBatch.DrawString(defaultFont, Math.Round(fpsCounter.framerate).ToString(), Vector2.Zero, Color.White);
                spriteBatch.End();
			}

            spriteBatch.Begin();
            spriteBatch.DrawString(defaultFont, TurnManager.TurnCount.ToString(), new Vector2(0, Camera.Viewport.Bottom - 20), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public async Task StartGame()
		{
            IsGamePaused = true;
            desktop.Root.FindWidgetById("continueButton").Enabled = true;
            Player.IsDead = false;

            //
            LightingManager.RefreshRenderTarget(GraphicsDevice, Camera);

            // load surface
            Map.SwitchActiveMap(ActiveMap.Surface);

            CreatureManager.Creatures.Clear(); // first clear everything
            LightingManager.ClearLightSources();
            ParticleSystemManager.Clear();

            // display loading sreeen and music
            SoundManager.PlayMusic("Loading");
            desktop.Root.FindWidgetById("mainMenu").Visible = false;
            desktop.Root.FindWidgetById("loadingScreen").Visible = true;
            _loadingAnimationTimer.Start();

            //
            var mapGenerator = new MapGenerator();
            Map.Tiles = await mapGenerator.GenerateMapAsync(this, Path.Combine(Content.RootDirectory, "customRegions_spreadsheet.xml"));

            //Map.SwitchActiveMap(ActiveMap.Surface);
            Map.UpdateTileTextures();

            var hero = CreatureLoader.GetCreature("Hero");
            Player.PossessedCreature = hero;
            CreatureManager.AddCreature(hero);

            //
            Sidekick.Init();

            Sidekick.DisplaySlots();
            Options.AdjustGUI();

            CreatureManager.EndCreaturesTurn();

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

        void ToggleMainMenu()
		{
            desktop.Root.FindWidgetById("mainMenu").Visible ^= true;
            desktop.Root.FindWidgetById("gameScreen").Visible ^= true;
        }
    }
}

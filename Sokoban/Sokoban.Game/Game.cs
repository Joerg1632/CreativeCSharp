using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sokoban.Core;
using Sokoban.Render;
using System.Collections.Generic;
using Sokoban.Core.Engine;
using Sokoban.Core.Levels;
using Sokoban.Core.Services;
using Sokoban.Data.Enums;
using Sokoban.Data.Models;

namespace Sokoban.Game
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private const int MaxTileSize = 64;
        private const int MenuWidth = 800;
        private const int MenuHeight = 600;

        private GraphicsDeviceManager Graphics;
        private SpriteBatch SpriteBatch;

        private MouseState PrevMouse;
        private float LevelTime;

        private Renderer Renderer;
        private LevelRenderer LevelRenderer;
        private UiRenderer UiRenderer;
        private LevelSelectionRenderer LevelSelectionRenderer;

        private InputManager InputManager = new();
        private LevelManager LevelManager;
        private PlayerService PlayerService;
        private RecordService RecordService;

        private GameState GameState = GameState.StartScreen;
        private string InputName = "";
        private LevelInfo SelectedLevel;
        private Direction LastDirection = Direction.Down;
        private bool IsMoving;
        private List<LevelInfo> Levels;

        public Level CurrentLevel { get; private set; }
        public SokobanEngine Engine { get; private set; }
        public bool LevelCompleted { get; private set; }

        public Game()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Graphics.PreferredBackBufferWidth = MenuWidth;
            Graphics.PreferredBackBufferHeight = MenuHeight;
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            PlayerService = new PlayerService();
            RecordService = new RecordService();
            LevelManager = new LevelManager(PlayerService, RecordService);

            Levels = LevelManager.GetLevels();

            var contentLoader = new ContentLoader(Content);
            var tileTextures = contentLoader.LoadTileTextures();
            var playerAnimations = contentLoader.LoadPlayerAnimations();
            var font = contentLoader.LoadFont("DefaultFont");
            var smallFont = contentLoader.LoadFont("SmallFont");

            Renderer = new Renderer(SpriteBatch, font, smallFont)
            {
                TileSize = MaxTileSize,
                OffsetX = 0,
                OffsetY = 0
            };

            LevelRenderer = new LevelRenderer(Renderer, tileTextures, playerAnimations);
            UiRenderer = new UiRenderer(Renderer);
            LevelSelectionRenderer = new LevelSelectionRenderer(Renderer);

            const int thumbnailSize = 120;
            LevelSelectionRenderer.GenerateLevelThumbnails(Levels, tileTextures, thumbnailSize, thumbnailSize);
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();

            switch (GameState)
            {
                case GameState.StartScreen:
                    UpdateStartScreen(keyboard);
                    break;
                case GameState.LevelSelection:
                    UpdateLevelSelection(mouse);
                    break;
                case GameState.Playing:
                    UpdatePlaying(gameTime, keyboard);
                    break;
                case GameState.Victory:
                    UpdateVictory(keyboard);
                    break;
            }

            InputManager.Update(keyboard);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SpriteBatch.Begin();

            switch (GameState)
            {
                case GameState.StartScreen:
                    UiRenderer.DrawStartScreen(InputName);
                    break;
                case GameState.LevelSelection:
                    LevelSelectionRenderer.DrawLevelSelection(
                        Levels, 
                        PlayerService.Profile, 
                        RecordService.GetAll(),
                        Mouse.GetState(), 
                        PrevMouse);
                    break;
                case GameState.Playing:
                    DrawPlaying();
                    break;
                case GameState.Victory:
                    DrawVictoryScreen();
                    break;
            }

            SpriteBatch.End();
            base.Draw(gameTime);
        }

        private void UpdateStartScreen(KeyboardState keyboard)
        {
            InputName = InputManager.HandleNameInput(keyboard, InputName);
            if (keyboard.IsKeyDown(Keys.Enter) && InputName.Length > 0)
            {
                PlayerService.Profile.PlayerName = InputName;
                PlayerService.Save();
                GameState = GameState.LevelSelection;
            }
        }

        private void UpdateLevelSelection(MouseState mouse)
        {
            SelectedLevel = LevelSelectionRenderer.HandleSelection(Levels, mouse, PrevMouse);
            if (SelectedLevel != null)
                LoadLevel(SelectedLevel.Path);

            PrevMouse = mouse;
        }

        private void UpdatePlaying(GameTime gameTime, KeyboardState keyboard)
        {
            LevelTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            InputManager.HandlePlayerInput(keyboard, Engine, ref LastDirection, out IsMoving);

            LevelRenderer.SetPlayerDirection(LastDirection);
            LevelRenderer.UpdatePlayerAnimation(gameTime, IsMoving);

            if (Engine.IsLevelCompleted())
            {
                LevelCompleted = true;
                GameState = GameState.Victory;
                LevelManager.SaveLevelResult(SelectedLevel.Path, Engine, LevelTime);
            }
            
            if (InputManager.IsKeyPressed(Keys.R, keyboard))
                LoadLevel(SelectedLevel.Path);

            if (InputManager.IsKeyPressed(Keys.Escape, keyboard))
                GoToMenu();
        }

        private void UpdateVictory(KeyboardState keyboard)
        {
            if (InputManager.IsKeyPressed(Keys.Escape, keyboard))
                GoToMenu();
            if (InputManager.IsKeyPressed(Keys.R, keyboard))
                LoadLevel(SelectedLevel.Path);
        }

        private void DrawPlaying()
        {
            LevelRenderer.DrawLevel(CurrentLevel);
            LevelRenderer.DrawPlayer(CurrentLevel);
            UiRenderer.DrawHud(PlayerService.Profile.PlayerName, Engine.Steps, LevelTime);
        }

        private void DrawVictoryScreen()
        {
            UiRenderer.DrawVictoryScreen(
                Engine.Steps,     
                LevelTime,
                "\nR - Restart\nESC - Level Selection"
            );
        }
        
        private void LoadLevel(string path)
        {
            (Engine, CurrentLevel) = LevelManager.LoadLevel(path);
            LevelCompleted = false;
            LevelTime = 0f;

            SelectedLevel = Levels.Find(l => l.Path == path);

            Graphics.PreferredBackBufferWidth = CurrentLevel.Width * MaxTileSize;
            Graphics.PreferredBackBufferHeight = CurrentLevel.Height * MaxTileSize;
            Graphics.ApplyChanges();

            Renderer.TileSize = MaxTileSize;
            GameState = GameState.Playing;
        }

        private void GoToMenu()
        {
            Graphics.PreferredBackBufferWidth = MenuWidth;
            Graphics.PreferredBackBufferHeight = MenuHeight;
            Graphics.ApplyChanges();

            CurrentLevel = null;
            Engine = null;
            SelectedLevel = null;
            GameState = GameState.LevelSelection;
        }
    }
}
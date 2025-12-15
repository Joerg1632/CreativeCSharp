using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sokoban.Core;
using Sokoban.Render;
using Sokoban.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sokoban.Game
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private const int MaxTileSize = 64;
        private const int MenuWidth = 800;
        private const int MenuHeight = 600;

        private GraphicsDeviceManager Graphics;
        private SpriteBatch SpriteBatch;
        private KeyboardState PreviousKeyboardState;
        private MouseState prevMouse;
        private float levelTime;

        private Renderer renderer;
        private Direction lastDirection = Direction.Down;
        private bool isMoving;
        private GameState gameState = GameState.StartScreen;
        private string inputName = "";
        private string CurrentLevelPath;

        private readonly List<LevelInfo> Levels = new()
        {
            new LevelInfo { Id = "easy", Name = "Easy", Path = @"Content\levels\easy.txt" },
            new LevelInfo { Id = "middle", Name = "Middle", Path = @"Content\levels\middle.txt" },
            new LevelInfo { Id = "hard", Name = "Hard", Path = @"Content\levels\hard.txt" }
        };

        private ContentLoader contentLoader;
        private PlayerService playerService;
        private RecordService recordService;

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

            contentLoader = new ContentLoader(Content);
            playerService = new PlayerService();
            recordService = new RecordService();

            var tileTextures = contentLoader.LoadTileTextures();
            var playerAnimations = contentLoader.LoadPlayerAnimations();
            var defaultFont = contentLoader.LoadFont("DefaultFont");
            var smallFont = contentLoader.LoadFont("SmallFont");

            renderer = new Renderer(SpriteBatch, tileTextures, playerAnimations, defaultFont, smallFont)
            {
                TileSize = MaxTileSize,
                OffsetX = 0,
                OffsetY = 0
            };

            renderer.GenerateLevelThumbnails(Levels, 120, 120);
        }

        protected override void Update(GameTime gameTime)
        {
            var state = Keyboard.GetState();

            switch (gameState)
            {
                case GameState.StartScreen:
                    HandleNameInput(state);
                    break;

                case GameState.LevelSelection:
                    break;

                case GameState.Playing:
                    levelTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    HandlePlayerInput(state, gameTime);
                    break;

                case GameState.Victory:
                    if (state.IsKeyDown(Keys.Escape) && !PreviousKeyboardState.IsKeyDown(Keys.Escape))
                        GoToMenu();

                    if (state.IsKeyDown(Keys.R) && !PreviousKeyboardState.IsKeyDown(Keys.R)
                        && !string.IsNullOrEmpty(CurrentLevelPath))
                        LoadLevel(CurrentLevelPath);
                    break;
            }

            PreviousKeyboardState = state;
            base.Update(gameTime);
        }

        private void HandleNameInput(KeyboardState state)
        {
            foreach (var key in state.GetPressedKeys())
            {
                if (PreviousKeyboardState.IsKeyUp(key))
                {
                    if (key == Keys.Back && inputName.Length > 0)
                        inputName = inputName[..^1];
                    else if (key == Keys.Enter && inputName.Length > 0)
                    {
                        playerService.Profile.PlayerName = inputName;
                        playerService.Save();
                        gameState = GameState.LevelSelection;
                    }
                    else
                    {
                        string s = KeyToChar(key, state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift));
                        if (!string.IsNullOrEmpty(s) && inputName.Length < 12)
                            inputName += s;
                    }
                }
            }
        }

        private string KeyToChar(Keys key, bool shift)
        {
            if (key >= Keys.A && key <= Keys.Z)
                return shift ? key.ToString() : key.ToString().ToLower();
            if (key >= Keys.D0 && key <= Keys.D9)
                return ((char)('0' + (key - Keys.D0))).ToString();
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
                return ((char)('0' + (key - Keys.NumPad0))).ToString();
            if (key == Keys.Space)
                return " ";
            return "";
        }

        private void LoadLevel(string path)
        {
            CurrentLevel = LevelLoader.LoadFromFile(path);
            Engine = new SokobanEngine(CurrentLevel);
            LevelCompleted = false;
            levelTime = 0f;
            CurrentLevelPath = path;

            Graphics.PreferredBackBufferWidth = CurrentLevel.Width * MaxTileSize;
            Graphics.PreferredBackBufferHeight = CurrentLevel.Height * MaxTileSize;
            Graphics.ApplyChanges();

            renderer.TileSize = MaxTileSize;
            gameState = GameState.Playing;
        }

        private void HandlePlayerInput(KeyboardState state, GameTime gameTime)
        {
            isMoving = false;

            if (state.IsKeyDown(Keys.Up))
                MovePlayer(Direction.Up, Keys.Up);
            else if (state.IsKeyDown(Keys.Down))
                MovePlayer(Direction.Down, Keys.Down);
            else if (state.IsKeyDown(Keys.Left))
                MovePlayer(Direction.Left, Keys.Left);
            else if (state.IsKeyDown(Keys.Right))
                MovePlayer(Direction.Right, Keys.Right);

            renderer.SetPlayerDirection(lastDirection);
            renderer.UpdatePlayerAnimation(gameTime, isMoving);

            if (Engine.IsLevelCompleted())
            {
                LevelCompleted = true;
                gameState = GameState.Victory;
                SaveLevelResult();
            }
        }

        private void MovePlayer(Direction dir, Keys key)
        {
            lastDirection = dir;
            isMoving = true;

            if (!PreviousKeyboardState.IsKeyDown(key))
                Engine.MovePlayer(dir);
        }

        private void SaveLevelResult()
        {
            var levelId = Path.GetFileNameWithoutExtension(CurrentLevelPath);

            recordService.TryUpdateRecord(levelId, playerService.Profile.PlayerName, Engine.Steps, levelTime);

            playerService.UpdateLevelStats(levelId, Engine.Steps, levelTime);
        }

        private void GoToMenu()
        {
            Graphics.PreferredBackBufferWidth = MenuWidth;
            Graphics.PreferredBackBufferHeight = MenuHeight;
            Graphics.ApplyChanges();

            CurrentLevel = null;
            Engine = null;
            gameState = GameState.LevelSelection;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SpriteBatch.Begin();

            switch (gameState)
            {
                case GameState.StartScreen:
                    renderer.DrawStartScreen(inputName);
                    break;

                case GameState.LevelSelection:
                    var mouse = Mouse.GetState();
                    var selected = renderer.DrawLevelSelection(Levels, playerService.Profile, recordService.GetAll(), mouse, prevMouse);

                    if (selected != null)
                        LoadLevel(selected.Path);

                    prevMouse = mouse;
                    break;

                case GameState.Playing:
                    renderer.DrawLevel(CurrentLevel);
                    renderer.DrawPlayer(CurrentLevel);
                    renderer.DrawHud(playerService.Profile.PlayerName, Engine.Steps, levelTime);
                    break;

                case GameState.Victory:
                    renderer.DrawLevel(CurrentLevel);
                    renderer.DrawPlayer(CurrentLevel);
                    renderer.DrawHud(playerService.Profile.PlayerName, Engine.Steps, levelTime);

                    var levelId = Path.GetFileName(CurrentLevelPath);
                    var result = playerService.GetLevelStats(levelId);
                    renderer.DrawVictoryScreen(result, extraHint: "R - Restart\nESC - Level Selection");
                    break;
            }

            SpriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

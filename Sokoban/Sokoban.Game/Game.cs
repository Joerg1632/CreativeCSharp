using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sokoban.Core;
using Sokoban.Render;
using System.Collections.Generic;

namespace Sokoban.Game;
public class Game : Microsoft.Xna.Framework.Game
{
    private const int MaxTileSize = 64;
    private const int ScreenPadding = 100;

    private GraphicsDeviceManager Graphics;
    private SpriteBatch SpriteBatch;
    private KeyboardState PreviousKeyboardState;

    public Level CurrentLevel { get; private set; }
    public SokobanEngine Engine { get; private set; }
    public bool LevelCompleted { get; private set; }

    private Renderer renderer;
    private string CurrentLevelPath = @"D:\JetBrains Rider 2025.2.3\RiderProjects\Sokoban\Sokoban.Game\Content\levels\biiigLevel.txt";

    private Dictionary<TileType, Texture2D> TileTextures;
    private Dictionary<Direction, AnimatedSprite> PlayerAnimations;
    private Direction lastDirection = Direction.Down;
    private bool isMoving;

    public Game()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        LoadLevel(CurrentLevelPath);
        LoadTextures();
        LoadPlayerAnimations();
        SetupScreen();

        renderer = new Renderer(SpriteBatch, TileTextures, PlayerAnimations, Content.Load<SpriteFont>("DefaultFont"))
        {
            TileSize = CalculateTileSize(),
            OffsetX = 0,
            OffsetY = 0
        };
    }

    private void LoadLevel(string path)
    {
        CurrentLevel = LevelLoader.LoadFromFile(path);
        Engine = new SokobanEngine(CurrentLevel);
        LevelCompleted = false;
    }

    private void LoadTextures()
    {
        TileTextures = new Dictionary<TileType, Texture2D>
        {
            { TileType.Empty, Content.Load<Texture2D>("floor") },
            { TileType.Wall, Content.Load<Texture2D>("wall") },
            { TileType.Box, Content.Load<Texture2D>("box") },
            { TileType.Goal, Content.Load<Texture2D>("goal") },
            { TileType.BoxOnGoal, Content.Load<Texture2D>("box_on_goal") }
        };
    }

    private void LoadPlayerAnimations()
    {
        PlayerAnimations = new Dictionary<Direction, AnimatedSprite>
        {
            { Direction.Up, LoadAnimation("Player/up") },
            { Direction.Down, LoadAnimation("Player/down") },
            { Direction.Left, LoadAnimation("Player/left") },
            { Direction.Right, LoadAnimation("Player/right") }
        };
    }

    private AnimatedSprite LoadAnimation(string path)
    {
        return new AnimatedSprite(new[]
        {
            Content.Load<Texture2D>($"{path}_1"),
            Content.Load<Texture2D>($"{path}_2"),
            Content.Load<Texture2D>($"{path}_3")
        });
    }

    private void SetupScreen()
    {
        Graphics.PreferredBackBufferWidth = CurrentLevel.Width * CalculateTileSize();
        Graphics.PreferredBackBufferHeight = CurrentLevel.Height * CalculateTileSize();
        Graphics.ApplyChanges();
    }

    private int CalculateTileSize()
    {
        int screenWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width - ScreenPadding;
        int screenHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height - ScreenPadding;
        int size = System.Math.Min(screenWidth / CurrentLevel.Width, screenHeight / CurrentLevel.Height);
        return System.Math.Min(size, MaxTileSize);
    }

    protected override void Update(GameTime gameTime)
    {
        var state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.R) && !PreviousKeyboardState.IsKeyDown(Keys.R))
            LoadLevel(CurrentLevelPath);

        if (!LevelCompleted)
            HandlePlayerInput(state, gameTime);

        PreviousKeyboardState = state;
        base.Update(gameTime);
    }

    private void HandlePlayerInput(KeyboardState state, GameTime gameTime)
    {
        isMoving = false;

        if (state.IsKeyDown(Keys.Up)) 
            MovePlayer(Direction.Up, state, Keys.Up);
        else if (state.IsKeyDown(Keys.Down)) 
            MovePlayer(Direction.Down, state, Keys.Down);
        else if (state.IsKeyDown(Keys.Left)) 
            MovePlayer(Direction.Left, state, Keys.Left);
        else if (state.IsKeyDown(Keys.Right)) 
            MovePlayer(Direction.Right, state, Keys.Right);

        renderer.SetPlayerDirection(lastDirection);
        renderer.UpdatePlayerAnimation(gameTime, isMoving);

        if (Engine.IsLevelCompleted())
            LevelCompleted = true;
    }

    private void MovePlayer(Direction dir, KeyboardState state, Keys key)
    {
        lastDirection = dir;
        isMoving = true;

        if (!PreviousKeyboardState.IsKeyDown(key))
            Engine.MovePlayer(dir);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
        SpriteBatch.Begin();

        renderer.DrawLevel(CurrentLevel);
        renderer.DrawPlayer(CurrentLevel);
        renderer.DrawVictoryText(LevelCompleted);

        SpriteBatch.End();
        base.Draw(gameTime);
    }
}


using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sokoban.Core;

namespace Sokoban.Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private const int MaxTileSize = 64;
    private const int ScreenPadding = 100;
    
    private GraphicsDeviceManager Graphics;
    private SpriteBatch SpriteBatch;
    private KeyboardState PreviousKeyboardState;

    public SpriteFont Font { get; private set; }
    public bool LevelCompleted { get; private set; }

    public Level CurrentLevel { get; private set; }
    public SokobanEngine Engine { get; private set; }

    public int TileSize { get; private set; }
    public int OffsetX { get; private set; }
    public int OffsetY { get; private set; }

    private Dictionary<TileType, Texture2D> TileTextures;
    private Dictionary<Direction, AnimatedSprite> PlayerAnimations;
    private AnimatedSprite CurrentPlayerAnim;

    private Direction lastDirection = Direction.Down;
    private bool isMoving;

    private string CurrentLevelPath = @"D:\JetBrains Rider 2025.2.3\RiderProjects\Sokoban\Sokoban.Game\Content\levels\biiigLevel.txt";
    
    public Game1()
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
        
        Font = Content.Load<SpriteFont>("DefaultFont");
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

        CurrentPlayerAnim  = PlayerAnimations[Direction.Down];
    }

    private AnimatedSprite LoadAnimation(string path)
    {
        return new AnimatedSprite(new[]
        {
            Content.Load<Texture2D>($"{path}_1"),
            Content.Load<Texture2D>($"{path}_2"),
            Content.Load<Texture2D>($"{path}_3"),
        });
    }

    private void SetupScreen()
    {
        int screenWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width - ScreenPadding;
        int screenHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height - ScreenPadding;

        TileSize = Math.Min(screenWidth / CurrentLevel.Width, screenHeight / CurrentLevel.Height);
        TileSize = Math.Min(TileSize, MaxTileSize);

        Graphics.PreferredBackBufferWidth = CurrentLevel.Width * TileSize;
        Graphics.PreferredBackBufferHeight = CurrentLevel.Height * TileSize;
        Graphics.ApplyChanges();

        OffsetX = 0;
        OffsetY = 0;
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

        CurrentPlayerAnim?.Update(gameTime, isMoving);

        if (Engine.IsLevelCompleted())
            LevelCompleted = true;
    }

    private void MovePlayer(Direction dir, KeyboardState state, Keys key)
    {
        if (!PlayerAnimations.ContainsKey(dir))
            return;

        CurrentPlayerAnim = PlayerAnimations[dir];
        lastDirection = dir;
        isMoving = true;

        if (!PreviousKeyboardState.IsKeyDown(key))
            Engine.MovePlayer(dir);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        SpriteBatch.Begin();

        DrawMap();
        DrawPlayer();
        DrawVictoryText();

        SpriteBatch.End();
        base.Draw(gameTime);
    }
    
    private void DrawMap()
    {
        for (var y = 0; y < CurrentLevel.Height; y++) 
        {
            for (var x = 0; x < CurrentLevel.Width; x++)
            {
                var tile = CurrentLevel.Map[y, x];
                var isGoal = CurrentLevel.Goals[y, x] == TileType.Goal;

                Texture2D baseTex;
                if (tile == TileType.Wall)
                    baseTex = TileTextures[TileType.Wall];
                else if (isGoal)
                    baseTex = TileTextures[TileType.Goal];
                else
                    baseTex = TileTextures[TileType.Empty];

                SpriteBatch.Draw(baseTex,
                    new Rectangle(OffsetX + x * TileSize, OffsetY + y * TileSize, TileSize, TileSize),
                    Color.White);

                if (tile == TileType.Box)
                {
                    var boxTexToDraw = isGoal ? TileTextures[TileType.BoxOnGoal] : TileTextures[TileType.Box];
                    SpriteBatch.Draw(boxTexToDraw,
                        new Rectangle(OffsetX + x * TileSize, OffsetY + y * TileSize, TileSize, TileSize),
                        Color.White);
                }
            }
        }
    }

    private void DrawPlayer()
    {
        var (px, py) = CurrentLevel.PlayerPosition;
        SpriteBatch.Draw(CurrentPlayerAnim.CurrentFrame, 
            new Rectangle(OffsetX + px * TileSize, OffsetY + py * TileSize, TileSize, TileSize), 
            Color.White);
    }

    private void DrawVictoryText()
    {
        if (!LevelCompleted) 
            return;

        var victoryText = "VICTORY!";
        var textSize = Font.MeasureString(victoryText);
        var position = new Vector2(
            (Graphics.PreferredBackBufferWidth - textSize.X) / 2,
            (Graphics.PreferredBackBufferHeight - textSize.Y) / 2
        );

        SpriteBatch.DrawString(Font, victoryText, position, Color.DarkRed);
    }
}
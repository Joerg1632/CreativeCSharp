using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sokoban.Core;

namespace Sokoban.Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private KeyboardState _previousKeyboardState;
    
    private SpriteFont font;
    private bool levelCompleted;
    
    private Level level;
    private SokobanEngine engine;
    private Texture2D pixelTexture;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        level = LevelLoader.LoadFromFile("D:\\JetBrains Rider 2025.2.3\\RiderProjects\\Sokoban\\Sokoban.Game\\Content\\levels\\level1.txt");
        engine = new SokobanEngine(level);

        pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });

        font = Content.Load<SpriteFont>("DefaultFont");
    }

    protected override void Update(GameTime gameTime)
    {
        var state = Keyboard.GetState();

        if (!levelCompleted)
        {
            if (state.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up))
                engine.MovePlayer(Direction.Up);
            else if (state.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.Down))
                engine.MovePlayer(Direction.Down);
            else if (state.IsKeyDown(Keys.Left) && !_previousKeyboardState.IsKeyDown(Keys.Left))
                engine.MovePlayer(Direction.Left);
            else if (state.IsKeyDown(Keys.Right) && !_previousKeyboardState.IsKeyDown(Keys.Right))
                engine.MovePlayer(Direction.Right);

            if (engine.IsLevelCompleted())
                levelCompleted = true;
        }

        _previousKeyboardState = state;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        for (int y = 0; y < level.Height; y++)
        {
            for (int x = 0; x < level.Width; x++)
            {
                var tile = level.Map[y, x];
                Color color = tile switch
                {
                    TileType.Empty => Color.Gray,
                    TileType.Wall => Color.DarkGray,
                    TileType.Box => Color.Brown,
                    TileType.Goal => Color.Yellow,
                    TileType.Player => Color.Green,
                    _ => Color.Magenta
                };

                _spriteBatch.Draw(pixelTexture, new Rectangle(x * 32, y * 32, 32, 32), color);
            }
        }

        if (levelCompleted)
            _spriteBatch.DrawString(font, "VICTORY!", new Vector2(100, 100), Color.Red);

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
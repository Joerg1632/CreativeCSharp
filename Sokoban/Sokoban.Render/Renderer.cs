using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sokoban.Core;
using Sokoban.Game; 
using Sokoban.Render;

namespace Sokoban.Render;
public class Renderer
{
    private const string victoryText = "VICTORY!";
    public SpriteBatch SpriteBatch { get; }
    public Dictionary<TileType, Texture2D> TileTextures { get; }
    public Dictionary<Direction, AnimatedSprite> PlayerAnimations { get; }
    public AnimatedSprite CurrentPlayerAnim { get; private set; }
    public SpriteFont Font { get; }

    public int TileSize { get; set; }
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }

    public Renderer(SpriteBatch spriteBatch,
                    Dictionary<TileType, Texture2D> tileTextures,
                    Dictionary<Direction, AnimatedSprite> playerAnimations,
                    SpriteFont font)
    {
        SpriteBatch = spriteBatch;
        TileTextures = tileTextures;
        PlayerAnimations = playerAnimations;
        CurrentPlayerAnim = playerAnimations[Direction.Down];
        Font = font;
    }

    public void SetPlayerDirection(Direction dir)
    {
        if (PlayerAnimations.ContainsKey(dir))
            CurrentPlayerAnim = PlayerAnimations[dir];
    }

    public void UpdatePlayerAnimation(GameTime gameTime, bool isMoving)
    {
        CurrentPlayerAnim?.Update(gameTime, isMoving);
    }

    public void DrawLevel(Level level)
    {
        for (int y = 0; y < level.Height; y++)
        {
            for (int x = 0; x < level.Width; x++)
            {
                var tile = level.Map[y, x];
                var isGoal = level.Goals[y, x] == TileType.Goal;
                Texture2D baseTex = tile == TileType.Wall
                    ? TileTextures[TileType.Wall]
                    : isGoal
                        ? TileTextures[TileType.Goal]
                        : TileTextures[TileType.Empty];

                SpriteBatch.Draw(baseTex,
                    new Rectangle(OffsetX + x * TileSize, OffsetY + y * TileSize, TileSize, TileSize),
                    Color.White);

                if (tile == TileType.Box)
                {
                    var boxTex = isGoal ? TileTextures[TileType.BoxOnGoal] : TileTextures[TileType.Box];
                    SpriteBatch.Draw(boxTex,
                        new Rectangle(OffsetX + x * TileSize, OffsetY + y * TileSize, TileSize, TileSize),
                        Color.White);
                }
            }
        }
    }

    public void DrawPlayer(Level level)
    {
        var (px, py) = level.PlayerPosition;
        SpriteBatch.Draw(CurrentPlayerAnim.CurrentFrame,
            new Rectangle(OffsetX + px * TileSize, OffsetY + py * TileSize, TileSize, TileSize),
            Color.White);
    }

    public void DrawVictoryText(bool levelCompleted)
    {
        if (!levelCompleted) 
            return;

        var size = Font.MeasureString(victoryText);
        var position = new Vector2((SpriteBatch.GraphicsDevice.PresentationParameters.BackBufferWidth - size.X) / 2,
                                   (SpriteBatch.GraphicsDevice.PresentationParameters.BackBufferHeight - size.Y) / 2);
        SpriteBatch.DrawString(Font, victoryText, position, Color.DarkRed);
    }
}


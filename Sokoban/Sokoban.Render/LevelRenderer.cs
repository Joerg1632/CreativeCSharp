using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sokoban.Core;
using System.Collections.Generic;
using Sokoban.Core.Engine;
using Sokoban.Core.Levels;
using Sokoban.Data;
using Sokoban.Data.Enums;

namespace Sokoban.Render;

public class LevelRenderer
{
    private readonly Renderer Renderer;
    private readonly Dictionary<TileType, Texture2D> Tiles;
    private readonly Dictionary<Direction, AnimatedSprite> Animations;
    private AnimatedSprite Current;

    public LevelRenderer(
        Renderer renderer,
        Dictionary<TileType, Texture2D> tileTextures,
        Dictionary<Direction, AnimatedSprite> playerAnimations)
    {
        Renderer = renderer;
        Tiles = tileTextures;
        Animations = playerAnimations;
        Current = Animations[Direction.Down];
    }

    public void SetPlayerDirection(Direction dir)
    {
        if (Animations.ContainsKey(dir))
            Current = Animations[dir];
    }

    public void UpdatePlayerAnimation(GameTime time, bool isMoving)
    {
        Current.Update(time, isMoving);
    }

    public void DrawLevel(Level level)
    {
        for (int y = 0; y < level.Height; y++)
        for (int x = 0; x < level.Width; x++)
        {
            var tile = level.Map[y, x];
            var isGoal = level.Goals[y, x] == TileType.Goal;

            Texture2D baseTex =
                tile == TileType.Wall ? Tiles[TileType.Wall] :
                isGoal ? Tiles[TileType.Goal] :
                Tiles[TileType.Empty];

            Renderer.SpriteBatch.Draw(
                baseTex,
                new Rectangle(
                    Renderer.OffsetX + x * Renderer.TileSize,
                    Renderer.OffsetY + y * Renderer.TileSize,
                    Renderer.TileSize,
                    Renderer.TileSize),
                Color.White);

            if (tile == TileType.Box)
            {
                var boxTex = isGoal ? Tiles[TileType.BoxOnGoal] : Tiles[TileType.Box];
                Renderer.SpriteBatch.Draw(
                    boxTex,
                    new Rectangle(
                        Renderer.OffsetX + x * Renderer.TileSize,
                        Renderer.OffsetY + y * Renderer.TileSize,
                        Renderer.TileSize,
                        Renderer.TileSize),
                    Color.White);
            }
        }
    }

    public void DrawPlayer(Level level)
    {
        var (px, py) = level.PlayerPosition;
        Renderer.SpriteBatch.Draw(
            Current.CurrentFrame,
            new Rectangle(
                Renderer.OffsetX + px * Renderer.TileSize,
                Renderer.OffsetY + py * Renderer.TileSize,
                Renderer.TileSize,
                Renderer.TileSize),
            Color.White);
    }
}
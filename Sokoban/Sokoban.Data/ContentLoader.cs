using Microsoft.Xna.Framework;        
using Microsoft.Xna.Framework.Content; 
using Microsoft.Xna.Framework.Graphics; 
using Sokoban.Core;                     
using System.Collections.Generic;

namespace Sokoban.Data;

public class ContentLoader
{
    private ContentManager Content;

    public ContentLoader(ContentManager Content)
    {
        this.Content = Content;
    }

    public Dictionary<TileType, Texture2D> LoadTileTextures()
    {
        return new Dictionary<TileType, Texture2D>
        {
            { TileType.Empty, Content.Load<Texture2D>("floor") },
            { TileType.Wall, Content.Load<Texture2D>("wall") },
            { TileType.Box, Content.Load<Texture2D>("box") },
            { TileType.Goal, Content.Load<Texture2D>("goal") },
            { TileType.BoxOnGoal, Content.Load<Texture2D>("box_on_goal") }
        };
    }

    public Dictionary<Direction, AnimatedSprite> LoadPlayerAnimations()
    {
        return new Dictionary<Direction, AnimatedSprite>
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

    public SpriteFont LoadFont(string name) => Content.Load<SpriteFont>(name);
}
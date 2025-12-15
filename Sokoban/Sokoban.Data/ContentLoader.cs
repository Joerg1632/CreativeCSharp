using Microsoft.Xna.Framework;        
using Microsoft.Xna.Framework.Content; 
using Microsoft.Xna.Framework.Graphics; 
using Sokoban.Core;                     
using System.Collections.Generic;

namespace Sokoban.Data;

public class ContentLoader
{
    private ContentManager content;

    public ContentLoader(ContentManager content)
    {
        this.content = content;
    }

    public Dictionary<TileType, Texture2D> LoadTileTextures()
    {
        return new Dictionary<TileType, Texture2D>
        {
            { TileType.Empty, content.Load<Texture2D>("floor") },
            { TileType.Wall, content.Load<Texture2D>("wall") },
            { TileType.Box, content.Load<Texture2D>("box") },
            { TileType.Goal, content.Load<Texture2D>("goal") },
            { TileType.BoxOnGoal, content.Load<Texture2D>("box_on_goal") }
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
            content.Load<Texture2D>($"{path}_1"),
            content.Load<Texture2D>($"{path}_2"),
            content.Load<Texture2D>($"{path}_3")
        });
    }

    public SpriteFont LoadFont(string name) => content.Load<SpriteFont>(name);
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sokoban.Core;
using Sokoban.Core.Levels;
using Sokoban.Data;
using Sokoban.Data.Enums;
using Sokoban.Data.Models;

namespace Sokoban.Render;

public class LevelSelectionRenderer
{
    private readonly Renderer Renderer;
    private readonly Dictionary<LevelInfo, Texture2D> Thumbs = new();

    private const int CardWidth = 560;
    private const int CardHeight = 96;
    private const int ThumbSize = 64;
    private const int CardPadding = 12;
    private const int TextOffsetX = 90;
    private const int TextOffsetY = 10;
    private const int TextGap = 4;
    private const int LineSpacingExtra = 2;
    private const int RecordOffsetX = 230;
    private const int RecordOffsetY = 30;
    private const int NameOffsetX = 20;

    private const float CardGap = 130f;
    private const float StartY = 150f;
    private const float StartX = 150f;

    public LevelSelectionRenderer(Renderer renderer)
    {
        Renderer = renderer;
    }

    public void GenerateLevelThumbnails(
        List<LevelInfo> levels,
        Dictionary<TileType, Texture2D> tiles,
        int width, int height)
    {
        Thumbs.Clear();

        foreach (var info in levels)
        {
            var level = LevelLoader.LoadFromFile(info.Path);
            var rt = new RenderTarget2D(Renderer.SpriteBatch.GraphicsDevice, width, height);

            Renderer.SpriteBatch.GraphicsDevice.SetRenderTarget(rt);
            Renderer.SpriteBatch.GraphicsDevice.Clear(Color.DarkGray);
            Renderer.SpriteBatch.Begin();

            var cellWidth = (float)width / level.Width;
            var cellHeight = (float)height / level.Height;

            for (var y = 0; y < level.Height; y++)
            for (var x = 0; x < level.Width; x++)
            {
                var tile = level.Map[y, x];
                var isGoal = level.Goals[y, x] == TileType.Goal;

                var dest = new Rectangle(
                    (int)(x * cellWidth),
                    (int)(y * cellHeight),
                    (int)cellWidth,
                    (int)cellHeight
                );

                Texture2D baseTex =
                    tile == TileType.Wall ? tiles[TileType.Wall] :
                    isGoal ? tiles[TileType.Goal] :
                    tiles[TileType.Empty];

                Renderer.SpriteBatch.Draw(baseTex, dest, Color.White);

                if (tile == TileType.Box)
                {
                    var boxTex = isGoal ? tiles[TileType.BoxOnGoal] : tiles[TileType.Box];
                    Renderer.SpriteBatch.Draw(boxTex, dest, Color.White);
                }
            }

            Renderer.SpriteBatch.End();
            Renderer.SpriteBatch.GraphicsDevice.SetRenderTarget(null);
            Thumbs[info] = rt;
        }
    }

    public LevelInfo HandleSelection(
        List<LevelInfo> levels,
        MouseState mouse,
        MouseState prevMouse)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            var rect = new Rectangle((int)StartX, (int)(StartY + i * CardGap), CardWidth, CardHeight);
            if (rect.Contains(mouse.Position) 
                && mouse.LeftButton == ButtonState.Pressed 
                && prevMouse.LeftButton == ButtonState.Released)
            {
                return levels[i];
            }
        }
        return null;
    }

    public LevelInfo DrawLevelSelection(
        List<LevelInfo> levels,
        PlayerProfile profile,
        IReadOnlyDictionary<string, LevelRecord> records,
        MouseState mouse,
        MouseState prevMouse)
    {
        for (var i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            var y = StartY + i * CardGap;

            var rect = new Rectangle((int)StartX, (int)y, CardWidth, CardHeight);
            var hover = rect.Contains(mouse.Position);

            Renderer.SpriteBatch.Draw(
                Renderer.GetPixel(),
                rect,
                hover ? Color.DarkSlateBlue : Color.Black * 0.4f
            );

            if (Thumbs.TryGetValue(level, out var thumb))
            {
                Renderer.SpriteBatch.Draw(
                    thumb,
                    new Rectangle(rect.X + CardPadding, 
                        rect.Y + (CardHeight - ThumbSize) / 2, 
                        ThumbSize, 
                        ThumbSize),
                    Color.White
                );
            }

            var textX = rect.X + TextOffsetX;
            var lineY = rect.Y + TextOffsetY;

            Renderer.SpriteBatch.DrawString(Renderer.Font, 
                level.Name, 
                new Vector2(textX, lineY + NameOffsetX), 
                Color.White);
            
            lineY += Renderer.Font.LineSpacing + TextGap;

            var personal = profile.CompletedLevels.FirstOrDefault(s => s.LevelId == level.Id);
            if (personal != null)
            {
                Renderer.SpriteBatch.DrawString(
                    Renderer.SmallFont,
                    $"Your     best:     {personal.Steps}     steps,     {personal.TimeSeconds:F1}s",
                    new Vector2(textX+ RecordOffsetX, lineY-RecordOffsetY),
                    Color.Aquamarine
                );
                lineY += Renderer.SmallFont.LineSpacing + LineSpacingExtra;
            }

            if (records.TryGetValue(level.Id, out var record))
            {
                Renderer.SpriteBatch.DrawString(
                    Renderer.SmallFont,
                    $"Best     player:     {record.PlayerName}",
                    new Vector2(textX+ RecordOffsetX, lineY-RecordOffsetY),
                    Color.Gold
                );
                lineY += Renderer.SmallFont.LineSpacing + LineSpacingExtra;

                Renderer.SpriteBatch.DrawString(
                    Renderer.SmallFont,
                    $"Steps:     {record.Steps},     Time:     {record.TimeSeconds:F1}s",
                    new Vector2(textX+ RecordOffsetX, lineY-RecordOffsetY),
                    Color.Gold
                );
                lineY += Renderer.SmallFont.LineSpacing + LineSpacingExtra;
            }
            else
            {
                Renderer.SpriteBatch.DrawString(
                    Renderer.SmallFont,
                    "No     record     yet",
                    new Vector2(textX+ RecordOffsetX, lineY-RecordOffsetY),
                    Color.Gray
                );
                lineY += Renderer.SmallFont.LineSpacing + LineSpacingExtra;
            }

            if (hover 
                && mouse.LeftButton == ButtonState.Pressed 
                && prevMouse.LeftButton == ButtonState.Released)
            {
                return level;
            }
        }

        Renderer.DrawCenteredText("Select a level", new Vector2(400, 50), Color.LightGray);
        return null;
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sokoban.Core;

namespace Sokoban.Render;

public class Renderer
{
    private SpriteBatch SpriteBatch;
    private Dictionary<TileType, Texture2D> TileTextures;
    private Dictionary<Direction, AnimatedSprite> PlayerAnimations;
    private AnimatedSprite CurrentPlayerAnim;
    private SpriteFont Font;
    private Texture2D pixel;
    private SpriteFont SmallFont;
    public Dictionary<LevelInfo, Texture2D> LevelThumbnails { get; } = new();


    public int TileSize { get; set; }
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
    
    public Renderer(SpriteBatch spriteBatch,
        Dictionary<TileType, Texture2D> tileTextures,
        Dictionary<Direction, AnimatedSprite> playerAnimations,
        SpriteFont font,
        SpriteFont smallFont)
    {
        SpriteBatch = spriteBatch;
        TileTextures = tileTextures;
        PlayerAnimations = playerAnimations;
        CurrentPlayerAnim = playerAnimations[Direction.Down];
        Font = font;
        SmallFont = smallFont;
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
                Texture2D tex = tile == TileType.Wall ? TileTextures[TileType.Wall]
                    : isGoal ? TileTextures[TileType.Goal]
                    : TileTextures[TileType.Empty];

                SpriteBatch.Draw(tex,
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

    public void DrawStartScreen(string name)
    {
        float centerX = 800 / 2f;
        float centerY = 600 / 2f;

        DrawCenteredText("Enter your name:", new Vector2(centerX, 100), Color.White);

        var nameText = name + "_";
        DrawCenteredText(nameText, new Vector2(centerX, 140), Color.Yellow);

        DrawCenteredText("Press Enter to start", new Vector2(centerX, 180), Color.DarkSlateBlue);
    }

    public void DrawHud(string player, int steps, float time)
    {
        var viewport = SpriteBatch.GraphicsDevice.Viewport;

        var lines = new[]
        {
            $"Player: {player}",
            $"Steps: {steps}",
            $"Time: {time:F1}s"
        };

        float y = 5;
        foreach (var line in lines)
        {
            var size = SmallFont.MeasureString(line);
            SpriteBatch.DrawString(
                SmallFont,
                line,
                new Vector2(viewport.Width - size.X - 5, y),
                Color.White * 0.9f
            );
            y += size.Y + 2;
        }
    }

    public void DrawVictoryScreen(LevelStats result, string extraHint = null)
    {
        var viewport = SpriteBatch.GraphicsDevice.Viewport;
        var center = new Vector2(viewport.Width / 2f, viewport.Height / 2f);

        SpriteBatch.Draw(GetPixel(), new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * 0.6f);

        var panelWidth = 420;
        var panelHeight = 280;
        var panelRect = new Rectangle((int)(center.X - panelWidth / 2), (int)(center.Y - panelHeight / 2), panelWidth, panelHeight);
        SpriteBatch.Draw(GetPixel(), panelRect, Color.DarkSlateBlue);

        float offsetY = -80;
        DrawCenteredText("LEVEL COMPLETED!", center + new Vector2(0, offsetY), Color.Gold);

        offsetY += Font.MeasureString("LEVEL COMPLETED!").Y + 20;
        DrawCenteredText($"Steps: {result.Steps}", center + new Vector2(0, offsetY), Color.White);

        offsetY += Font.MeasureString($"Steps: {result.Steps}").Y + 10;
        DrawCenteredText($"Time: {result.TimeSeconds:F1}s", center + new Vector2(0, offsetY), Color.White);

        if (!string.IsNullOrEmpty(extraHint))
        {
            offsetY += Font.MeasureString($"Time: {result.TimeSeconds:F1}s").Y + 20;
            var hints = extraHint.Split('\n');
            foreach (var hint in hints)
            {
                DrawCenteredText(hint, center + new Vector2(0, offsetY), Color.LightGray);
                offsetY += Font.MeasureString(hint).Y + 3; 
            }
        }
    }

    public LevelInfo DrawLevelSelection(
        List<LevelInfo> levels,
        PlayerProfile profile,
        Dictionary<string, LevelRecord> records,
        MouseState mouse,
        MouseState prevMouse)
    {
        float startY = 150;
        float x = 180;
        float gap = 100;

        for (int i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            float y = startY + i * gap;

            var rect = new Rectangle((int)x, (int)y, 420, 80);
            bool hover = rect.Contains(mouse.Position);

            SpriteBatch.Draw(GetPixel(), rect,
                hover ? Color.DarkSlateBlue : Color.Black * 0.4f);

            if (LevelThumbnails.TryGetValue(level, out var thumb))
            {
                SpriteBatch.Draw(thumb,
                    new Rectangle(rect.X + 10, rect.Y + 10, 40, 40),
                    Color.White);
            }

            SpriteBatch.DrawString(Font, level.Name, new Vector2(rect.X + 60, rect.Y + 10), Color.White);

            float lineY = rect.Y + 10; 

            var stats = profile.CompletedLevels.FirstOrDefault(s => s.LevelId == level.Id);
            if (stats != null)
            {
                SpriteBatch.DrawString(
                    SmallFont,
                    $"Your     best:     {stats.Steps}     steps,     {stats.TimeSeconds:F1}s",
                    new Vector2(rect.X + 210, lineY),
                    Color.Yellow
                );
                lineY += SmallFont.MeasureString($"Your     best:     {stats.Steps} steps,     {stats.TimeSeconds:F1}s").Y + 2;
            }

            if (records.TryGetValue(level.Id, out var record))
            {
                SpriteBatch.DrawString(
                    SmallFont,
                    $"Best     player:     {record.PlayerName}",
                    new Vector2(rect.X + 210, lineY),
                    Color.LightGreen
                );
                lineY += SmallFont.MeasureString($"Best     player:     {record.PlayerName}").Y + 2;

                SpriteBatch.DrawString(
                    SmallFont,
                    $"Steps:     {record.Steps}, Time:     {record.TimeSeconds:F1}s",
                    new Vector2(rect.X + 210, lineY),
                    Color.Gold
                );
                lineY += SmallFont.MeasureString($"Steps:     {record.Steps}, Time:     {record.TimeSeconds:F1}s").Y + 2;
            }
            else
            {
                SpriteBatch.DrawString(
                    SmallFont,
                    "No     record     yet",
                    new Vector2(rect.X + 210, lineY),
                    Color.Gray
                );
            }

            if (hover &&
                mouse.LeftButton == ButtonState.Pressed &&
                prevMouse.LeftButton == ButtonState.Released)
            {
                return level;
            }
        }

        DrawCenteredText("Select a level", new Vector2(400, 500), Color.LightGray);
        return null;
    }

    public void GenerateLevelThumbnails(List<LevelInfo> levels, int width, int height)
    {
        LevelThumbnails.Clear();

        foreach (var levelInfo in levels)
        {
            var level = LevelLoader.LoadFromFile(levelInfo.Path);
            var tex = new RenderTarget2D(SpriteBatch.GraphicsDevice, width, height);

            SpriteBatch.GraphicsDevice.SetRenderTarget(tex);
            SpriteBatch.GraphicsDevice.Clear(Color.DarkGray);

            SpriteBatch.Begin();

            Console.WriteLine((float)width + " "+ level.Width);
            float cellWidth = (float)width / level.Width;
            float cellHeight = (float)height / level.Height;

            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    var tile = level.Map[y, x];
                    var isGoal = level.Goals[y, x] == TileType.Goal;

                    Texture2D baseTex =
                        tile == TileType.Wall ? TileTextures[TileType.Wall] :
                        isGoal ? TileTextures[TileType.Goal] :
                        TileTextures[TileType.Empty];

                    SpriteBatch.Draw(
                        baseTex,
                        new Rectangle((int)(x * cellWidth), (int)(y * cellHeight), (int)cellWidth, (int)cellHeight),
                        Color.White
                    );

                    if (tile == TileType.Box)
                    {
                        var boxTex = isGoal ? TileTextures[TileType.BoxOnGoal] : TileTextures[TileType.Box];
                        SpriteBatch.Draw(
                            boxTex,
                            new Rectangle((int)(x * cellWidth), (int)(y * cellHeight), (int)cellWidth, (int)cellHeight),
                            Color.White
                        );
                    }
                }
            }

            SpriteBatch.End();
            SpriteBatch.GraphicsDevice.SetRenderTarget(null);

            LevelThumbnails[levelInfo] = tex;
        }
    }

    
    private Texture2D GetPixel()
    {
        if (pixel != null) 
            return pixel;
        
        pixel = new Texture2D(SpriteBatch.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
        return pixel;
    }

    private void DrawCenteredText(string text, Vector2 center, Color color)
    {
        var size = Font.MeasureString(text);
        SpriteBatch.DrawString(Font, text, center - size / 2, color);
    }
}
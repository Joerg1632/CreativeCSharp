using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sokoban.Render;

public class Renderer
{
    public SpriteBatch SpriteBatch { get; }
    public SpriteFont Font { get; }
    public SpriteFont SmallFont { get; }

    private Texture2D Pixel;

    public int TileSize { get; set; }
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }

    public Renderer(SpriteBatch spriteBatch, SpriteFont font, SpriteFont smallFont)
    {
        SpriteBatch = spriteBatch;
        Font = font;
        SmallFont = smallFont;
    }

    public Texture2D GetPixel()
    {
        if (Pixel != null)
            return Pixel;

        Pixel = new Texture2D(SpriteBatch.GraphicsDevice, 1, 1);
        Pixel.SetData(new[] { Color.White });
        return Pixel;
    }

    public void DrawCenteredText(string text, Vector2 center, Color color)
    {
        var size = Font.MeasureString(text);
        SpriteBatch.DrawString(Font, text, center - size / 2, color);
    }
}
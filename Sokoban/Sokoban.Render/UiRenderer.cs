using Microsoft.Xna.Framework;
using Sokoban.Core;

namespace Sokoban.Render;

public class UiRenderer
{
    private readonly Renderer Renderer;

    private readonly Vector2 StartScreenTitlePos = new(400, 100);
    private readonly Vector2 StartScreenNamePos = new(400, 140);
    private readonly Vector2 StartScreenHintPos = new(400, 180);

    private readonly Color StartScreenTitleColor = Color.White;
    private readonly Color StartScreenNameColor = Color.Yellow;
    private readonly Color StartScreenHintColor = Color.DarkSlateBlue;

    private readonly int HudMargin = 5;
    private readonly int HudLineSpacingExtra = 2;

    private readonly Color HudTextColor = Color.White;

    private readonly Color VictoryOverlayColor = Color.Black * 0.6f;
    private readonly Vector2 VictoryTitleOffset = new(0, -80);
    private readonly Vector2 VictoryStepsOffset = new(0, 0);
    private readonly Vector2 VictoryTimeOffset = new(0, 40);
    private readonly Vector2 VictoryHintOffset = new(0, 90);

    private readonly Color VictoryTitleColor = Color.Gold;
    private readonly Color VictoryTextColor = Color.White;
    private readonly Color VictoryHintColor = Color.LightGray;

    public UiRenderer(Renderer renderer)
    {
        Renderer = renderer;
    }

    public void DrawStartScreen(string name)
    {
        Renderer.DrawCenteredText("Enter your name:", StartScreenTitlePos, StartScreenTitleColor);
        Renderer.DrawCenteredText(name + "_", StartScreenNamePos, StartScreenNameColor);
        Renderer.DrawCenteredText("Press Enter to start", StartScreenHintPos, StartScreenHintColor);
    }

    public void DrawHud(string player, int steps, float time)
    {
        var y = HudMargin;
        foreach (var line in new[]
                 {
                     $"Player: {player}",
                     $"Steps: {steps}",
                     $"Time: {time:F1}s"
                 })
        {
            var x = Renderer.SpriteBatch.GraphicsDevice.Viewport.Width - Renderer.SmallFont.MeasureString(line).X - HudMargin;
            Renderer.SpriteBatch.DrawString(Renderer.SmallFont, line, new Vector2(x, y), HudTextColor);
            y += Renderer.SmallFont.LineSpacing + HudLineSpacingExtra;
        }
    }

    public void DrawVictoryScreen(LevelStats stats, string hint)
    {
        var vp = Renderer.SpriteBatch.GraphicsDevice.Viewport;
        var center = new Vector2(vp.Width / 2f, vp.Height / 2f);

        Renderer.SpriteBatch.Draw(Renderer.GetPixel(), new Rectangle(0, 0, vp.Width, vp.Height), VictoryOverlayColor);
        Renderer.DrawCenteredText("LEVEL COMPLETED!", center + VictoryTitleOffset, VictoryTitleColor);
        Renderer.DrawCenteredText($"Steps: {stats.Steps}", center + VictoryStepsOffset, VictoryTextColor);
        Renderer.DrawCenteredText($"Time: {stats.TimeSeconds:F1}s", center + VictoryTimeOffset, VictoryTextColor);
        Renderer.DrawCenteredText(hint, center + VictoryHintOffset, VictoryHintColor);
    }
}

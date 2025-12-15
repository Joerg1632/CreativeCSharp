using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sokoban.Core;

public class AnimatedSprite
{
    public Texture2D[] Frames { get; private set; }
    public int CurrentFrameIndex { get; private set; }
    public float FrameTime { get; private set; }
    private float Timer;

    public AnimatedSprite(Texture2D[] frames, float frameTime = 0.15f)
    {
        Frames = frames;
        FrameTime = frameTime;
        CurrentFrameIndex = 0;
        Timer = 0f;
    }

    public void Update(GameTime gameTime, bool isMoving)
    {
        if (!isMoving)
        {
            CurrentFrameIndex = 0;
            Timer = 0f;
            return;
        }

        Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (Timer >= FrameTime)
        {
            CurrentFrameIndex = (CurrentFrameIndex + 1) % Frames.Length;
            Timer = 0f;
        }
    }

    public Texture2D CurrentFrame => Frames[CurrentFrameIndex];
}
using Microsoft.Xna.Framework.Input;
using Sokoban.Core;

namespace Sokoban.Game;

public class InputManager
{
    private KeyboardState previousState;

    public void UpdateState(KeyboardState currentState)
    {
        previousState = currentState;
    }

    public bool IsKeyPressed(Keys key, KeyboardState currentState)
        => currentState.IsKeyDown(key) && !previousState.IsKeyDown(key);

    public string HandleNameInput(KeyboardState state, string currentName)
    {
        foreach (var key in state.GetPressedKeys())
        {
            if (!previousState.IsKeyDown(key))
            {
                if (key == Keys.Back && currentName.Length > 0)
                    currentName = currentName[..^1];
                else if (key == Keys.Space)
                    currentName += " ";
                else if (key >= Keys.A && key <= Keys.Z)
                    currentName += state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift) 
                        ? key.ToString() 
                        : key.ToString().ToLower();
                else if (key >= Keys.D0 && key <= Keys.D9)
                    currentName += (char)('0' + (key - Keys.D0));
                else if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
                    currentName += (char)('0' + (key - Keys.NumPad0));
            }
        }

        return currentName.Length > 12 ? currentName[..12] : currentName;
    }
}
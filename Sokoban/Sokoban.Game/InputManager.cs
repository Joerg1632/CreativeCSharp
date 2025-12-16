using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sokoban.Core.Engine;

namespace Sokoban.Game
{
    public class InputManager
    {
        private KeyboardState PreviousState;

        public void Update(KeyboardState currentState)
        {
            PreviousState = currentState;
        }

        public bool IsKeyPressed(Keys key, KeyboardState currentState)
        {
            return currentState.IsKeyDown(key) && !PreviousState.IsKeyDown(key);
        }
        
        public string HandleNameInput(KeyboardState state, string currentName)
        {
            foreach (var key in state.GetPressedKeys())
            {
                if (!PreviousState.IsKeyDown(key))
                {
                    if (key == Keys.Back && currentName.Length > 0)
                        currentName = currentName[..^1];
                    else if (key == Keys.Space)
                        currentName += " ";
                    else
                    {
                        string s = KeyToChar(key, state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift));
                        if (!string.IsNullOrEmpty(s) && currentName.Length < 12)
                            currentName += s;
                    }
                }
            }
            return currentName;
        }

        private string KeyToChar(Keys key, bool shift)
        {
            if (key >= Keys.A && key <= Keys.Z)
                return shift ? key.ToString() : key.ToString().ToLower();
            if (key >= Keys.D0 && key <= Keys.D9)
                return ((char)('0' + (key - Keys.D0))).ToString();
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
                return ((char)('0' + (key - Keys.NumPad0))).ToString();
            return "";
        }

        public void HandlePlayerInput(
            KeyboardState state,
            SokobanEngine engine,
            ref Direction lastDirection,
            out bool isMoving)
        {
            isMoving = false;

            if (engine == null)
                return;

            if (state.IsKeyDown(Keys.Up))
                MovePlayer(Direction.Up, Keys.Up, engine, ref lastDirection, ref isMoving);
            else if (state.IsKeyDown(Keys.Down))
                MovePlayer(Direction.Down, Keys.Down, engine, ref lastDirection, ref isMoving);
            else if (state.IsKeyDown(Keys.Left))
                MovePlayer(Direction.Left, Keys.Left, engine, ref lastDirection, ref isMoving);
            else if (state.IsKeyDown(Keys.Right))
                MovePlayer(Direction.Right, Keys.Right, engine, ref lastDirection, ref isMoving);
        }

        private void MovePlayer(Direction dir, 
            Keys key, 
            SokobanEngine engine, 
            ref Direction lastDirection, 
            ref bool isMoving)
        {
            lastDirection = dir;
            isMoving = true;

            if (!PreviousState.IsKeyDown(key))
                engine.MovePlayer(dir);
        }
    }
}

namespace Sokoban.Core;

public class SokobanEngine
{
    public Level CurrentLevel { get; private set; }

    public SokobanEngine(Level level)
    {
        CurrentLevel = level;
    }

    public void MovePlayer(Direction dir)
    {
        var (px, py) = CurrentLevel.PlayerPosition;
        var nx = px;
        var ny = py;

        switch (dir)
        {
            case Direction.Up: ny--; 
                break;
            case Direction.Down: ny++;
                break;
            case Direction.Left: nx--; 
                break;
            case Direction.Right: nx++; 
                break;
        }

        if (ny < 0 || ny >= CurrentLevel.Height || nx < 0 || nx >= CurrentLevel.Width)
            return;

        var targetTile = CurrentLevel.Map[ny, nx];
        if (targetTile == TileType.Wall) 
            return;

        if (targetTile == TileType.Box)
        {
            int bx = nx, by = ny;
            switch (dir)
            {
                case Direction.Up: by--; 
                    break;
                case Direction.Down: by++; 
                    break;
                case Direction.Left: bx--; 
                    break;
                case Direction.Right: bx++; 
                    break;
            }

            if (by < 0 || by >= CurrentLevel.Height || bx < 0 || bx >= CurrentLevel.Width)
                return;

            var nextTile = CurrentLevel.Map[by, bx];
            if (nextTile == TileType.Empty || nextTile == TileType.Goal)
            {
                CurrentLevel.Map[by, bx] = TileType.Box;
                CurrentLevel.Map[ny, nx] = TileType.Player;
                CurrentLevel.Map[py, px] = TileType.Empty;
                CurrentLevel.PlayerPosition = (nx, ny);
            }
            return;
        }

        if (targetTile == TileType.Empty || targetTile == TileType.Goal)
        {
            CurrentLevel.Map[ny, nx] = TileType.Player;
            CurrentLevel.Map[py, px] = TileType.Empty;
            CurrentLevel.PlayerPosition = (nx, ny);
        }
    }
    
    public bool IsLevelCompleted()
    {
        for (var y = 0; y < CurrentLevel.Height; y++)
        {
            for (var x = 0; x < CurrentLevel.Width; x++)
            {
                if (CurrentLevel.Goals[y, x] == TileType.Goal && CurrentLevel.Map[y, x] != TileType.Box)
                    return false;
            }
        }
        return true;
    }

    private bool IsGoal(int x, int y)
    {
        return CurrentLevel.Map[y, x] == TileType.Goal;
    }
}
namespace Sokoban.Core;

public class SokobanEngine
{
    public Level CurrentLevel { get; private set; }
    
    public int Steps { get; private set; }

    public SokobanEngine(Level level)
    {
        CurrentLevel = level;
    }

    public void MovePlayer(Direction dir)
    {
        var (px, py) = CurrentLevel.PlayerPosition;
        var (nx, ny) = GetNextPosition(px, py, dir);

        if (!IsInsideBounds(nx, ny)) 
            return;

        var targetTile = CurrentLevel.Map[ny, nx];

        if (targetTile == TileType.Wall)
            return;

        if (targetTile == TileType.Box)
        {
            var (bx, by) = GetNextPosition(nx, ny, dir);
            if (!IsInsideBounds(bx, by))
                return;

            var nextTile = CurrentLevel.Map[by, bx];
            if (nextTile == TileType.Empty || nextTile == TileType.Goal)
            {
                MoveTile(nx, ny, bx, by, TileType.Box);
                MovePlayerTo(nx, ny, px, py);
            }
            
            return;
        }

        if (targetTile == TileType.Empty || targetTile == TileType.Goal)
        {
            MovePlayerTo(nx, ny, px, py);
        }
        
        Steps++;
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

    private (int x, int y) GetNextPosition(int x, int y, Direction dir) => dir switch
    {
        Direction.Up => (x, y - 1),
        Direction.Down => (x, y + 1),
        Direction.Left => (x - 1, y),
        Direction.Right => (x + 1, y),
        _ => (x, y)
    };

    private bool IsInsideBounds(int x, int y)
    {
        return x >= 0 && x < CurrentLevel.Width && y >= 0 && y < CurrentLevel.Height;
    }
        

    private void MovePlayerTo(int nx, int ny, int px, int py)
    {
        CurrentLevel.Map[ny, nx] = TileType.Player;
        CurrentLevel.Map[py, px] = CurrentLevel.Goals[py, px] == TileType.Goal ? TileType.Goal : TileType.Empty;
        CurrentLevel.PlayerPosition = (nx, ny);
    }

    private void MoveTile(int fromX, int fromY, int toX, int toY, TileType tileType)
    {
        CurrentLevel.Map[toY, toX] = tileType;
        CurrentLevel.Map[fromY, fromX] = CurrentLevel.Goals[fromY, fromX] == TileType.Goal ? TileType.Goal : TileType.Empty;
    }
}
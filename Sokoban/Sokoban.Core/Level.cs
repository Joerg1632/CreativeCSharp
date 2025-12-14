namespace Sokoban.Core;

public class Level
{
    public TileType[,] Map { get; private set; }
    public TileType[,] Goals { get; private set; }
    public int Width => Map.GetLength(1);
    public int Height => Map.GetLength(0);
    public (int X, int Y) PlayerPosition { get; set; }

    public Level(TileType[,] map)
    {
        Map = new TileType[map.GetLength(0), map.GetLength(1)];
        Goals = new TileType[map.GetLength(0), map.GetLength(1)];

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                Map[y, x] = map[y, x];
                Goals[y, x] = IsGoalTile(map[y, x]) ? TileType.Goal : TileType.Empty;
            }
        }
    }
    
    public Level Clone()
    {
        var newMap = new TileType[Height, Width];
        Array.Copy(Map, newMap, Map.Length);

        var clone = new Level(newMap)
        {
            PlayerPosition = this.PlayerPosition
        };
        return clone;
    }
    
    private static bool IsGoalTile(TileType tile) => tile == TileType.Goal;
}
namespace Sokoban.Core;

public static class LevelLoader
{
    public static Level LoadFromFile(string path)
    {
        var lines = File.ReadAllLines(path);
        var height = lines.Length;
        var width = lines[0].Length;
        TileType[,] map = new TileType[height, width];
        (int X, int Y) playerPos = (0,0);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                switch (lines[y][x])
                {
                    case '#': map[y, x] = TileType.Wall; 
                        break;
                    case '$': map[y, x] = TileType.Box; 
                        break;
                    case '.': map[y, x] = TileType.Goal; 
                        break;
                    case '@':
                        map[y, x] = TileType.Player;
                        playerPos = (x, y);
                        break;
                    default: map[y, x] = TileType.Empty; 
                        break;
                }
            }
        }

        var level = new Level(map);
        level.PlayerPosition = playerPos;
        return level;
    }
}
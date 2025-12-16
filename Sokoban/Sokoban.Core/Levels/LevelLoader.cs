using Sokoban.Core.Levels;
using Sokoban.Data;
using Sokoban.Data.Enums;

namespace Sokoban.Core.Levels;

public static class LevelLoader
{
    private static readonly Dictionary<char, TileType> TileMapping = new()
    {
        ['#'] = TileType.Wall,
        ['$'] = TileType.Box,
        ['.'] = TileType.Goal,
        ['@'] = TileType.Player
    };

    public static Level LoadFromFile(string path)
    {
        var lines = File.ReadAllLines(path);
        var height = lines.Length;
        var width = lines[0].Length;

        var map = new TileType[height, width];
        (int X, int Y) playerPos = (0, 0);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var c = lines[y][x];

                if (!TileMapping.TryGetValue(c, out var tile))
                    tile = TileType.Empty;

                map[y, x] = tile;

                if (tile == TileType.Player)
                    playerPos = (x, y);
            }
        }

        var level = new Level(map)
        {
            PlayerPosition = playerPos
        };
        return level;
    }
}
namespace Sokoban.Core;

public class LevelRecord
{
    public string LevelId { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public int Steps { get; set; }
    public float TimeSeconds { get; set; }
}
namespace Sokoban.Core;

public class PlayerProfile
{
    public string PlayerName { get; set; } = "Player";

    public List<LevelStats> CompletedLevels { get; set; } = new();

    public int BestSteps { get; set; } = int.MaxValue;
    public float BestTime { get; set; } = float.MaxValue;
}
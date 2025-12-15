using Sokoban.Core;

namespace Sokoban.Data;

public class PlayerService
{
    public PlayerProfile Profile { get; private set; }

    public PlayerService()
    {
        Profile = SaveService.Load() ?? new PlayerProfile();
    }

    public void Save() => SaveService.Save(Profile);

    public LevelStats GetLevelStats(string levelId) =>
        Profile.CompletedLevels.FirstOrDefault(l => l.LevelId == levelId);

    public void UpdateLevelStats(string levelId, int steps, float time)
    {
        Profile.CompletedLevels.RemoveAll(l => l.LevelId == levelId);
        Profile.CompletedLevels.Add(new LevelStats { LevelId = levelId, Steps = steps, TimeSeconds = time });
        Save();
    }
}

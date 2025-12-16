using Sokoban.Data;
using Sokoban.Data.Models;

namespace Sokoban.Core.Services;

public class PlayerService
{
    public PlayerProfile Profile { get; private set; }

    public PlayerService()
    {
        Profile = ProfileService.Load();
    }

    public void Save() => ProfileService.Save(Profile);
    
    public void UpdateLevelStats(string levelId, int steps, float time)
    {
        var existing = Profile.CompletedLevels.FirstOrDefault(l => l.LevelId == levelId);

        if (existing == null)
        {
            Profile.CompletedLevels.Add(new LevelStats { LevelId = levelId, Steps = steps, TimeSeconds = time });
        }
        else
        {
            if (steps < existing.Steps || (steps == existing.Steps && time < existing.TimeSeconds))
            {
                existing.Steps = steps;
                existing.TimeSeconds = time;
            }
        }

        Save();
    }
}

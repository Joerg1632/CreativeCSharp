using Sokoban.Core;

namespace Sokoban.Data;

public class RecordService
{
    private Dictionary<string, LevelRecord> Records;

    public RecordService()
    {
        Records = RecordsService.Load();
    }

    public LevelRecord GetBestRecord(string levelId)
    {
        return Records.TryGetValue(levelId, out var record) ? record : null;
    }
    
    public void TryUpdateRecord(string levelId, string playerName, int steps, float time)
    {
        if (!Records.TryGetValue(levelId, out var best) ||
            steps < best.Steps || (steps == best.Steps && time < best.TimeSeconds))
        {
            Records[levelId] = new LevelRecord
            {
                LevelId = levelId,
                PlayerName = playerName,
                Steps = steps,
                TimeSeconds = time
            };
            RecordsService.Save(Records);
        }
    }

    public Dictionary<string, LevelRecord> GetAll() => Records;
}

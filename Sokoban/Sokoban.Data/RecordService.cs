using Sokoban.Core;

namespace Sokoban.Data;

public class RecordService
{
    private Dictionary<string, LevelRecord> records;

    public RecordService()
    {
        records = RecordsService.Load();
    }

    public LevelRecord GetBestRecord(string levelId) =>
        records.TryGetValue(levelId, out var record) ? record : null;

    public void TryUpdateRecord(string levelId, string playerName, int steps, float time)
    {
        if (!records.TryGetValue(levelId, out var best) ||
            steps < best.Steps || (steps == best.Steps && time < best.TimeSeconds))
        {
            records[levelId] = new LevelRecord
            {
                LevelId = levelId,
                PlayerName = playerName,
                Steps = steps,
                TimeSeconds = time
            };
            RecordsService.Save(records);
        }
    }

    public Dictionary<string, LevelRecord> GetAll() => records;
}

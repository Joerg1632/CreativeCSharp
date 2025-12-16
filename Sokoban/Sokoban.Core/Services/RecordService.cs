using Sokoban.Data;
using Sokoban.Data.Models;

namespace Sokoban.Core.Services;

public class RecordService
{
    private const string FilePath = "records.json";
    private Dictionary<string, LevelRecord> Records;

    public RecordService()
    {
        Records = JsonStorage.Load<Dictionary<string, LevelRecord>>(FilePath);
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

            JsonStorage.Save(FilePath, Records);
        }
    }

    public IReadOnlyDictionary<string, LevelRecord> GetAll() => Records;
}
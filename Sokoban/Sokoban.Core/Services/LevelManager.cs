using Sokoban.Core.Engine;
using Sokoban.Core.Levels;
using Sokoban.Data;
using Sokoban.Data.Models;

namespace Sokoban.Core.Services;

public class LevelManager
{
    private readonly PlayerService PlayerService;
    private readonly RecordService RecordService;

    public LevelManager(PlayerService playerService, RecordService recordService)
    {
        PlayerService = playerService;
        RecordService = recordService;
    }

    public List<LevelInfo> GetLevels() => new()
    {
        new LevelInfo { Id = "easy", Name = "Easy", Path = @"Content\levels\easy.txt" },
        new LevelInfo { Id = "middle", Name = "Middle", Path = @"Content\levels\middle.txt" },
        new LevelInfo { Id = "hard", Name = "Hard", Path = @"Content\levels\hard.txt" }
    };

    public (SokobanEngine engine, Level level) LoadLevel(string path)
    {
        var level = LevelLoader.LoadFromFile(path);
        var engine = new SokobanEngine(level);
        return (engine, level);
    }

    public void SaveLevelResult(string levelPath, SokobanEngine engine, float levelTime)
    {
        var levelId = Path.GetFileNameWithoutExtension(levelPath);
        RecordService.TryUpdateRecord(levelId, PlayerService.Profile.PlayerName, engine.Steps, levelTime);
        PlayerService.UpdateLevelStats(levelId, engine.Steps, levelTime);
    }
}

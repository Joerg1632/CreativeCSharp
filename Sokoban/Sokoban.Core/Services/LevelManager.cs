using Sokoban.Core.Engine;
using Sokoban.Core.Levels;
using Sokoban.Data;
using Sokoban.Data.Models;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Sokoban.Core.Services
{
    public class LevelManager
    {
        private readonly PlayerService PlayerService;
        private readonly RecordService RecordService;
        private readonly string LevelsFolder;

        public LevelManager(PlayerService playerService, RecordService recordService, string levelsFolder = @"Content\levels")
        {
            PlayerService = playerService;
            RecordService = recordService;
            LevelsFolder = levelsFolder;
        }

        public List<LevelInfo> GetLevels()
        {
            if (!Directory.Exists(LevelsFolder))
                return new List<LevelInfo>();

            var files = Directory.GetFiles(LevelsFolder, "*.txt");
            return files.Select(f =>
            {
                var id = Path.GetFileNameWithoutExtension(f);
                var name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(id); 
                return new LevelInfo
                {
                    Id = id,
                    Name = name,
                    Path = f
                };
            }).ToList();
        }

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
}

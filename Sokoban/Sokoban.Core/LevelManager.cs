using System.Collections.Generic;

namespace Sokoban.Core
{
    public class LevelManager
    {
        public List<LevelInfo> GetLevels() => new()
        {
            new LevelInfo { Id = "easy", Name = "Easy", Path = @"Content\levels\easy.txt" },
            new LevelInfo { Id = "middle", Name = "Middle", Path = @"Content\levels\middle.txt" },
            new LevelInfo { Id = "hard", Name = "Hard", Path = @"Content\levels\hard.txt" }
        };

        public SokobanEngine CreateEngine(Level level)
        {
            return new SokobanEngine(level);
        }
    }
}
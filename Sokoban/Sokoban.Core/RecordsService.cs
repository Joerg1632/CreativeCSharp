using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Sokoban.Core
{
    public static class RecordsService
    {
        private static readonly string FilePath = "records.json";

        public static Dictionary<string, LevelRecord> Load()
        {
            if (!File.Exists(FilePath))
                return new Dictionary<string, LevelRecord>();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<Dictionary<string, LevelRecord>>(json)
                   ?? new Dictionary<string, LevelRecord>();
        }

        public static void Save(Dictionary<string, LevelRecord> records)
        {
            var json = JsonSerializer.Serialize(records, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(FilePath, json);
        }
    }
}
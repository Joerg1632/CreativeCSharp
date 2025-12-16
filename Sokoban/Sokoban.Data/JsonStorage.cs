using System.IO;
using System.Text.Json;

namespace Sokoban.Data;

public static class JsonStorage
{
    public static T Load<T>(string path) where T : new()
    {
        if (!File.Exists(path))
            return new T();

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json) ?? new T();
    }

    public static void Save<T>(string path, T data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }
}
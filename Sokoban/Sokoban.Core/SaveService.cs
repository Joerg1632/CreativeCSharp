using System.Text.Json;

namespace Sokoban.Core;

public static class SaveService
{
    public static void Save(PlayerProfile profile)
    {
        var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("profile.json", json);
    }

    public static PlayerProfile? Load()
    {
        if (!File.Exists("profile.json"))
            return new PlayerProfile();

        return JsonSerializer.Deserialize<PlayerProfile>(
            File.ReadAllText("profile.json")
        );
    }
}

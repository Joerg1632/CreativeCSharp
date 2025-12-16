using Sokoban.Data;
using Sokoban.Data.Models;

namespace Sokoban.Core.Services;

public static class ProfileService
{
    private const string FilePath = "profile.json";

    public static PlayerProfile Load()
        => JsonStorage.Load<PlayerProfile>(FilePath);

    public static void Save(PlayerProfile profile)
        => JsonStorage.Save(FilePath, profile);
}
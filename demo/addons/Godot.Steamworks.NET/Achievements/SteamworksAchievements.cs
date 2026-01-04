#if GODOT_PC
using Godot;
using Godot.Steamworks.Net;
using Godot.Steamworks.Net.Models;
using Steamworks;
using System.Collections.Generic;

namespace Godot.Steamworks.Net;
/// <summary>
/// Class for managing Steamworks achievements
/// </summary>
public partial class SteamworksAchievements : RefCounted
{
    // Cache for unlocked achievement icons
    private Dictionary<string, Texture2D> _unlockedIconCache = new Dictionary<string, Texture2D>();

    // Cache for locked achievement icons
    private Dictionary<string, Texture2D> _lockedIconCache = new Dictionary<string, Texture2D>();

    public void Init()
    {
        SteamUserStats.RequestUserStats(SteamUser.GetSteamID());
        GodotSteamworksLogger.LogInfo("Steam achievements initialized");
    }

    /// <summary>
    /// Gets a list of all achievements and their unlocked status.
    /// </summary>
    /// <returns></returns>
    public List<Achievement> GetAchievements()
    {
        uint achievementCount = SteamUserStats.GetNumAchievements();
        GodotSteamworksLogger.LogInfo($"Found {achievementCount} achievements");
        List<Achievement> achievements = new List<Achievement>();
        for (int i = 0; i < achievementCount; i++)
        {
            string achievementName = SteamUserStats.GetAchievementName((uint)i);
            bool achieved;
            SteamUserStats.GetAchievement(achievementName, out achieved);
            var displayName = SteamUserStats.GetAchievementDisplayAttribute(achievementName, "name");
            var description = SteamUserStats.GetAchievementDisplayAttribute(achievementName, "desc");
            var icon = GetAchievementIcon(achievementName);
            SteamUserStats.GetAchievementProgressLimits(achievementName, out int minProgress, out int maxProgress);
            achievements.Add(new Achievement(achievementName, achieved, displayName, description, minProgress, maxProgress)
            {
                Icon = icon
            });
        }
        return achievements;
    }

    /// <summary>
    /// Gets the icon for the specified achievement as a Godot Texture2D.
    /// Returns the unlocked icon if the achievement is unlocked, or the locked icon if locked.
    /// Icons are cached in memory to avoid repeated retrieval and conversion.
    /// </summary>
    /// <param name="achievementKey">The achievement identifier</param>
    /// <returns>Texture2D of the achievement icon, or null if not available</returns>
    public Texture2D? GetAchievementIcon(string achievementKey)
    {
        // Check if the achievement is unlocked
        bool isUnlocked;
        SteamUserStats.GetAchievement(achievementKey, out isUnlocked);

        // Select the appropriate cache based on unlock state
        var cache = isUnlocked ? _unlockedIconCache : _lockedIconCache;

        // Check cache first
        if (cache.TryGetValue(achievementKey, out Texture2D? cachedIcon))
        {
            return cachedIcon;
        }

        // Get the icon handle from Steam
        // Note: Steam returns the unlocked icon when achievement is locked, and vice versa
        int iconHandle = SteamUserStats.GetAchievementIcon(achievementKey);
        if (iconHandle == 0)
        {
            GodotSteamworksLogger.LogWarning($"No icon found for achievement: {achievementKey}");
            return null;
        }

        // Get icon dimensions
        uint width, height;
        if (!SteamUtils.GetImageSize(iconHandle, out width, out height))
        {
            GodotSteamworksLogger.LogWarning($"Failed to get icon size for achievement: {achievementKey}");
            return null;
        }

        // Get the raw RGBA data
        int imageSize = (int)(width * height * 4); // 4 bytes per pixel (RGBA)
        byte[] imageData = new byte[imageSize];

        if (!SteamUtils.GetImageRGBA(iconHandle, imageData, imageSize))
        {
            GodotSteamworksLogger.LogWarning($"Failed to get icon data for achievement: {achievementKey}");
            return null;
        }

        // Create Godot Image from raw RGBA data
        var image = Image.CreateFromData((int)width, (int)height, false, Image.Format.Rgba8, imageData);

        // Create texture from image
        var texture = ImageTexture.CreateFromImage(image);

        // Cache the texture in the appropriate cache
        cache[achievementKey] = texture;

        return texture;
    }


    /// <summary>
    /// Unlocks the specified achievement for the user.
    /// </summary>
    /// <param name="achievementKey"></param>
    public void UnlockAchievement(string achievementKey)
    {
        if (!IsAchievementUnlocked(achievementKey))
        {
            SteamUserStats.SetAchievement(achievementKey);
            SteamUserStats.StoreStats();
        }
    }

    /// <summary>
    /// Checks if the specified achievement is unlocked for the user.
    /// </summary>
    /// <param name="achievementKey"></param>
    /// <returns></returns>
    public bool IsAchievementUnlocked(string achievementKey)
    {
        bool achieved;
        SteamUserStats.GetAchievement(achievementKey, out achieved);
        return achieved;
    }
    /// <summary>
    /// Gets the progress of the specified stat for the user.
    /// </summary>
    /// <param name="statKey">The Steam stat key (not the achievement key)</param>
    /// <returns>The current stat value</returns>
    public int GetStatProgress(string statKey)
    {
        int progress;
        SteamUserStats.GetStat(statKey, out progress);
        return progress;
    }

    /// <summary>
    /// Sets the progress of the specified stat for the user.
    /// </summary>
    /// <param name="statKey">The Steam stat key (not the achievement key)</param>
    /// <param name="progress">The new stat value</param>
    public void SetStatProgress(string statKey, int progress)
    {
        SteamUserStats.SetStat(statKey, progress);
        SteamUserStats.StoreStats();
    }

    /// <summary>
    /// Clears the specified achievement for the user. Primarily used for testing.
    /// Use with caution as this will remove the achievement from the user's profile.
    /// </summary>
    /// <param name="achievementKey"></param>
    public void ClearAchievement(string achievementKey)
    {
        SteamUserStats.ClearAchievement(achievementKey);
        SteamUserStats.StoreStats();
    }

    /// <summary>
    /// Clears all achievements for the user. Primarily used for testing.
    /// Use with caution as this will remove all achievements for the game from the user's profile.
    /// </summary>
    public void ResetAllAchievements()
    {
        uint achievementCount = SteamUserStats.GetNumAchievements();
        for (int i = 0; i < achievementCount; i++)
        {
            string achievementKey = SteamUserStats.GetAchievementName((uint)i);
            SteamUserStats.ClearAchievement(achievementKey);
        }
        SteamUserStats.StoreStats();
    }
}
#else
// Stub implementation for non-desktop platforms (Android, iOS, Web, etc.)
using Godot;
using Godot.Steamworks.Net.Models;
using System.Collections.Generic;

namespace Godot.Steamworks.Net;

/// <summary>
/// Stub class for non-desktop platforms.
/// Steam achievements are not supported on mobile platforms.
/// </summary>
public partial class SteamworksAchievements : RefCounted
{
    public void Init()
    {
        GodotSteamworksLogger.LogWarning("Steam achievements are not supported on this platform.");
    }

    public List<Achievement> GetAchievements() => new List<Achievement>();
    public Texture2D? GetAchievementIcon(string achievementKey) => null;
    public void UnlockAchievement(string achievementKey) { }
    public bool IsAchievementUnlocked(string achievementKey) => false;
    public int GetStatProgress(string statKey) => 0;
    public void SetStatProgress(string statKey, int progress) { }
    public void ClearAchievement(string achievementKey) { }
    public void ResetAllAchievements() { }
}
#endif

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
    public void Init()
    {
        // SteamUserStats.RequestCurrentStats(); // is what the docs say, but I dont see a call for it? Might be deprecated now
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
            // Eventually - SteamUserStats.GetAchievementIcon()
            bool achieved;
            SteamUserStats.GetAchievement(achievementName, out achieved);
            var displayName = SteamUserStats.GetAchievementDisplayAttribute(achievementName, "name");
            var description = SteamUserStats.GetAchievementDisplayAttribute(achievementName, "desc");
            achievements.Add(new Achievement(achievementName, achieved, displayName, description));            
        }
        return achievements;
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
    /// Gets the progress of the specified achievement for the user.
    /// </summary>
    /// <param name="achievementKey"></param>
    /// <returns></returns>
    public int GetAchievementProgress(string achievementKey)
    {
        int progress;
        SteamUserStats.GetStat(achievementKey, out progress);
        return progress;
    }

    /// <summary>
    /// Sets the progress of the specified achievement for the user.
    /// </summary>
    /// <param name="achievementKey"></param>
    /// <param name="progress"></param>
    public void SetAchievementProgress(string achievementKey, int progress)
    {
        SteamUserStats.SetStat(achievementKey, progress);
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

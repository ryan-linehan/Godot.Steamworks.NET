using Godot;
using Godot.Steamworks.Net;
using Godot.Steamworks.Net.Models;
using Steamworks;
using System.Collections.Generic;

namespace Godot.Steamworks.Net;

/// <summary>
/// Class for managing Steamworks stats
/// </summary>
public partial class SteamworksStats : RefCounted
{
    public void Init()
    {
        SteamUserStats.RequestUserStats(SteamUser.GetSteamID());
        GodotSteamworksLogger.LogInfo("Steam stats initialized");
    }

    /// <summary>
    /// Gets a list of all stats and their values.
    /// Note: Steam API doesn't provide a way to enumerate all stats like it does for achievements.
    /// This method returns stats that are stored as achievements' associated stats.
    /// For a complete stats implementation, you would need to manually track your stats names.
    /// </summary>
    /// <returns>List of stats</returns>
    public List<Stat> GetStats()
    {
        List<Stat> stats = new List<Stat>();
        
        // Since Steam doesn't provide direct enumeration of stats,
        // we attempt to get stats that might be associated with achievements
        uint achievementCount = SteamUserStats.GetNumAchievements();
        HashSet<string> processedStats = new HashSet<string>();
        
        for (int i = 0; i < achievementCount; i++)
        {
            string achievementName = SteamUserStats.GetAchievementName((uint)i);
            // Try to get progress limits which might indicate an associated stat
            if (SteamUserStats.GetAchievementProgressLimits(achievementName, out int minProgress, out int maxProgress))
            {
                if (maxProgress > 0 && !processedStats.Contains(achievementName))
                {
                    int statValue;
                    if (SteamUserStats.GetStat(achievementName, out statValue))
                    {
                        var displayName = SteamUserStats.GetAchievementDisplayAttribute(achievementName, "name");
                        stats.Add(new Stat(achievementName, displayName ?? achievementName, statValue, StatType.Int));
                        processedStats.Add(achievementName);
                    }
                }
            }
        }
        
        GodotSteamworksLogger.LogInfo($"Found {stats.Count} stats");
        return stats;
    }

    /// <summary>
    /// Gets the value of an integer stat.
    /// </summary>
    /// <param name="statKey">The stat identifier</param>
    /// <returns>The stat value, or 0 if not found</returns>
    public int GetStatInt(string statKey)
    {
        int value;
        if (SteamUserStats.GetStat(statKey, out value))
        {
            return value;
        }
        GodotSteamworksLogger.LogWarning($"Failed to get int stat: {statKey}");
        return 0;
    }

    /// <summary>
    /// Gets the value of a float stat.
    /// </summary>
    /// <param name="statKey">The stat identifier</param>
    /// <returns>The stat value, or 0.0 if not found</returns>
    public float GetStatFloat(string statKey)
    {
        float value;
        if (SteamUserStats.GetStat(statKey, out value))
        {
            return value;
        }
        GodotSteamworksLogger.LogWarning($"Failed to get float stat: {statKey}");
        return 0.0f;
    }

    /// <summary>
    /// Sets the value of an integer stat.
    /// </summary>
    /// <param name="statKey">The stat identifier</param>
    /// <param name="value">The value to set</param>
    public void SetStatInt(string statKey, int value)
    {
        if (SteamUserStats.SetStat(statKey, value))
        {
            SteamUserStats.StoreStats();
            GodotSteamworksLogger.LogInfo($"Set int stat {statKey} to {value}");
        }
        else
        {
            GodotSteamworksLogger.LogWarning($"Failed to set int stat: {statKey}");
        }
    }

    /// <summary>
    /// Sets the value of a float stat.
    /// </summary>
    /// <param name="statKey">The stat identifier</param>
    /// <param name="value">The value to set</param>
    public void SetStatFloat(string statKey, float value)
    {
        if (SteamUserStats.SetStat(statKey, value))
        {
            SteamUserStats.StoreStats();
            GodotSteamworksLogger.LogInfo($"Set float stat {statKey} to {value}");
        }
        else
        {
            GodotSteamworksLogger.LogWarning($"Failed to set float stat: {statKey}");
        }
    }

    /// <summary>
    /// Resets all stats (and optionally achievements).
    /// Primarily used for testing. Use with caution.
    /// </summary>
    /// <param name="achievementsToo">Whether to also reset achievements</param>
    public void ResetAllStats(bool achievementsToo = false)
    {
        SteamUserStats.ResetAllStats(achievementsToo);
        SteamUserStats.StoreStats();
        GodotSteamworksLogger.LogInfo($"Reset all stats (achievements too: {achievementsToo})");
    }
}

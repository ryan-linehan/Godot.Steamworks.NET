using System.ComponentModel;

namespace Godot.Steamworks.Net;

/// <summary>
/// Editor modules available in GodotSteamworks.NET
/// </summary>
public enum GodotSteamworksModules
{
    /// <summary>
    /// The settings for the editor plug-in (always enabled)
    /// </summary>
    [Description("Settings")]
    Settings,
    /// <summary>
    /// Achievements module for editor
    /// </summary>
    [Description("Achievements")]
    Achievements,
    /// <summary>
    /// Stats module for editor
    /// </summary>
    [Description("Stats")]
    Stats,
    /// <summary>
    /// Leaderboards module for editor
    /// </summary>
    [Description("Leaderboards")]
    Leaderboards,
    /// <summary>
    /// Rich Presence module for editor
    /// </summary>
    [Description("Rich Presence")]
    RichPresence,
}
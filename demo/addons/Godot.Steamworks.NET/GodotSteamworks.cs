using System;
using Godot;
using Steamworks;
namespace Godot.Steamworks.NET;

/// <summary>
/// Singleton class for Godot Steamworks.NET plugin for editor and runtime integration
/// </summary>
public partial class GodotSteamworks : Node
{
    /// <summary>
    /// The current log level for GodotSteamworks logging
    /// </summary>
    public static LogLevel LogLevel { get; set; } = LogLevel.Info;
    /// <summary>
    /// Singleton instance of GodotSteamworks.
    /// </summary>
    public static GodotSteamworks Instance { get; private set; } = null!;
    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
        try
        {
            GodotSteamworksLogger.LogDebug("Steam is running: " + SteamAPI.IsSteamRunning());
            if (SteamAPI.Init())
            {
                GodotSteamworksLogger.LogInfo("Steamworks initialized successfully.");
            }
            else
            {
                SteamAPI.InitEx(out var outSteamErrMsg);
                GodotSteamworksLogger.LogError("Steamworks initialization failed! err: " + outSteamErrMsg);
            }
        }
        catch (Exception ex)
        {
            GodotSteamworksLogger.LogError(ex.Message);
        }
    }
}

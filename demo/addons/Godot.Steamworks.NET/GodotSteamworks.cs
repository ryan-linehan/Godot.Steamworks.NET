using System;
using Godot;
using Godot.Steamworks.NET.Multiplayer;
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
    /// Singleton instance of GodotSteamworks
    /// </summary>
    public static GodotSteamworks Instance { get; private set; } = null!;
    /// <summary>
    /// Singleton instance of SteamworksLobby.
    /// </summary>
    public static SteamworksLobby SteamworksLobbyInstance { get; private set; } = new SteamworksLobby();
    /// <summary>
    /// Whether Steamworks has been successfully initialized or not
    /// </summary>
    public bool IsInitalized { get; private set; } = false;
    /// <summary>
    /// Whether to call SteamAPI.RunCallbacks() in the _Process method of the singleton.
    /// If true, Steam callbacks will be handled automatically. Otherwise the user is expected
    /// to call SteamAPI.RunCallbacks() manually. Defaults to true.
    /// </summary>
    public bool HandleSteamCallbacks { get; set; } = true;
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
                IsInitalized = true;
                SetProcess(HandleSteamCallbacks);
                if (!HandleSteamCallbacks)
                {
                    GodotSteamworksLogger.LogInfo("Automatic Steam callback handling is disabled. You must call SteamAPI.RunCallbacks() manually.");
                }
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

    public override void _Process(double delta)
    {
        base._Process(delta);
        SteamAPI.RunCallbacks();
    }
}



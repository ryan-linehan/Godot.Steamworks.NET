using System;
using Godot.Steamworks.Net.Multiplayer;
using Steamworks;

namespace Godot.Steamworks.Net;

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
    public static SteamworksLobby Lobby { get; private set; } = new SteamworksLobby();
    public static SteamworksAchievements Achievements { get; private set; } = new SteamworksAchievements();
    /// <summary>
    /// Whether Steamworks has been successfully initialized or not
    /// </summary>
    public bool IsInitialized { get; private set; } = false;
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
        InitGodotSteamworks();
    }

    public void InitGodotSteamworks()
    {
        try
        {
            if (IsInitialized)
                return;

            GodotSteamworksLogger.LogDebug("Steam is running: " + SteamAPI.IsSteamRunning());
            if (SteamAPI.Init())
            {
                IsInitialized = true;
                ParseCommandLineArgs();
                if (!HandleSteamCallbacks)
                {
                    GodotSteamworksLogger.LogInfo("Automatic Steam callback handling is disabled. You must call SteamAPI.RunCallbacks() manually.");
                }
                GodotSteamworksLogger.LogInfo("Steamworks initialized successfully.");
            }
            else
            {
                IsInitialized = false;
                SteamAPI.InitEx(out var outSteamErrMsg);
                GodotSteamworksLogger.LogError("Steamworks initialization failed! err: " + outSteamErrMsg);
            }
        }
        catch (Exception ex)
        {
            GodotSteamworksLogger.LogError(ex.Message);
            IsInitialized = false;
        }
    }

    public override void _ExitTree()
    {
        if (!IsInitialized)
            return;
        SteamAPI.Shutdown();
        base._ExitTree();
    }


    /// <summary>
    /// Parses command line arguments for Steam integration
    /// </summary>
    private static void ParseCommandLineArgs()
    {
        var args = System.Environment.GetCommandLineArgs();
        GodotSteamworksLogger.LogInfo("Steam command line args: " + string.Join(", ", args));
        bool nextIsLobby = false;
        foreach (var arg in args)
        {
            GodotSteamworksLogger.LogDebug("Arg: " + arg);
            if (arg.Equals("+connect_lobby"))
            {
                nextIsLobby = true;
            }
            else if (nextIsLobby)
            {
                if (ulong.TryParse(arg, out ulong lobbyId))
                {
                    GodotSteamworksLogger.LogInfo("Auto joining lobby from command line: " + lobbyId);
                    Lobby.JoinLobby(new CSteamID(lobbyId));
                }
                nextIsLobby = false;
            }
        }
    }


    public override void _Ready()
    {
        base._Ready();
        if (!IsInitialized || !HandleSteamCallbacks)
            SetProcess(false);
    }


    public override void _Process(double delta)
    {
        base._Process(delta);
        SteamAPI.RunCallbacks();
    }
}



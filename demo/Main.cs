using Godot;
using Godot.Steamworks.Net;
using System;

/// <summary>
/// Communication hub between the Steam Lobby Menu and the Game scene
/// </summary>
public partial class Main : Node
{
    [Export]
    SteamLobbyMenu SteamLobbyMenu = null!;
    [Export]
    Game Game = null!;
    ulong SteamLobbyId = 0;
    public override void _Ready()
    {
        SteamLobbyMenu.SignalStartGame += OnStartGame;
        SteamLobbyMenu.SignalJoinGame += OnJoinGame;
    }

    private void OnJoinGame(ulong lobbyId)
    {
        SteamLobbyMenu.Visible = false;
        SteamLobbyId = lobbyId;
        ConnectToGameServer();
    }


    private void OnStartGame(ulong lobbyId)
    {
        SteamLobbyMenu.Visible = false;
        SteamLobbyId = lobbyId;
        StartHosting();
        Game.StartGame();
    }

    /// <summary>
    /// Starts the host connection for godot's multiplayer via steam p2p
    /// </summary>
    private void StartHosting()
    {
        GD.Print("Hosting Game Session");
        try
        {
            var steamMultiplayerPeer = new SteamMultiplayerPeer();
            var steamErr = steamMultiplayerPeer.CreateServer(0);
            if (steamErr == Error.Ok)
            {
                Multiplayer.MultiplayerPeer = steamMultiplayerPeer;
                GD.Print("Hosting via Steam P2P successful");
                GodotSteamworks.Lobby.SetLobbyData(SteamLobbyId, "host_ready", "true");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to start hosting: {ex.Message}");
        }
    }

    /// <summary>
    /// Joins a godot multiplayer game via steam p2p
    /// </summary>
    private void ConnectToGameServer()
    {
        GD.Print("Joining Game Session");
        if (!GodotSteamworks.Instance.IsInitalized)
        {
            GD.PrintErr("GodotSteamworks is not initialized! Multiplayer only supported when Steam is running and initialized for the demo");
            return;
        }

        try
        {
            var steamMultiplayerPeer = new SteamMultiplayerPeer();
            var steamErr = steamMultiplayerPeer.CreateClient(GodotSteamworks.Lobby.GetLobbyOwner(SteamLobbyId), 0);
            if (steamErr == Error.Ok)
            {
                // Use the MultiplayerPeer property for Godot compatibility
                Multiplayer.MultiplayerPeer = steamMultiplayerPeer;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to join game: {ex.Message}");
        }
    }

}

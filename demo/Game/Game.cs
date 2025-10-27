using Godot;
using Godot.Steamworks.NET;
using System;

public partial class Game : Node
{
    [Export]
    public Node2D World { get; set; } = null;
    public ulong SteamLobbyId { get; set; }
    public override void _Ready()
    {
        base._Ready();
        World.Visible = false;
    }
    public void StartGame()
    {
        GD.Print("Game Started!");
        GD.Print($"Starting Networking P2P via Steam for Lobby ID: {SteamLobbyId}");
        if (GodotSteamworks.Instance.IsInitalized && GodotSteamworks.Lobby.IsLobbyOwner(SteamLobbyId))
        {
            StartHosting();
            World.Visible = true;
        }
        else
        {
            GD.PrintErr("GodotSteamworks is not initialized! Multiplayer only supported when Steam is running and initialized for the demo");
        }
    }

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

    public void JoinGame()
    {
        GD.Print("Joining Game Session");
        if (!GodotSteamworks.Instance.IsInitalized)
        {
            GD.PrintErr("GodotSteamworks is not initialized! Multiplayer only supported when Steam is running and initialized for the demo");
            return;
        }

        World.Visible = true;
        try
        {

            var steamMultiplayerPeer = new SteamMultiplayerPeer();
            var steamErr = steamMultiplayerPeer.CreateClient(GodotSteamworks.Lobby.GetLobbyOwner(SteamLobbyId), 0);
            if (steamErr == Error.Ok)
            {
                // Use the MultiplayerPeer property for Godot compatibility
                Multiplayer.MultiplayerPeer = steamMultiplayerPeer;
                GD.Print("Joined via Steam P2P successful");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to join game: {ex.Message}");
        }
    }
}

using Godot;
using Godot.Steamworks.Net;
using System;
using System.Collections.Generic;
public partial class Game : Node
{
    [Export]
    public Node2D World { get; set; } = null!;
    [Export]
    public PackedScene PlayerScene { get; set; } = null!;
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
                AddPlayer(Multiplayer.GetUniqueId());
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
                RpcId(1, MethodName.PlayerJoined, Multiplayer.GetUniqueId());
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to join game: {ex.Message}");
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void PlayerJoined(int peerId)
    {
        // Only the host needs to add players as they join since they will be added to the scene
        // by the host automatically via MultiplayerSpawner
        if (!Multiplayer.IsServer())
            return;
        GD.Print($"Player Joined with Peer ID: {peerId}");
        AddPlayer(peerId);
        SyncExistingPlayers();
    }

    private void AddPlayer(int peerId)
    {
        GD.Print("Adding player " + peerId + " to the game world");
        var player = PlayerScene.Instantiate<Player>();
        player.PeerId = peerId;
        World.AddChild(player, true);
    }

    [Rpc]
    private void SyncExistingPlayers()
    {
        GD.Print("Syncing Existing Players in Game");
        var players = GetTree().GetNodesInGroup("Player");
        HashSet<int> existingPeerIds = new HashSet<int>();
        foreach (Player player in players)
        {
            existingPeerIds.Add(player.PeerId);
        }
        foreach (var peerId in Multiplayer.GetPeers())
        {
            if (existingPeerIds.Contains(peerId))
                continue;
            AddPlayer(peerId);
        }
    }
}

using Godot;
using Godot.Steamworks.Net;

public partial class Game : Node
{
    [Export]
    public Node2D World { get; set; } = null!;
    [Export]
    public PackedScene PlayerScene { get; set; } = null!;
    public override void _Ready()
    {
        base._Ready();
        World.Visible = false;
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
    }

    private void OnPeerConnected(long id)
    {
        GD.Print($"Peer Connected: {id}" + "Self id: " + Multiplayer.GetUniqueId());
        if (!Multiplayer.IsServer())
            return;
        PlayerJoined(id);
    }

    /// <summary>
    /// Handles a peer disconnecting from the game
    /// </summary>
    /// <param name="id"></param>
    private void OnPeerDisconnected(long id)
    {
        GD.Print($"Peer Disconnected: {id}");
        // Handle player disconnection logic here
        if (!Multiplayer.IsServer())
            return;
        PlayerLeft(id);
    }

    /// <summary>
    /// Handles a player leaving the game by removing them from the world
    /// </summary>
    /// <param name="peerId"></param>
    private void PlayerLeft(long peerId)
    {
        GD.Print($"Player Left with Peer ID: {peerId}");
        // Remove the player from the world
        foreach (var player in World.GetChildren())
        {
            if (player is Player p && p.PeerId == peerId)
            {
                p.QueueFree();
                break;
            }
        }
    }

    /// <summary>
    /// Starts the game by making the world visible
    /// If the local peer is the host, adds the host player to the world
    /// which syncs out to all connected clients
    /// </summary>
    public void StartGame()
    {
        World.Visible = true;
        if(!Multiplayer.IsServer())
            return;
        AddPlayerToGameWorld(Multiplayer.GetUniqueId());
    }

    /// <summary>
    /// Called by the host when a new player joins the gameto sync them into the world
    /// and other clients
    /// </summary>
    /// <param name="peerId"></param>
    private void PlayerJoined(long peerId)
    {
        // Only the host needs to add players as they join since they will be added to the scene
        // by the host automatically via MultiplayerSpawner
        if (!Multiplayer.IsServer())
            return;
        GD.Print($"Player Joined with Peer ID: {peerId}");
        AddPlayerToGameWorld(peerId);
    }

    /// <summary>
    /// Adds a player to the game world to sets its authority to the given peer id
    /// NOTE: This is okay for games that are not competitive since a malicious client could
    /// easily cheat. For competitive games, consider using a server authoritative model.
    /// </summary>
    /// <param name="peerId"></param>
    private void AddPlayerToGameWorld(long peerId)
    {
        GD.Print($"[{Multiplayer.GetUniqueId()}] Adding player {peerId} to the game world");
        var player = PlayerScene.Instantiate<Player>();
        if(Multiplayer.MultiplayerPeer is SteamMultiplayerPeer steamPeer)
        {
            var peerName = steamPeer.GetSteamDisplayNameFromPeerId((int)peerId);
            player.PlayerName = peerName;
        }        
        player.PeerId = peerId;
        
        World.AddChild(player, true);
        player.SetMultiplayerAuthority((int)peerId);
    }
}

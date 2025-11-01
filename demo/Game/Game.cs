using Godot;

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
        GD.Print("Joined via Steam P2P successful");
        if (!Multiplayer.IsServer())
            return;
        PlayerJoined(id);
    }

    private void OnPeerDisconnected(long id)
    {
        GD.Print($"Peer Disconnected: {id}");
        // Handle player disconnection logic here
        if (!Multiplayer.IsServer())
            return;
        PlayerLeft(id);
    }

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

    public void StartGame()
    {
        GD.Print("Game Started!");
        AddPlayerToGameWorld(Multiplayer.GetUniqueId());
        World.Visible = true;
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
    /// Adds a player to the game world
    /// </summary>
    /// <param name="peerId"></param>
    private void AddPlayerToGameWorld(long peerId)
    {
        GD.Print("Adding player " + peerId + " to the game world");
        var player = PlayerScene.Instantiate<Player>();
        player.PeerId = peerId;
        World.AddChild(player, true);
    }
}

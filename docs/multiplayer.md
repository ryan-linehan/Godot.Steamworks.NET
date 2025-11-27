# Multiplayer Peer-to-Peer Networking

The Godot.Steamworks.NET plugin provides a `SteamMultiplayerPeer` implementation that integrates Steam's P2P networking with Godot's native multiplayer system. This enables you to build peer-to-peer multiplayer games that leverage Steam's relay network for secure, NAT-traversable connections.

The **key importance** of this plugin is that it handles the establishment of the `MultiplayerPeer` for you once you have a working `SteamMultiplayerPeer` set as your `Multiplayer.MultiplayerPeer`, all of Godot's built-in multiplayer features (node replication, RPCs, synchronization) work seamlessly over Steam's network.

> Note: SteamMultiplayerPeer is based off this implementation: <https://github.com/expressobits/steam-multiplayer-peer>

## Testing vs. Deployment

### Why Use GodotENet for Development

For faster iteration during development, the demo project demonstrates how to use **Godot's built-in ENet multiplayer** as a fallback when testing locally. This is significantly easier than:

- Deploying to Steam repeatedly
- Managing lobby creation for tests
- Dealing with Steam network initialization overhead

**The demo project shows both approaches** because the important part is that the `MultiplayerPeer` gets established correctly. Whether you use `ENetMultiplayerPeer` for local testing or `SteamMultiplayerPeer` for production, the rest of your game code remains the same.

```csharp
// From NetworkingCanvas.cs - Choose your networking backend
if (usingENet)
{
    var multiplayerPeer = new ENetMultiplayerPeer();
    multiplayerPeer.CreateServer(port, maxPlayers);
    Multiplayer.MultiplayerPeer = multiplayerPeer;
}
else if (usingSteam)
{
    var steamMultiplayerPeer = new SteamMultiplayerPeer();
    steamMultiplayerPeer.CreateHost(0);
    Multiplayer.MultiplayerPeer = steamMultiplayerPeer;
}
```

This flexibility means you can:

1. Test locally with ENet (fast iteration)
2. Deploy to Steam with no code changes (only configuration)
3. Fall back to ENet if Steam is unavailable

## Establishing the MultiplayerPeer

### Host Setup

The host is typically the first player who creates a lobby or game session:

```csharp
var steamMultiplayerPeer = new SteamMultiplayerPeer();
var error = steamMultiplayerPeer.CreateHost(0);  // 0 = virtual port

if (error == Error.Ok)
{
    Multiplayer.MultiplayerPeer = steamMultiplayerPeer;
    GD.Print("Hosting started successfully");
    // Other peers can now connect to you
}
```

### Client Setup

Clients connect to the host by providing the host's Steam ID:

```csharp
var steamMultiplayerPeer = new SteamMultiplayerPeer();
var error = steamMultiplayerPeer.CreateClient(
    hostSteamId,  // The Steam ID of the host
    0              // Virtual port (must match host's port)
);

if (error == Error.Ok)
{
    Multiplayer.MultiplayerPeer = steamMultiplayerPeer;
    GD.Print("Connected to host");
    // You are now part of the multiplayer network
}
```

## Synchronizing Game State

Once the `MultiplayerPeer` is established, you have two main approaches for synchronizing game state:

### 1. MultiplayerSpawner + MultiplayerSynchronizer

These are godot's high level multiplayer nodes. It automatically handles node replication and property synchronization. There are plenty of tutorials on how to use these on youtube or check out the demo project for a simple example

### 2. RPC Calls (For Events and Actions)

While the demo primarily uses automatic synchronization, you can also use RPCs for discrete events:

```csharp
[Rpc(MultiplayerApi.RpcMode.Any)]
public void OnPlayerAttack(Vector2 direction)
{
    // This method will be called on all peers
    // Only execute gameplay logic on the authority
    if (Multiplayer.GetUniqueId() != PeerId)
        return;

    PlayAttackAnimation(direction);
    DealDamage(direction);
}

// Call it from code:
Rpc(MethodName.OnPlayerAttack, attackDirection);
```

## Multiplayer Authority: The Tricky Part

**Authority determines who has control over a node's behavior.** Getting this right is critical but can be confusing:

### The Problem

Without careful authority handling, you might experience:

- All players responding to the same input
- Desynced state across peers
- Cheating in competitive games
- Network bandwidth waste from conflicting updates

### The Solution

**The demo project uses a peer-trusting, non-competitive model:** each player controls their own character, and other players trust that input.

This works for:

- Cooperative games (friends playing together)
- Games where players don't directly compete
- Games where minor desync isn't game-breaking

## Alternative P2P Architecture: Server Authoritative Model

For competitive games where players might cheat, use a **server authoritative model** within P2P:

### How It Works

1. One player acts as the "server" (not the host necessarily)
2. All other players are "clients"
3. Clients send their input/actions to the server
4. The server validates and applies the actions
5. The server sends the corrected game state back to all clients

### Pros and Cons

**Pros:**

- Prevents cheating (server validates all actions)
- Single source of truth
- Works for competitive games

**Cons:**

- Higher latency (all actions go through one player)
- That one player's connection strength affects everyone
- More complex code

## Steam's Dedicated Server Support

Steam also supports **dedicated server deployments**, where a dedicated machine (not a player) runs the server. However, this plugin is primarily focused on **peer-to-peer gaming** between friends.

### Why P2P Instead of Dedicated Servers?

- **Lower cost**: No need to maintain dedicated servers
- **Lower latency**: Direct peer-to-peer connections (with Steam relay for NAT traversal)
- **Friends playing with friends**: Perfect for cooperative or casual games

### If You Need Dedicated Server Support

This plugin currently doesn't provide dedicated server examples or documentation. However:

- Contributions are welcome! If you implement dedicated server support, consider contributing examples or documentation.
- You can still use Godot's standard networking with a custom implementation on a dedicated machine.
- Steam's relay network can be used alongside dedicated servers for mixed deployments.

## Common Patterns from the Demo

### 1. Detecting Remote vs. Local Players

```csharp
if (Multiplayer.GetUniqueId() == PeerId)
{
    // This is the local player (us)
    ProcessInput();
}
else
{
    // This is a remote player
    InterpolatePosition();
}
```

### 2. Host-Only Logic

```csharp
private void OnPeerConnected(long id)
{
    if (!Multiplayer.IsServer())
        return;  // Only host executes this

    SpawnPlayerForNewPeer(id);
}
```

### 3. Bandwidth Optimization

The demo only syncs position when it changes significantly:

```csharp
if (GlobalPosition.DistanceSquaredTo(_lastNetworkPosition) > 1.0f)
{
    NetworkPosition = GlobalPosition;  // Send update
    _lastNetworkPosition = GlobalPosition;
}
```

This reduces network traffic dramatically for smooth movement.

### 4. Property Interpolation

Remote players are smoothly interpolated to avoid choppy movement:

```csharp
GlobalPosition = GlobalPosition.Lerp(NetworkPosition, 0.25f);
```

The `0.25f` value controls interpolation speed. Higher values = faster sync but choppier; lower values = smoother but more latency.

## Debugging and Common Issues

### Connection Not Established

Check that:

1. Steam is running and the app ID is correct
2. Both host and client are using the same virtual port
3. Firewall/NAT isn't blocking the connection (Steam relay should handle this)

## See Also

- [Achievements API](./achievements.md)
- [Godot Multiplayer Documentation](https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html)
- [Godot MultiplayerSpawner](https://docs.godotengine.org/en/stable/classes/class_multiplayerspawner.html)
- [Godot MultiplayerSynchronizer](https://docs.godotengine.org/en/stable/classes/class_multiplayersynchronizer.html)

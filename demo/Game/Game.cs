using Godot;
using System;

public partial class Game : Node
{
    public ulong SteamLobbyId { get; set; }
    public void StartGame()
    {
        GD.Print("Game Started!");
        if (SteamLobbyId != 0)
        {
            GD.Print($"Networking P2P via Steam for Lobby ID: {SteamLobbyId}");
        }
    }
}

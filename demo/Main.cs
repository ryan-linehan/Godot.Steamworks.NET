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
    Control _networkMenu = null!;
    public override void _Ready()
    {
        _networkMenu = SteamLobbyMenu;
        SteamLobbyMenu.SignalGameHostReady += OnHostGame;
        SteamLobbyMenu.SignalGameJoined += OnJoinGame;
    }

    private void OnJoinGame()
    {
        _networkMenu.Visible = false;
    }


    private void OnHostGame()
    {
        _networkMenu.Visible = false;
        Game.StartGame();
    }
}

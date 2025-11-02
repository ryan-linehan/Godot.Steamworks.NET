using Godot;
using Godot.Steamworks.Net;
using System;

/// <summary>
/// Communication hub between the Steam Lobby Menu and the Game scene
/// </summary>
public partial class Main : Node
{
    [Export]
    NetworkingCanvas NetworkingCanvas = null!;
    [Export]
    Game Game = null!;
    NetworkingCanvas _networkMenu = null!;
    public override void _Ready()
    {
        _networkMenu = NetworkingCanvas;
        _networkMenu.SignalGameHostReady += OnHostReady;
        _networkMenu.SignalGameJoined += OnGameJoined;
    }

    private void OnGameJoined()
    {
        _networkMenu.Visible = false;
        Game.StartGame();
    }


    private void OnHostReady()
    {
        _networkMenu.Visible = false;
        Game.StartGame();
    }
}

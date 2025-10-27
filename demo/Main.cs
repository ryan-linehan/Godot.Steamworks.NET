using Godot;
using System;

public partial class Main : Node
{
    [Export]
    SteamLobbyMenu SteamLobbyMenu = null!;
    [Export]
    Game Game = null!;
    public override void _Ready()
    {
        SteamLobbyMenu.SignalStartGame += OnStartGame;
        SteamLobbyMenu.SignalJoinGame += OnJoinGame;
    }

    private void OnJoinGame(ulong lobbyId)
    {
        SteamLobbyMenu.Visible = false;
        Game.SteamLobbyId = lobbyId;
        Game.JoinGame();
    }


    private void OnStartGame(ulong lobbyId)
    {
        SteamLobbyMenu.Visible = false;
        Game.SteamLobbyId = lobbyId;
        Game.StartGame();
    }

}

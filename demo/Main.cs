using Godot;
using System;

public partial class Main : Node
{
    [Export]
    SteamLobbyMenu SteamLobbyMenu;
    [Export]
    Game Game;
    public override void _Ready()
    {
        SteamLobbyMenu.SignalStartGame += OnStartGame;
    }

    private void OnStartGame(ulong lobbyId)
    {
        SteamLobbyMenu.Visible = false;
        Game.SteamLobbyId = lobbyId;
        Game.StartGame();
    }

}

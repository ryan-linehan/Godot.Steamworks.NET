using Godot;
using System;

public partial class SinglePlayerMenu : Control
{
    /// <summary>
    /// Signals that the game should start
    /// </summary>
    /// <param name="lobbyId"></param>
    [Signal]
    public delegate void SignalGameHostReadyEventHandler();
    [Export]
    public Button StartGame { get; set; } = null!;
    public override void _Ready()
    {
        base._Ready();
        StartGame.Pressed += OnStartGamePressed;
    }

    private void OnStartGamePressed()
    {
        EmitSignal(SignalName.SignalGameHostReady);
    }

}

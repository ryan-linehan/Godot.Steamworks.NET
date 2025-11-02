using Godot;
using System;

public partial class GodotJoinMenu : PanelContainer
{
    [Signal]
    public delegate void SignalGameJoinedEventHandler();
    [Export]
    public Button BackButton = null!;
    [Export]
    public LineEdit IpAddress = null!;
    [Export]
    public LineEdit Port = null!;
    [Export]
    public Button JoinGameButton = null!;
}

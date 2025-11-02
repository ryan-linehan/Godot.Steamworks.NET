using Godot;
using System;

public partial class GodotHostMenu : PanelContainer
{
    [Export]
    public Button BackButton = null!;
    [Export]
    public LineEdit Port = null!;
    [Export]
    public Button HostGameButton = null!;
}

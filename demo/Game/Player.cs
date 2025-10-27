using Godot;
using System;

public partial class Player : Node2D
{
    /// <summary>
    /// Exported so we can sync it over the network using godot's MultiplayerSyncronizer
    /// </summary>
    [Export]
    public int PeerId { get; set; } = 1;
    public override void _Ready()
    {
        base._Ready();
    }
}

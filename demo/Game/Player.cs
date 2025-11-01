using Godot;
using System;

public partial class Player : CharacterBody2D
{
    /// <summary>
    /// Exported so we can sync it over the network using godot's MultiplayerSyncronizer
    /// </summary>
    [Export]
    public int PeerId { get; set; } = 1;
    Vector2 _direction = Vector2.Zero;
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("Player");
        if (PeerId == Multiplayer.GetUniqueId())
        {
            AddChild(new Camera2D());
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessMovement(_direction);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        GD.Print("Unhandled Input in Player " + PeerId);
        if (Multiplayer.GetUniqueId() != PeerId)
            return;
        GD.Print("Processing Input for Local Player " + PeerId);
        _direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        _direction = new Vector2(SnapToAxis(_direction.X), SnapToAxis(_direction.Y));
        if (Mathf.IsZeroApprox(_direction.X) && Mathf.IsZeroApprox(_direction.Y))
        {
            // If the player is not moving, change to idle state
            Velocity = Vector2.Zero;
        }
    }
    /// <summary>
    /// Snaps the value to an axis based on a threshold
    /// </summary>
    /// <param name="value"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    int SnapToAxis(float value, float threshold = 0.4f)
    {
        if (value >= threshold)
            return 1;
        else if (value <= -threshold)
            return -1;
        else
            return 0;
    }

    /// <summary>
    /// Processes the player movement. No network code.
    /// </summary>
    private void ProcessMovement(Vector2 direction)
    {
        _direction = direction;
        Velocity = direction * 600;
        GD.Print("Player " + PeerId + " moving in direction " + direction);
        MoveAndSlide();
        // Snap position to whole pixels to avoid subpixel movement
        GlobalPosition = new Vector2(Mathf.Round(GlobalPosition.X), Mathf.Round(GlobalPosition.Y));
    }
}

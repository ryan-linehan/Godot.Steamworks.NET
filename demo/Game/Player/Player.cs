using Godot;
using System;

public partial class Player : CharacterBody2D
{
    private long _peerId;
    /// <summary>
    /// Exported so we can sync it over the network using godot's MultiplayerSyncronizer
    /// </summary>
    [Export]
    public long PeerId
    {
        get
        {
            return _peerId;
        }
        set
        {
            _peerId = value;
        }
    }
    [Export]
    public PackedScene PlayerCamera { get; set; } = null!;
    [Export]
    public Vector2 NetworkPosition { get; set; } = Vector2.Zero;
    Vector2 _direction = Vector2.Zero;
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("Player");
        GD.Print($"[{Multiplayer.GetUniqueId()}] Player._Ready() - PeerId: {PeerId}, Authority: {GetMultiplayerAuthority()}");

        if (!Multiplayer.IsServer())
        {
            SetMultiplayerAuthority((int)PeerId);
        }

        // Add a camera if this is the local player
        if (PeerId == Multiplayer.GetUniqueId())
        {
            var camera = PlayerCamera.Instantiate<Camera2D>();
            AddChild(camera, true);
            GD.Print($"[{Multiplayer.GetUniqueId()}] CAMERA ADDED to player {PeerId}");
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Multiplayer.GetUniqueId() != PeerId)
        {
            // Smoothly interpolate to the networked position if not the local player
            GlobalPosition = GlobalPosition.Lerp(NetworkPosition, 0.2f);
        }
        else
        {
            ProcessMovement(_direction);
            NetworkPosition = GlobalPosition;
        }
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (Multiplayer.GetUniqueId() != PeerId)
            return;
        _direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        _direction = new Vector2(SnapToAxis(_direction.X), SnapToAxis(_direction.Y));
        if (Mathf.IsZeroApprox(_direction.X) && Mathf.IsZeroApprox(_direction.Y))
        {
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
        MoveAndSlide();
        // Snap position to whole pixels to avoid subpixel movement
        GlobalPosition = new Vector2(Mathf.Round(GlobalPosition.X), Mathf.Round(GlobalPosition.Y));
    }
}

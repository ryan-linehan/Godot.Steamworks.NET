#if !(GODOT_PC || GODOT_WINDOWS || GODOT_LINUX || GODOT_MACOS || GODOT_X11 || GODOT_OSX)
using Godot;

namespace Godot.Steamworks.Net;

/// <summary>
/// Stub implementation of GodotSteamworks for non-desktop platforms (Android, iOS, Web).
/// Steam is not available on these platforms, so this provides a no-op implementation.
/// </summary>
public partial class GodotSteamworks : Node
{
    public static GodotSteamworks? Instance { get; private set; }
    public bool IsInitialized => false;
    public bool HandleSteamCallbacks { get; set; } = false;

    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
        GD.Print("[GodotSteamworks] Steam is not supported on this platform.");
    }

    public override void _Ready()
    {
        base._Ready();
        SetProcess(false);
    }
}
#endif

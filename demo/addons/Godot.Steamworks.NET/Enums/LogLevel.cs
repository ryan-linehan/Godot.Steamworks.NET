#if GODOT_PC || GODOT_WINDOWS || GODOT_LINUX || GODOT_MACOS || GODOT_X11 || GODOT_OSX
namespace Godot.Steamworks.Net;

/// <summary>
/// Log level enumeration for GodotSteamworks logging.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// No logging output
    /// </summary>
    None = 0,
    /// <summary>
    /// Error level logging
    /// </summary>
    Error = 1,
    /// <summary>
    /// Warning level logging
    /// </summary>
    Warning = 2,
    /// <summary>
    /// Info level logging
    /// </summary>
    Info = 3,
    /// <summary>
    /// Debug level logging
    /// </summary>
    Debug = 4
}
#endif
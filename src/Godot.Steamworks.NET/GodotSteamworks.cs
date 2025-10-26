using Godot;
using Steamworks;
namespace Godot.Steamworks.NET;

/// <summary>
/// Singleton class for Godot Steamworks.NET integration.
/// A user should inherit from this class and add it as a global to their godot project to begin using steamworks features.
/// </summary>
public partial class GodotSteamworks : Node
{
    /// <summary>
    /// Singleton instance of GodotSteamWorks.
    /// </summary>
    public static GodotSteamworks Instance { get; private set; } = null!;
    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
        GD.Print("[GodotSteamworks] Initialized Godot Steamworks.NET singleton.");
        GD.Print("Steamworks: attempting initialization...");
        try
        {
            GD.Print("Steam is running: " + SteamAPI.IsSteamRunning());
            if (SteamAPI.Init())
            {
                GD.Print("Steamworks initialization succeeded!");
            }
            else
            {
                SteamAPI.InitEx(out var outSteamErrMsg);
                GD.Print("Steamworks initialization failed! err: " + outSteamErrMsg);
            }
        }
        catch (Exception e)
        {
            GD.Print("Steamworks initialization threw an exception :O");
            GD.Print(e);
        }
    }
}

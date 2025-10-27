using Godot;
namespace Godot.Steamworks.Net.Models;
public partial class LobbyMember : Godot.RefCounted
{
    /// <summary>
    /// The Steam ID of the lobby member
    /// </summary>
    public ulong SteamId { get; set; }
    /// <summary>
    /// The steam friends list name for the lobby member
    /// </summary>
    public string SteamDisplayName { get; set; } = null!;
}
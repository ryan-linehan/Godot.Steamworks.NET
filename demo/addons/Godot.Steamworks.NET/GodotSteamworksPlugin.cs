#if TOOLS
using Godot;
using System;
namespace Godot.Steamworks.Net;

[Tool]
public partial class GodotSteamworksPlugin : EditorPlugin
{
	public const string GodotSteamworksAutoloadName = "GodotSteamworks";
	
	public override void _EnterTree()
	{
		AddAutoloadSingleton(GodotSteamworksAutoloadName, "res://addons/Godot.Steamworks.NET/GodotSteamworks.cs");
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton(GodotSteamworksAutoloadName);
	}
}

#endif
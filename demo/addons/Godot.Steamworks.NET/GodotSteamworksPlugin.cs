using Godot;
using System;

[Tool]
public partial class GodotSteamworksPlugin : EditorPlugin
{
	public const string GodotSteamworksAutoloadName = "GodotSteamworksSingleton";
	public override void _EnterTree()
	{
		AddAutoloadSingleton(GodotSteamworksAutoloadName, "res://addons/Godot.Steamworks.NET/GodotSteamworksSingleton.cs");
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton(GodotSteamworksAutoloadName);
	}
}

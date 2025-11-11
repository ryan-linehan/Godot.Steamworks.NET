#if TOOLS
using Godot;
using System;
namespace Godot.Steamworks.Net;

/// <summary>
/// Main editor plugin for GodotSteamworks.NET
/// This plugin registers the runtime Steamworks singleton.
/// The editor UI tools are handled by the optional GodotSteamworksEditorPlugin sub-plugin.
/// </summary>
[Tool]
public partial class GodotSteamworksPlugin : EditorPlugin
{
	public const string GodotSteamworksAutoloadName = "GodotSteamworks";

	public override void _EnablePlugin()
	{
		GD.Print("Enabling GodotSteamworksPlugin");

	}

	public override void _DisablePlugin()
	{
		GD.Print("Disabling GodotSteamworksPlugin");
	}

	public override void _EnterTree()
	{
		// Register the main runtime singleton
		AddAutoloadSingleton(GodotSteamworksAutoloadName, "res://addons/Godot.Steamworks.NET/GodotSteamworks.cs");
		// Enable the editor sub-plugin
		GD.Print("Enabling GodotSteamworks.NET.Editor sub-plugin");
		EditorInterface.Singleton.SetPluginEnabled("res://addons/Godot.Steamworks.NET/Editor/plugin.cfg", true);
		base._EnterTree();
	}

	public override void _ExitTree()
	{
		GodotSteamworksEditorPlugin.Instance?.Cleanup();
		RemoveAutoloadSingleton(GodotSteamworksAutoloadName);
		base._ExitTree();
	}


}

#endif
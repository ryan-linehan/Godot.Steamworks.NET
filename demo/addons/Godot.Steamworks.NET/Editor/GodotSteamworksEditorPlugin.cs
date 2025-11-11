#if TOOLS
using Godot;
using System;
namespace Godot.Steamworks.Net;

/// <summary>
/// Editor plugin for GodotSteamworks.NET
/// This is a sub-plugin that provides editor UI tools.
/// It is optional and can be disabled without affecting runtime Steamworks functionality.
/// </summary>
[Tool]
public partial class GodotSteamworksEditorPlugin : EditorPlugin
{
	public const string GodotSteamworksEditorName = "GodotSteamworksEditor";
	public string SteamPanelScenePath = "res://addons/Godot.Steamworks.NET/Editor/SteamPanel.tscn";
	Control steamPanel = null!;
	public override void _EnablePlugin()
	{
		base._EnablePlugin();
		GD.Print("Entering GodotSteamworksEditorPlugin");
	}

    public override void _EnterTree()
    {
		base._EnterTree();
		// Initialize Steamworks if the instance is ready
		if (GodotSteamworks.Instance != null && !GodotSteamworks.Instance.IsInitialized)
		{
			GD.Print("Initializing GodotSteamworks from EditorPlugin");
			GodotSteamworks.Instance.InitGodotSteamworks();
		}

		// Add the Steam panel to the editor's main screen
		steamPanel = GD.Load<PackedScene>(SteamPanelScenePath).Instantiate<Control>();
		EditorInterface.Singleton.GetEditorMainScreen().AddChild(steamPanel);
		_MakeVisible(false);
    }



	public override void _DisablePlugin()
	{
		GD.Print("Exiting GodotSteamworksEditorPlugin");
		if (steamPanel != null)
		{
			steamPanel.QueueFree();
		}
	}

	public override bool _HasMainScreen()
	{
		return true;
	}

	public override void _MakeVisible(bool visible)
	{
		if (steamPanel != null)
		{
			steamPanel.Visible = visible;
		}
	}

	public override string _GetPluginName()
	{
		return "Steam";
	}

	public override Texture2D _GetPluginIcon()
	{
		// Return a custom icon or use a default one
		return EditorInterface.Singleton.GetBaseControl().GetThemeIcon("Node", "EditorIcons");
	}
}

#endif

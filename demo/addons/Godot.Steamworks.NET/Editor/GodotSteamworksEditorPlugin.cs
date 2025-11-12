#if TOOLS
using Godot;
using System;
using SteamWebAPI2.Utilities;
namespace Godot.Steamworks.Net;

/// <summary>
/// Editor plugin for GodotSteamworks.NET
/// This is a sub-plugin that provides editor UI tools.
/// It is optional and can be disabled without affecting runtime Steamworks functionality.
/// </summary>
[Tool]
public partial class GodotSteamworksEditorPlugin : EditorPlugin
{
	public static GodotSteamworksEditorPlugin Instance { get; private set; } = null!;
	public const string GodotSteamworksEditorName = "GodotSteamworksEditor";
	public string SteamPanelScenePath = "res://addons/Godot.Steamworks.NET/Editor/SteamPanel.tscn";
	public SteamPanel SteamPanel { get; private set; } = null!;
	public static SteamWebInterfaceFactory WebInterfaceFactory { get; private set; } = null!;
	public override void _EnablePlugin()
	{
		base._EnablePlugin();
		GD.Print("Entering GodotSteamworksEditorPlugin");
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		Instance = this;
		// Add the Steam panel to the editor's main screen
		SteamPanel = GD.Load<PackedScene>(SteamPanelScenePath).Instantiate<SteamPanel>();
		EditorInterface.Singleton.GetEditorMainScreen().AddChild(SteamPanel);
		_MakeVisible(false);
	}

	/// <summary>
	/// Gets the static SteamWebInterfaceFactory with the given API key
	/// </summary>
	public static SteamWebInterfaceFactory GetSteamWebInterfaceFactory(string apiKey)
	{
		WebInterfaceFactory = new SteamWebInterfaceFactory(apiKey);
		return WebInterfaceFactory;
	}

	public void Cleanup()
	{
		SteamPanel?.CallDeferred(SteamPanel.MethodName.QueueFree);
	}

	public override bool _HasMainScreen()
	{
		return true;
	}

	public override void _MakeVisible(bool visible)
	{
		if (SteamPanel != null)
		{
			SteamPanel.Visible = visible;
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

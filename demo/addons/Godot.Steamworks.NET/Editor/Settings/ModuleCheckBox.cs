using Godot;
using System;
using Godot.Steamworks.Net.Utils;

namespace Godot.Steamworks.Net;

[Tool]
public partial class ModuleCheckBox : Control
{
    [Export]
    public GodotSteamworksModules Module { get; set; } = GodotSteamworksModules.Achievements;
    private CheckButton _checkButton = null!;
    private Label _label = null!;

    public override void _Ready()
    {
        base._Ready();
        _checkButton = GetNode<CheckButton>("CheckButton");
        _label = GetNode<Label>("ModuleLabel");
        _label.Text = Module.GetDescription();
        _checkButton.Toggled += OnCheckButtonToggled;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _checkButton.Toggled -= OnCheckButtonToggled;
    }


    private void OnCheckButtonToggled(bool pressed)
    {
        if (pressed)
        {
            GodotSteamworksEditorPlugin.Instance.SteamPanel.AddPanel(Module);
        }
        else
        {
            GodotSteamworksEditorPlugin.Instance.SteamPanel.RemovePanel(Module);
        }
    }
}

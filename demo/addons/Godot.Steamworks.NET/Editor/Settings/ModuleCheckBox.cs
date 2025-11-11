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
    private bool _isSubscribed = false;

    public override void _Ready()
    {
        base._Ready();
        _checkButton = GetNode<CheckButton>("CheckButton");
        _label = GetNode<Label>("ModuleLabel");
        _label.Text = Module.GetDescription();

        if (!_isSubscribed)
        {
            _checkButton.Toggled += OnCheckButtonToggled;
            _isSubscribed = true;
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        if (_isSubscribed)
        {
            _checkButton.Toggled -= OnCheckButtonToggled;
            _isSubscribed = false;
        }
    }

    /// <summary>
    /// Disables the module checkbox and unchecks it
    /// </summary>
    public void DisableModule()
    {
        _checkButton.Disabled = true;
        _checkButton.ButtonPressed = false;
    }

    /// <summary>
    /// Enables the module checkbox
    /// </summary>
    public void EnableModule()
    {
        _checkButton.Disabled = false;
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

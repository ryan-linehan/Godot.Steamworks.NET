using Godot;
using Godot.Steamworks.Net.Editor.Achievements;

public partial class GDSteamworksDebugPanel : PanelContainer
{
    [Export]
    public Button OpenDebugPanelButton = null!;
    public AchievementsDebugPanel AchievementsDebugPanel = null!;
    public Button CloseButton = null!;
    public override void _Ready()
    {
        base._Ready();
        AchievementsDebugPanel = GetNode<AchievementsDebugPanel>("%Achievements");
        CloseButton = GetNode<Button>("%CloseButton");

        if (OpenDebugPanelButton != null)
        {
            OpenDebugPanelButton.Pressed += OnButtonOpenPressed;
            OpenDebugPanelButton.Pressed += OnAchievementsButtonPressed;
        }

        CloseButton.Pressed += OnButtonClosedPressed;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (OpenDebugPanelButton != null)
        {
            OpenDebugPanelButton.Pressed -= OnButtonOpenPressed;
        }
        CloseButton.Pressed -= OnButtonClosedPressed;
    }

    /// <summary>
    /// Opens the debug panel. Only functional in debug builds.
    /// </summary>
    public void Open()
    {
#if DEBUG
        Visible = true;
#else
        GD.PushWarning("GDSteamworksDebugPanel.Open() was called in a release build. The debug panel is disabled outside of debug builds.");
#endif
    }

    /// <summary>
    /// Closes the debug panel. Only functional in debug builds.
    /// </summary>
    public void Close()
    {
#if DEBUG
        Visible = false;
#else
        GD.PushWarning("GDSteamworksDebugPanel.Close() was called in a release build. The debug panel is disabled outside of debug builds.");
#endif
    }

    private void OnButtonOpenPressed()
    {
        Open();
    }

    private void OnButtonClosedPressed()
    {
        Close();
    }

    private void OnAchievementsButtonPressed()
    {
        AchievementsDebugPanel.Visible = true;
    }
}

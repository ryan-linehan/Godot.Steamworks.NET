using Godot;
using System;

namespace Godot.Steamworks.Net;


[Tool]
public partial class StatsTab : MarginContainer, ISteamPanelTab
{
    [Export]
    public PackedScene StatListItemScene = null!;
    [Export]
    public Control StatsContainer = null!;
    [Export]
    public Button ResetAllButton = null!;
    [Export]
    public Button RefreshButton = null!;

    public override void _Ready()
    {
        ResetAllButton.Pressed += OnResetAllButtonPressed;
        RefreshButton.Pressed += Init;
        base._Ready();
    }


    public override void _ExitTree()
    {
        ResetAllButton.Pressed -= OnResetAllButtonPressed;
        RefreshButton.Pressed -= Init;
        base._ExitTree();
    }


    private void OnResetAllButtonPressed()
    {
        GodotSteamworks.Stats.ResetAllStats(false);
        Init();
    }


    public void Init()
    {
        GodotSteamworks.Stats.Init();

        foreach (var item in StatsContainer.GetChildren())
        {
            item.QueueFree();
        }

        var stats = GodotSteamworks.Stats.GetStats();
        foreach (var item in stats)
        {
            GD.Print($"Stat Key: {item.Key}, Value: {item.Value}");
            var listItem = StatListItemScene.Instantiate<StatListItem>();
            StatsContainer.AddChild(listItem);
            listItem.SetStat(item);
        }
    }

    public void HidePanel()
    {
        Visible = false;
    }

    public void ShowPanel()
    {
        Visible = true;
    }
}

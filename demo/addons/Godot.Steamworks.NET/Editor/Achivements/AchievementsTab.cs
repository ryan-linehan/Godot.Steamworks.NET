using Godot;
using System;

namespace Godot.Steamworks.Net;


[Tool]
public partial class AchievementsTab : MarginContainer, ISteamPanelTab
{
    [Export]
    public PackedScene AchievementListItemScene = null!;
    [Export]
    public Control AchievementsContainer = null!;
    [Export]
    public Button ClearAllButton = null!;
    [Export]
    public Button UnlockAllButton = null!;
    [Export]
    public Button RefreshButton = null!;

    public override void _Ready()
    {
        ClearAllButton.Pressed += OnClearAllButtonPressed;
        UnlockAllButton.Pressed += OnUnlockAllButtonPressed;
        RefreshButton.Pressed += Init;
        base._Ready();
    }


    public override void _ExitTree()
    {
        ClearAllButton.Pressed -= OnClearAllButtonPressed;
        UnlockAllButton.Pressed -= OnUnlockAllButtonPressed;
        RefreshButton.Pressed -= Init;
        base._ExitTree();
    }


    private void OnUnlockAllButtonPressed()
    {
        foreach (var item in GodotSteamworks.Achievements.GetAchievements())
        {
            GodotSteamworks.Achievements.UnlockAchievement(item.Key);
        }
        Init();
    }


    private void OnClearAllButtonPressed()
    {
        foreach (var item in GodotSteamworks.Achievements.GetAchievements())
        {
            GodotSteamworks.Achievements.ClearAchievement(item.Key);
        }
        Init();
    }


    public void Init()
    {
        GodotSteamworks.Achievements.Init();

        foreach (var item in AchievementsContainer.GetChildren())
        {
            item.QueueFree();
        }

        var achievements = GodotSteamworks.Achievements.GetAchievements();
        foreach (var item in achievements)
        {
            GD.Print($"Achievement ID: {item.Key}, Unlocked: {item.Unlocked}");
            var listItem = AchievementListItemScene.Instantiate<AchievementListItem>();
            AchievementsContainer.AddChild(listItem);
            listItem.SetAchievement(item);
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

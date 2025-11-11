using Godot;
using System;

namespace Godot.Steamworks.Net;


[Tool]
public partial class AchievementsTab : MarginContainer, ISteamPanelTab
{
    [Export]
    public PackedScene AchievementListItemScene = null!;
    [Export]
    public Control AchievementsVBox = null!;
    public void Init()
    {
        GodotSteamworks.Achievements.Init();

        foreach (var item in AchievementsVBox.GetChildren())
        {
            item.QueueFree();
        }
        
        var achievements = GodotSteamworks.Achievements.GetAchievements();
        foreach(var item in achievements)
        {
            GD.Print($"Achievement ID: {item.Key}, Unlocked: {item.Unlocked}");
            var listItem = AchievementListItemScene.Instantiate<AchievementListItem>();
            AchievementsVBox.AddChild(listItem);
            listItem.SetAchievement(item.Key, item.Name, item.Description, item.Unlocked);
        }
    }

}

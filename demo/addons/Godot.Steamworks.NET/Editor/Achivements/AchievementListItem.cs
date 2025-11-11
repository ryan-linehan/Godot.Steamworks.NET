using Godot;
using Godot.Steamworks.Net;
using System;

[Tool]
public partial class AchievementListItem : Control
{
    [Export]
    Label DescriptionLabel = null!;
    [Export]
    Label NameLabel = null!;
    [Export]
    public Texture2D UnlockedIcon = null!;
    [Export]
    public Texture2D LockedIcon = null!;
    [Export]
    TextureRect IconTextureRect = null!;
    [Export]
    Texture2D NoIconAvailable = null!;
    [Export]
    public TextureRect AchievedTextureRect = null!;
    private bool isUnlocked = false;
    private string _achievementKey = string.Empty;
    private Button UnlockButton = null!;
    private Button ClearAchievementButton = null!;
    public override void _Ready()
    {
        UnlockButton = GetNode<Button>("%UnlockButton");
        ClearAchievementButton = GetNode<Button>("%ClearAchievementButton");
        UnlockButton.Pressed += OnUnlockButtonPressed;
        ClearAchievementButton.Pressed += OnClearAchievementButtonPressed;
        base._Ready();
    }

    private void OnClearAchievementButtonPressed()
    {
        GodotSteamworks.Achievements.ClearAchievement(_achievementKey);
        isUnlocked = GodotSteamworks.Achievements.IsAchievementUnlocked(_achievementKey);
        AchievedTextureRect.Texture = isUnlocked ? UnlockedIcon : LockedIcon;
    }

    public override void _ExitTree()
    {
        UnlockButton.Pressed -= OnUnlockButtonPressed;
        ClearAchievementButton.Pressed -= OnClearAchievementButtonPressed;
        base._ExitTree();
    }



    private void OnUnlockButtonPressed()
    {
        GodotSteamworks.Achievements.UnlockAchievement(_achievementKey);
        isUnlocked = GodotSteamworks.Achievements.IsAchievementUnlocked(_achievementKey);
        AchievedTextureRect.Texture = isUnlocked ? UnlockedIcon : LockedIcon;
    }


    public void SetAchievement(string achievementKey, string name, string description, bool unlocked)
    {
        _achievementKey = achievementKey;
        NameLabel.Text = name;
        DescriptionLabel.Text = description;
        isUnlocked = unlocked;
        // TODO: Load these from steam and store into temp files
        IconTextureRect.Texture = NoIconAvailable;
        AchievedTextureRect.Texture = unlocked ? UnlockedIcon : LockedIcon;
    }
}

using Godot;
using System;

[Tool]
public partial class AchievementListItem : VBoxContainer
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
    private bool isUnlocked = false;
    private string _achievementKey = string.Empty;
    public void SetAchievement(string achievementKey, string name, string description, bool unlocked)
    {
        _achievementKey = achievementKey;
        NameLabel.Text = name;
        DescriptionLabel.Text = description;
        isUnlocked = unlocked;
        IconTextureRect.Texture = unlocked ? UnlockedIcon : LockedIcon;
    }
}

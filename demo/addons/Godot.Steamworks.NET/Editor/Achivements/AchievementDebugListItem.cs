using Godot.Steamworks.Net.Models;

namespace Godot.Steamworks.Net.Editor.Achievements;

public partial class AchievementDebugListItem : Control
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
        IconTextureRect.Texture = GodotSteamworks.Achievements.GetAchievementIcon(_achievementKey) ?? NoIconAvailable;
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
        IconTextureRect.Texture = GodotSteamworks.Achievements.GetAchievementIcon(_achievementKey) ?? NoIconAvailable;
    }


    public void SetAchievement(Achievement achievement)
    {
        _achievementKey = achievement.Key;
        NameLabel.Text = achievement.Name;
        DescriptionLabel.Text = achievement.Description;
        isUnlocked = achievement.Unlocked;
        IconTextureRect.Texture = GodotSteamworks.Achievements.GetAchievementIcon(_achievementKey) ?? NoIconAvailable;
        AchievedTextureRect.Texture = isUnlocked ? UnlockedIcon : LockedIcon;
    }
}

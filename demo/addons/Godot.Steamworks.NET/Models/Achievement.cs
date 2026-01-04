namespace Godot.Steamworks.Net.Models;

/// <summary>
/// Class representing a Steam achievement
/// </summary>
public class Achievement
{
    /// <summary>
    /// The achievement identifier setup in Steamworks
    /// </summary>
    public string Key { get; set; }
    /// <summary>
    /// Whether the achievement is unlocked
    /// </summary>
    public bool Unlocked { get; set; }
    /// <summary>
    /// Whether this achievement is a progress-based achievement
    /// </summary>
    public bool IsProgressAchievement => MinProgress != 0 || MaxProgress != 0;
    /// <summary>
    /// The display name of the achievement
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The description of the achievement
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The minimum progress value for progress-based achievements
    /// </summary>
    public int MinProgress { get; set; }
    /// <summary>
    /// The maximum progress value for progress-based achievements
    /// </summary>
    public int MaxProgress { get; set; }
    /// <summary>
    /// The icon for the achievement as a Godot Texture2D
    ///  - Null if no icon is set
    ///  - Steam returns the icon based on the state of the achievement (unlocked vs locked) this texture will reflect the current state of the achivement
    /// </summary>
    public Texture2D? Icon { get; set; }

    public Achievement(string key, bool unlocked, string name, string description, int minProgress = 0, int maxProgress = 0)
    {
        Key = key;
        Unlocked = unlocked;
        Name = name;
        Description = description;
        MinProgress = minProgress;
        MaxProgress = maxProgress;
    }
}
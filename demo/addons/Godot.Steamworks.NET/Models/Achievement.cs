namespace Godot.Steamworks.Net.Models;

/// <summary>
/// Class representing a Steam achievement
/// </summary>
public class Achievement
{
    public string Key { get; set; }
    public bool Unlocked { get; set; }
    public bool IsProgressAchievement => MinProgress != 0 || MaxProgress != 0;
    public string Name { get; set; }
    public string Description { get; set; }
    public int MinProgress { get; set; }
    public int MaxProgress { get; set; }

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
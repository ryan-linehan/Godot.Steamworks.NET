namespace Godot.Steamworks.Net.Models;

/// <summary>
/// Class representing a Steam achievement
/// </summary>
public class Achievement
{
    public string Key { get; set; }
    public bool Unlocked { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public Achievement(string key, bool unlocked, string name, string description)
    {
        Key = key;
        Unlocked = unlocked;
        Name = name;
        Description = description;
    }
}
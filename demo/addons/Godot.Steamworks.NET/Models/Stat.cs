namespace Godot.Steamworks.Net.Models;

/// <summary>
/// Class representing a Steam stat
/// </summary>
public class Stat
{
    public string Key { get; set; }
    public string Name { get; set; }
    public float Value { get; set; }
    public StatType Type { get; set; }

    public Stat(string key, string name, float value, StatType type)
    {
        Key = key;
        Name = name;
        Value = value;
        Type = type;
    }
}

/// <summary>
/// Enum representing the type of a stat
/// </summary>
public enum StatType
{
    Int,
    Float
}

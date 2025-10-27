namespace Godot.Steamworks.Net;

using Godot;
public static class GodotSteamworksLogger
{
    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        if (level == LogLevel.None)
            return;

        if (level <= GodotSteamworks.LogLevel)
        {
            GD.Print("[GodotSteamworks] " + message);
        }
    }

    public static void LogError(string message)
    {
        Log(message, LogLevel.Error);
    }

    public static void LogWarning(string message)
    {
        Log(message, LogLevel.Warning);
    }

    public static void LogInfo(string message)
    {
        Log(message, LogLevel.Info);
    }

    public static void LogDebug(string message)
    {
        Log(message, LogLevel.Debug);
    }
}
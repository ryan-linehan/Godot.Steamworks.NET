using Godot;
using System;

namespace Godot.Steamworks.Net.Utils;

/// <summary>
/// Manages Steam API configuration for the editor plugin
/// </summary>
public static class SteamConfigManager
{
    private const string ConfigDirPath = "user://godot.steamworks.net";
    private const string ConfigFilePath = "user://godot.steamworks.net/config.cfg";
    private const string SectionName = "steam_api";
    private const string AppIdKey = "app_id";
    private const string ApiKeyKey = "api_key";

    /// <summary>
    /// Loads the Steam API configuration from file
    /// </summary>
    public static (string appId, string apiKey) LoadConfig()
    {
        var configFile = new ConfigFile();
        var error = configFile.Load(ConfigFilePath);

        if (error != Error.Ok)
        {
            return ("", "");
        }

        var appId = configFile.GetValue(SectionName, AppIdKey, "").AsString();
        var apiKey = configFile.GetValue(SectionName, ApiKeyKey, "").AsString();

        return (appId, apiKey);
    }

    /// <summary>
    /// Saves the Steam API configuration to file
    /// </summary>
    public static Error SaveConfig(string appId, string apiKey)
    {
        var configFile = new ConfigFile();

        // Create directory if it doesn't exist
        var dir = DirAccess.Open(ConfigDirPath);
        if (dir == null)
        {
            var makeError = DirAccess.MakeDirRecursiveAbsolute(ConfigDirPath);
            if (makeError != Error.Ok)
            {
                GD.PrintErr($"Failed to create config directory: {makeError}");
                return makeError;
            }
        }

        configFile.SetValue(SectionName, AppIdKey, appId);
        configFile.SetValue(SectionName, ApiKeyKey, apiKey);

        var saveError = configFile.Save(ConfigFilePath);
        if (saveError != Error.Ok)
        {
            GD.PrintErr($"Failed to save config: {saveError}");
            return saveError;
        }

        GD.Print($"Steam config saved successfully");
        return Error.Ok;
    }

    /// <summary>
    /// Checks if configuration exists and is valid
    /// </summary>
    public static bool HasValidConfig()
    {
        var (appId, apiKey) = LoadConfig();
        return !string.IsNullOrWhiteSpace(appId) && !string.IsNullOrWhiteSpace(apiKey);
    }
}

# Achievements API

The Godot.Steamworks.NET plugin provides an `Achievements` API that wraps Steam achievements functionality. It allows you to unlock, reset, and check achievement status with simple method calls, without dealing with low-level Steamworks.NET boilerplate.

## Basic Usage

### Unlocking an Achievement

```csharp
// Unlock an achievement by ID
GodotSteamworks.Instance.Achievements.Unlock("ACHIEVEMENT_ID");

// Steam will automatically display the achievement unlock notification
```

### Checking Achievement Status

```csharp
// Check if an achievement is unlocked
bool isUnlocked = GodotSteamworks.Instance.Achievements.IsUnlocked("ACHIEVEMENT_ID");

// Use this for conditional UI or gameplay logic
if (isUnlocked)
{
    // Show special cosmetic or unlock bonus content
}
```

### Resetting for Testing

```csharp
// Reset an achievement (useful for testing)
GodotSteamworks.Instance.Achievements.Reset("ACHIEVEMENT_ID");

// Reset all achievements
GodotSteamworks.Instance.Achievements.ResetAll();
```

## How should I implement achievements in my game?

The key to clean achievement integration is **creating an abstraction layer** that decouples your game from any specific platform. This allows you to:

- Support Steam on PC
- Support Game Center on iOS
- Support Google Play on Android
- Enable/disable different backends per target platform

### Recommended: Platform-Agnostic Achievements Singleton

Create a dedicated singleton that your game code calls, and this singleton delegates to the appropriate backend:

```csharp
public partial class AchievementSystem : Node
{
    public static AchievementSystem Instance { get; private set; }

    public override void _EnterTree()
    {
        if (Instance != null)
            QueueFree();
        else
            Instance = this;
    }

    /// <summary>
    /// Unlock an achievement. Platform is determined at runtime.
    /// </summary>
    public void Unlock(string achievementId)
    {
        #if GODOT_PC
            GodotSteamworks.Instance.Achievements.Unlock(achievementId);
        #elif GODOT_IOS
            // iOS: Use GameCenter integration
            IOSAchievementManager.Unlock(achievementId);
        #elif GODOT_ANDROID
            // Android: Use Google Play integration
            GooglePlayAchievementManager.Unlock(achievementId);
        #endif
    }

    public bool IsUnlocked(string achievementId)
    {
        #if GODOT_PC
            return GodotSteamworks.Instance.Achievements.IsUnlocked(achievementId);
        #elif GODOT_IOS
            return IOSAchievementManager.IsUnlocked(achievementId);
        #elif GODOT_ANDROID
            return GooglePlayAchievementManager.IsUnlocked(achievementId);
        #endif
        return false;
    }
}
```

### Game Code (Platform-Agnostic)

Your game logic now calls the abstraction, not Steam directly:

```csharp
public partial class GameManager : Node
{
    public void OnPlayerWon(int difficulty)
    {
        if (difficulty >= 3)
        {
            // Just call the achievement system - it handles which backend to use
            AchievementSystem.Instance.Unlock("DEFEAT_HARD_BOSS");
        }
    }

    public void OnPlayerCollectedItem(string itemType)
    {
        if (itemType == "rare_item" && !AchievementSystem.Instance.IsUnlocked("COLLECTOR"))
        {
            AchievementSystem.Instance.Unlock("COLLECTOR");
        }
    }
}
```

### Benefits

- **Platform independence**: Your game code doesn't know about Steam, GameCenter, or Google Play
- **Easy to enable/disable**: Comment out `#if` blocks to disable a platform
- **Testable**: Mock `AchievementSystem` in tests without Steam running
- **Maintainable**: Add new platforms by adding new `#if` blocks
- **Single point of control**: All achievement logic flows through one class

> **Note:** This API does not emit signals when achievements unlock. Handle reactions (UI notifications, unlocking rewards, etc.) in your own game logic!

## Debug and Testing

### GDSteamworksDebugPanel

The demo project includes a debug panel for managing achievements during development:

1. Open `addons/Godot.Steamworks.NET/Editor/GDSteamworksDebugPanel.tscn` in your editor
2. This provides a UI to unlock/reset achievements for testing
3. Useful for rapid iteration without replaying the game

**Important:** Don't ship this debug panel in your game build.

### Testing Without Steam

To test achievement logic without Steam running:

1. Mock the Achievements API in your test environment
2. Your game code doesn't need to know the difference
3. Integration with Steam only happens at the API boundary

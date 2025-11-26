# GodotSteamworks.NET

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Godot](https://img.shields.io/badge/Godot-4.4%2B-blue)](https://godotengine.org/)
[![Steamworks.NET](https://img.shields.io/badge/Steamworks.NET-2025.162.6-green)](https://github.com/rlabrecque/Steamworks.NET)

A Godot wrapper around [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) that makes Steam integration easy and intuitive for Godot 4.4+ C# projects. Get your indie game on Steam with minimal setup and maximum functionality.

## 🎯 What is This?

This project provides a Godot-friendly wrapper around Steamworks.NET, simplifying the integration of Steam features into your Godot C# games.

While many Steam solutions for Godot are GDScript-focused, this wrapper is designed specifically for C# developers, providing a clean API that feels natural in both Godot and C# contexts.

Rather than dealing with the complexities of Steamworks.NET directly, this wrapper handles the boilerplate and provides Godot-specific patterns, letting you focus on making your game.

## 🚀 Features

- **Steam Initialization** - Simplified Steam API setup with Godot lifecycle integration
- **P2P Networking** - Steam's networking capabilities wrapped for Godot's networking APIs
- **Achievements API** - Unlock, reset, and query Steam achievements easily from Godot C# scripts
- **CI/CD Pipeline** - Example GitHub Actions workflow for building and deploying Steam games
- **Multiplayer Demo Project** - A demo project with an example of how to connect to steam lobbies and have peers connect to the same game through steam P2P

## 🚧 In Development / Planned

- **Cloud Saves** - Simplify save files with multiple steam users on the same PC

## 📋 Prerequisites

- **Godot 4.4+** with **.NET/C# support**
- **Steamworks.NET.AnyCPU** package

## 📦 Installation

### 1. Install the Steamworks.NET.AnyCPU Package

Install the Steamworks.NET.AnyCPU package using the dotnet CLI:

```bash
dotnet add package Steamworks.NET.AnyCPU
```

Or add it directly to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Steamworks.NET.AnyCPU" Version="2025.162.6-b-socket.1" />
</ItemGroup>
```

> **Note:** The Steamworks.NET.AnyCPU package is still under active development and may not be fully production-ready. That said, it's been working reliably in our testing. Please report any issues!

### 2. Add the Godot.Steamworks.NET Plugin

Copy the `addons/Godot.Steamworks.NET/` folder from the demo project into your Godot project's `addons/` directory. Build your project first, then enable the plugin in your project settings under **Project > Project Settings > Plugins**.

## 🎮 Usage

### Accessing the Singleton

The `GodotSteamworks` class provides a singleton instance for accessing Steam functionality throughout your game:

```csharp
using Godot.Steamworks.Net;

// Check if Steam is initialized
if (GodotSteamworks.Instance.IsInitialized)
{
    GD.Print("Steam is ready!");
}

// Access lobby functionality
GodotSteamworks.Lobby.CreateLobby(/* ... */);
```

## 🏆 Achievements API

Easily integrate Steam achievements into your Godot C# game. Unlock, reset, and check achievement status with simple method calls.

```
GodotSteamworks.Instance.Achievements.Unlock("FIRST_WIN");
```

> Instantiate the debug panel to quickly reset achievements for testing.

See the full documentation: [docs/achievements.md](docs/achievements.md)

### Accessing the Full Steamworks.NET API

If the wrapper doesn't provide functionality you need, you can access the full Steamworks.NET library directly through the nuget package:

```csharp
using Steamworks;

// Use any Steamworks.NET API directly
var playerName = SteamFriends.GetPersonaName();
var appId = SteamUtils.GetAppID();
```

For more examples, like setting up peer-to-peer, check out the demo project in the `demo/` folder.

> If you can find it on Steam's [API reference](https://partner.steamgames.com/doc/api), you can likely use it with the Steamworks.NET API

## 🏗️ Project Structure

- `demo/` - Example Godot project demonstrating the wrapper's features
- `demo/addons/Godot.Steamworks.NET/` - The core wrapper plugin
- `docs/` - Detailed documentation for features like Achievements API
- `.github/workflows/` - Example CI/CD pipeline for building and deploying Steam games

## 🔗 Related Projects

Other Godot C# tools I maintain and use:

- 💻 **[Limbo Console C# Wrapper](https://github.com/ryan-linehan/limbo_console_sharp)** - NuGet package wrapper for the Limbo Console plugin. Excellent for debugging Steam integration during development.
- 🎮 **[Godot Input Icons](https://github.com/ryan-linehan/godot_input_icons)** - Runtime input icon display plugin. Perfect for showing controller inputs in Steam Deck games.

## 📄 License

This project is licensed under the MIT License

**Third-party licenses:**

- [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) - MIT License
- Steam SDK - Valve Corporation

## 💖 Support

If this plugin helps your game, consider:

- ⭐ Starring this repo
- 🎮 Checking out my games on [itch.io](https://rcubdev.itch.io)

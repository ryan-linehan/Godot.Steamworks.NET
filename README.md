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
- **CI/CD Pipeline** - Example GitHub Actions workflow for building and deploying Steam games
- **Multiplayer Demo Project** - A demo project with an example of how to connect to steam lobbies and have peers connect to the same game through steam P2P

## 🚧 In Development / Planned

- **Achievement System** - Simplify achievement management for steam
- **Cloud Saves** - Simplify save files with multiple steam users on the same pc

### Prerequisites

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

### Accessing the Full Steamworks.NET API

If the wrapper doesn't provide functionality you need, you can access the full Steamworks.NET library directly:

```csharp
using Steamworks;

// Use any Steamworks.NET API directly
var playerName = SteamFriends.GetPersonaName();
var appId = SteamUtils.GetAppID();
```

For more examples, like setting up peer to peer check out the demo project in the `demo/` folder.

## 🏗️ Project Structure

- `demo/` - Example Godot project demonstrating the wrapper's features
- `demo/addons/Godot.Steamworks.NET/` - The core wrapper plugin
- `.github/workflows/` - Example CI/CD pipeline for building and deploying Steam games

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

**Third-party licenses:**

- [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) - MIT License
- Steam SDK - Valve Corporation

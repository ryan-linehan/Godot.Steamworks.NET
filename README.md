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

### Prerequisites

- **Godot 4.4+** with **.NET/C# support**
- **Steamworks.NET.AnyCPU** package (included via local NuGet configuration)

## 📦 Installation

This project uses local NuGet package references for Steamworks.NET.AnyCPU. The demo project demonstrates how to integrate the wrapper into your Godot C# game.

> Currently the Steamworks.NET.AnyCPU is still being developed but this repository uses a version that is still a work in progress. See <https://github.com/Akarinnnnn/Steamworks.NET.AnyCPU/pull/9> for the branch we are using. We will use this until a new package is pushed with these fixes

1. Clone this repository and copy the `libs/` directory to the root of your project
2. Ensure you have a `NuGet.config` file in the root of your project. If you do not, create one.
3. Add the following to your `NuGet.config`:

    ```xml
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
    <packageSources>
        <add key="local" value="./libs" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    </packageSources>
    </configuration>
    ```

4. Finally the following to your `.csproj` file to reference the local NuGet package:

   ```xml
   <ItemGroup>
       <PackageReference Include="Steamworks.NET.AnyCPU" Version="2025.162.6-anycpu021" />
   </ItemGroup>
   ```

> Once the Steamworks.NET.AnyCPU package is updated on nuget.org you will simply be able to edit your csproj to add the package reference. This README's instructions will be updated to reflect the change once it is updated


## 🏗️ Project Structure

- `demo/` - Example Godot project demonstrating the wrapper's features
- `demo/addons/Godot.Steamworks.NET/` - The core wrapper plugin
- `libs/` - Local NuGet packages for Steamworks.NET.AnyCPU
- `NuGet.config` - NuGet configuration pointing to local packages
- `.github/workflows/` - Example CI/CD pipeline for building and deploying Steam games

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

**Third-party licenses:**

- [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) - MIT License
- Steam SDK - Valve Corporation

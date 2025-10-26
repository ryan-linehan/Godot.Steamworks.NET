# Steamworks.NET Library Distribution Strategy

## Approach: Embedded Libraries with Version Matching

### Why This Approach?

1. **Self-contained**: Users get everything they need in one package
2. **Version consistency**: Guaranteed compatibility between wrapper and Steamworks.NET
3. **Simplified deployment**: No external dependency management required
4. **Offline development**: Works without internet access

### Library Organization

```txt
libs/Steamworks.NET/
├── {version}/
│   ├── managed/
│   │   ├── net40/Steamworks.NET.dll
│   │   ├── net48/Steamworks.NET.dll
│   │   ├── net6.0/Steamworks.NET.dll
│   │   ├── net8.0/Steamworks.NET.dll
│   │   └── netstandard2.1/Steamworks.NET.dll
│   ├── native/
│   │   ├── win-x64/steam_api64.dll
│   │   ├── win-x86/steam_api.dll
│   │   ├── linux-x64/libsteam_api.so
│   │   └── osx-x64/libsteam_api.dylib
│   └── LICENSE.txt
```

### Versioning Strategy

- **Match Steamworks.NET version exactly**: `{year}.{steamworks_version}.{patch}-beta-001`
- **Example**: `2025.162.1-pkg.0.0.1` = Steamworks.NET 2025.162.1 + package version 0.0.1
- **Package semantic versioning**:
  - Major: Breaking changes to package API
  - Minor: New features (backward compatible)
  - Patch: Bug fixes and minor improvements

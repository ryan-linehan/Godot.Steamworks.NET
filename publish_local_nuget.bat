REM This batch file automates building and placing local changes in the Godot.Steamworks.NET project and its demo project.
@echo off

REM Read the version from the file first
set version_file=version.txt
if not exist %version_file% (
    echo ERROR: %version_file% not found. Ensure the file exists and contains the version.
    exit /b
)

REM Read the version from the file that outputs on Godot.Steamworks.NET build
set /p nuget_version=< version.txt

if "%nuget_version%" == "" (
    echo ERROR: Version not found in %version_file%. Make sure to build the plugin first!.
    exit /b
)

REM We need to bust the nuget cache or else we will continually have to increment version numbers
REM we only really want to increment version numbers when we are ready to publish
echo This will clear your nuget cache completely if you proceed -- packages will be restored during this process
echo Version to build: %nuget_version%
set /p continue=Continue (y/n)?
if "%continue%" == "y" (
    echo Continuing with version %nuget_version%
) else (
    echo Exiting script.
    exit /b
)

REM Navigate to the first folder and run a dotnet command
echo Changing directories to src/Godot.Steamworks.NET
cd src/Godot.Steamworks.NET

echo restoring Godot.Steamworks.NET version %nuget_version%
dotnet restore
echo building Godot.Steamworks.NET version %nuget_version%
dotnet build --configuration Release --no-restore
echo packing Godot.Steamworks.NET version %nuget_version%
dotnet pack --configuration Release --output ../../demo/nuget
echo nuget pack completed

cd ../../demo

echo Clearing nuget cache (global, http, temp)
dotnet nuget locals all --clear

REM Add the local nuget directory as a source if it doesn't exist
set local_source_path=%cd%\nuget
echo Adding local nuget source: %local_source_path%
dotnet nuget add source "%local_source_path%" --name "demo-local" 2>nul || echo Local source already exists or failed to add

dotnet add package Godot.Steamworks.NET --version %nuget_version%
dotnet clean
dotnet build

echo nuget package version %nuget_version% added to demo project.
echo you should restart your IDEs to get the updated intellisense for the package

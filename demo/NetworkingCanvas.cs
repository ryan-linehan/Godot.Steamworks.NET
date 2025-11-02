using Godot;
using Godot.Steamworks.Net;
using System;

/// <summary>
/// Communication hub between the Steam Lobby Menu and the Main scene
/// </summary>
public partial class NetworkingCanvas : CanvasLayer
{
    [Export]
    public NetworkingCanvasTypes StartingNetworkingType = NetworkingCanvasTypes.GodotENet;
    [Signal]
    public delegate void SignalGameHostReadyEventHandler();
    [Signal]
    public delegate void SignalGameJoinedEventHandler();
    [Export]
    public SteamLobbyMenu SteamLobbyMenu = null!;
    [Export]
    public GodotConnectMenu GodotMenu = null!;
    [Export]
    public Control SinglePlayerMenu = null!;
    [Export]
    public OptionButton MenuOptionButton = null!;

    public override void _Ready()
    {
        SteamLobbyMenu.SignalGameHostReady += EmitHostGame;
        SteamLobbyMenu.SignalGameJoined += EmitJoinGame;
        GodotMenu.SignalGameHostReady += EmitHostGame;
        GodotMenu.SignalGameJoined += EmitJoinGame;
        MenuOptionButton.ItemSelected += OnMenuOptionSelected;
        MenuOptionButton.Clear();
        foreach (NetworkingCanvasTypes type in Enum.GetValues(typeof(NetworkingCanvasTypes)))
        {
            if (type == NetworkingCanvasTypes.SteamworksNET && !GodotSteamworks.Instance.IsInitialized)
            {
                continue;
            }
            else
            {
                MenuOptionButton.AddItem(type.ToString(), (int)type);
            }
        }

        MenuOptionButton.Selected = (int)StartingNetworkingType;
        OnMenuOptionSelected(MenuOptionButton.Selected);
    }

    private void OnMenuOptionSelected(long optionId)
    {
        SteamLobbyMenu.Visible = optionId == (long)NetworkingCanvasTypes.SteamworksNET;
        GodotMenu.Visible = optionId == (long)NetworkingCanvasTypes.GodotENet;
        if (optionId == (long)NetworkingCanvasTypes.None)
        {
            SinglePlayerMenu.Visible = true;
            SteamLobbyMenu.Visible = false;
            GodotMenu.Visible = false;
        }
        else if (optionId == (long)NetworkingCanvasTypes.AutoDetect)
        {
            AutoDetectNetworking();
        }
    }

    private void EmitHostGame()
    {
        EmitSignal(nameof(SignalGameHostReady));
    }

    private void EmitJoinGame()
    {
        EmitSignal(nameof(SignalGameJoined));
    }

    private void AutoDetectNetworking()
    {
        if (GodotSteamworks.Instance.IsInitialized)
        {
            SteamLobbyMenu.Visible = true;
            GodotMenu.Visible = false;
        }
        else
        {
            SteamLobbyMenu.Visible = false;
            GodotMenu.Visible = true;
        }
    }
}

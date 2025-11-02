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
    public SinglePlayerMenu SinglePlayerMenu = null!;
    [Export]
    public OptionButton MenuOptionButton = null!;
    [Export]
    public Button RefreshButton = null!;

    public override async void _Ready()
    {
        await OnlineConnectionChecker.RefreshOnlineStatus();
        SinglePlayerMenu.SignalGameHostReady += EmitHostGame;
        SteamLobbyMenu.SignalGameHostReady += EmitHostGame;
        SteamLobbyMenu.SignalGameJoined += EmitJoinGame;
        GodotMenu.SignalGameHostReady += EmitHostGame;
        GodotMenu.SignalGameJoined += EmitJoinGame;
        MenuOptionButton.ItemSelected += OnMenuOptionSelected;
        MenuOptionButton.Clear();
        RefreshButton.Pressed += OnRefreshButtonPressed;
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

        MenuOptionButton.Selected = MenuOptionButton.GetItemIndex((int)StartingNetworkingType);
        OnMenuOptionSelected(MenuOptionButton.Selected);
    }

    private void OnRefreshButtonPressed()
    {
        AutoDetectNetworking();
    }


    private void OnMenuOptionSelected(long optionIdx)
    {
        var optionValue = MenuOptionButton.GetItemId((int)optionIdx);
        SteamLobbyMenu.Visible = optionValue == (long)NetworkingCanvasTypes.SteamworksNET;
        GodotMenu.Visible = optionValue == (long)NetworkingCanvasTypes.GodotENet;
        RefreshButton.Visible = false;
        if (optionValue == (long)NetworkingCanvasTypes.None)
        {
            SinglePlayerMenu.Visible = true;
            SteamLobbyMenu.Visible = false;
            GodotMenu.Visible = false;
        }
        else if (optionValue == (long)NetworkingCanvasTypes.AutoDetect)
        {
            AutoDetectNetworking();
            RefreshButton.Visible = true;
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

    private async void AutoDetectNetworking()
    {
        await OnlineConnectionChecker.RefreshOnlineStatus();        
        // Retry initializing GodotSteamworks if not already initialized
        if (!GodotSteamworks.Instance.IsInitialized)
        {
            GodotSteamworks.Instance.InitGodotSteamworks();
        }

        if (GodotSteamworks.Instance.IsInitialized && OnlineConnectionChecker.IsOnline)
        {
            var index = MenuOptionButton.GetItemIndex((int)NetworkingCanvasTypes.SteamworksNET);
            if (index == -1)
            {
                MenuOptionButton.AddItem(NetworkingCanvasTypes.SteamworksNET.ToString(), (int)NetworkingCanvasTypes.SteamworksNET);
            }
            SinglePlayerMenu.Visible = false;
            SteamLobbyMenu.Visible = true;
            GodotMenu.Visible = false;
        }
        else if (!OnlineConnectionChecker.IsOnline)
        {
            SinglePlayerMenu.Visible = true;
            SteamLobbyMenu.Visible = false;
            GodotMenu.Visible = false;
        }
        else if(OnlineConnectionChecker.IsOnline)
        {
            SteamLobbyMenu.Visible = false;
            GodotMenu.Visible = true;
            SinglePlayerMenu.Visible = false;
        }
    }
}

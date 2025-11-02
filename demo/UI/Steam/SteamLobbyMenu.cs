using Godot.Steamworks.Net;
using Godot;
using Steamworks;
using System;

/// <summary>
/// Example menu script to create or join a Steam lobby and start p2p connection over steam
/// </summary>
public partial class SteamLobbyMenu : Control
{
    /// <summary>
    /// Signals that the game should start for the given lobby id
    /// </summary>
    /// <param name="lobbyId"></param>
    [Signal]
    public delegate void SignalGameHostReadyEventHandler();
    /// <summary>
    /// Signals that the game should start and join the given lobby id
    /// </summary>
    /// <param name="lobbyId"></param>
    [Signal]
    public delegate void SignalGameJoinedEventHandler();
    /// <summary>
    /// Button to create a lobby
    /// </summary>
    [Export]
    public Button CreateLobbyButton = null!;
    /// <summary>
    /// Button to join a lobby
    /// </summary>
    [Export]
    public Button JoinLobbyButton = null!;
    /// <summary>
    /// Button to go back from lobby members list to lobby list
    /// </summary>
    [Export]
    public Button BackButton = null!;
    /// <summary>
    /// Button that starts the actual game for p2p connection via steam
    /// </summary>
    [Export]
    public Button StartGameButton = null!;
    /// <summary>
    /// Control to show the lobbies available for steam user to join
    /// </summary>
    [Export]
    public SteamLobbyList LobbyListMenu = null!;
    /// <summary>
    /// Control to show the members of the current lobby the steam user is in
    /// </summary>
    [Export]
    public SteamLobbyMembersList LobbyMembersListMenu = null!;
    /// <summary>
    /// Label that shows the current lobby id
    /// </summary>
    [Export]
    public Label LobbyIdLabel = null!;
    private ulong _lobbyId = 0;
    override public void _Ready()
    {
        BackButton.Visible = false;
        StartGameButton.Visible = false;
        CreateLobbyButton.Visible = true;
        JoinLobbyButton.Visible = true;
        CreateLobbyButton.Pressed += OnCreateLobbyButtonPressed;
        JoinLobbyButton.Pressed += OnJoinLobbyButtonPressed;
        BackButton.Pressed += OnBackButtonPressed;
        StartGameButton.Pressed += OnStartGameButtonPressed;

        // Subscribe to lobby joined event to update UI accordingly
        GodotSteamworks.Lobby.LobbyJoined += OnLobbyJoined;
        GodotSteamworks.Lobby.PlayerJoinedLobby += OnRemotePlayerLobbyStatusChanged;
        GodotSteamworks.Lobby.PlayerLeftLobby += OnRemotePlayerLobbyStatusChanged;

        GodotSteamworks.Lobby.LobbyDataUpdatedDetailed += (lobbyData) =>
        {
            // Update the members list when lobby data is updated
            if (lobbyData.TryGetValue("host_ready", out string? hostReady)
                 && hostReady != null && bool.TryParse(hostReady, out bool isReady) && isReady
                 && !GodotSteamworks.Lobby.IsLobbyOwner(_lobbyId))
            {
                // Signal to start peer connection as client
                EmitSignal(SignalName.SignalGameJoined);
            }
        };
    }


    private void OnStartGameButtonPressed()
    {
        StartHosting();
        EmitSignal(SignalName.SignalGameHostReady);
    }

    private void OnBackButtonPressed()
    {
        if (_lobbyId != 0)
        {
            GodotSteamworks.Lobby.LeaveLobby(_lobbyId);
            _lobbyId = 0;
            LobbyIdLabel.Text = "none";
        }
        LobbyMembersListMenu.Visible = false;
        LobbyListMenu.Visible = false;
        BackButton.Visible = false;
        StartGameButton.Visible = false;
        CreateLobbyButton.Visible = true;
        JoinLobbyButton.Visible = true;
    }

    private async void OnJoinLobbyButtonPressed()
    {
        LobbyListMenu.SetIsLoading(true);
        LobbyListMenu.Visible = true;
        var lobbies = await GodotSteamworks.Lobby.SearchLobbiesAsync();
        LobbyListMenu.PopulateLobbyList(lobbies);
        LobbyListMenu.AppendLobbies(GodotSteamworks.Lobby.GetFriendLobbies());
        LobbyListMenu.SetIsLoading(false);
    }

    private async void OnCreateLobbyButtonPressed()
    {
        var lobbyId = await GodotSteamworks.Lobby.CreateLobbyAsync(4, ELobbyType.k_ELobbyTypeFriendsOnly);
        if (lobbyId != null)
        {
            OnLobbyJoined(lobbyId.Value);
        }
    }

    /// <summary>
    /// Handler for when the local user has joined a lobby
    /// </summary>
    /// <param name="lobbyId"></param>
    private void OnLobbyJoined(ulong lobbyId)
    {
        _lobbyId = lobbyId;
        BackButton.Visible = true;
        if (SteamUser.GetSteamID().m_SteamID == GodotSteamworks.Lobby.GetLobbyOwner(lobbyId))
        {
            StartGameButton.Visible = true;
        }
        CreateLobbyButton.Visible = false;
        JoinLobbyButton.Visible = false;
        LobbyIdLabel.Text = lobbyId.ToString();
        LobbyListMenu.Visible = false;
        LobbyMembersListMenu.Visible = true;
        LobbyMembersListMenu.UpdateMembersList(lobbyId);
    }

    /// <summary>
    /// Handler for when a remote player joins the lobby
    /// </summary>
    /// <param name="lobbyId"></param>
    /// <param name="_"></param>
    /// <param name="__"></param>
    private void OnRemotePlayerLobbyStatusChanged(ulong lobbyId, ulong _, string __)
    {
        LobbyMembersListMenu.UpdateMembersList(_lobbyId);
    }

    /// <summary>
    /// Starts the host connection for godot's multiplayer via steam p2p
    /// </summary>
    private void StartHosting()
    {
        GD.Print("Hosting Game Session");
        try
        {
            var steamMultiplayerPeer = new SteamMultiplayerPeer();
            var steamErr = steamMultiplayerPeer.CreateServer(0);
            if (steamErr == Error.Ok)
            {
                Multiplayer.MultiplayerPeer = steamMultiplayerPeer;
                GD.Print("Hosting via Steam P2P successful");
                GodotSteamworks.Lobby.SetLobbyData(_lobbyId, "host_ready", "true");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to start hosting: {ex.Message}");
        }
    }

    /// <summary>
    /// Joins a godot multiplayer game via steam p2p
    /// </summary>
    private void ConnectToGameServer()
    {
        GD.Print("Joining Game Session");
        if (!GodotSteamworks.Instance.IsInitialized)
        {
            GD.PrintErr("GodotSteamworks is not initialized! Multiplayer only supported when Steam is running and initialized for the demo");
            return;
        }

        try
        {
            var steamMultiplayerPeer = new SteamMultiplayerPeer();
            var steamErr = steamMultiplayerPeer.CreateClient(GodotSteamworks.Lobby.GetLobbyOwner(_lobbyId), 0);
            if (steamErr == Error.Ok)
            {
                // Use the MultiplayerPeer property for Godot compatibility
                Multiplayer.MultiplayerPeer = steamMultiplayerPeer;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to join game: {ex.Message}");
        }
    }
}

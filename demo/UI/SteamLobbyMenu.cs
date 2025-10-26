using Godot.Steamworks.NET;
using Godot;
using Steamworks;

/// <summary>
/// Example menu script to create or join a Steam lobby and start p2p connection over steam
/// </summary>
public partial class SteamLobbyMenu : Control
{
    /// <summary>
    /// Signals that the game should start for the given lobby idx
    /// </summary>
    /// <param name="lobbyId"></param>
    [Signal]
    public delegate void SignalStartGameEventHandler(ulong lobbyId);
    /// <summary>
    /// Button to create a lobby
    /// </summary>
    [Export]
    public Button CreateLobbyButton;
    /// <summary>
    /// Button to join a lobby
    /// </summary>
    [Export]
    public Button JoinLobbyButton;
    /// <summary>
    /// Button to go back from lobby members list to lobby list
    /// </summary>
    [Export]
    public Button BackButton;
    /// <summary>
    /// Button that starts the actual game for p2p connection via steam
    /// </summary>
    [Export]
    public Button StartGameButton;
    /// <summary>
    /// Control to show the lobbies available for steam user to join
    /// </summary>
    [Export]
    public LobbyList LobbyListMenu;
    /// <summary>
    /// Control to show the members of the current lobby the steam user is in
    /// </summary>
    [Export]
    public LobbyMembersList LobbyMembersListMenu;
    /// <summary>
    /// Label that shows the current lobby id
    /// </summary>
    [Export]
    public Label LobbyIdLabel;
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

        // Subscribe to lobby joined event to update UI accordingly
        GodotSteamworks.Lobby.LobbyJoined += OnLobbyJoined;
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
}

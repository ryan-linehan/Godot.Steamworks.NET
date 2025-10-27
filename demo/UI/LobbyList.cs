using Godot;
using Godot.Steamworks.Net;
using System;

public partial class LobbyList : Panel
{
    /// <summary>
    /// Emitted when a user joins a lobby from the LobbyList
    /// </summary>
    /// <param name="lobbyId"></param>
    [Signal]
    public delegate void SignalLobbyJoinedEventHandler(ulong lobbyId);
    private bool _isJoining = false;
    [Export]
    VBoxContainer _lobbyListContainer = null!;
    [Export]
    public Label _loadingLabel = null!;
    public override void _Ready()
    {
        Visible = false;
    }

    public void SetIsLoading(bool isLoading)
    {
        _loadingLabel.Visible = isLoading;
    }

    public void PopulateLobbyList(ulong[] lobbyIds)
    {
        foreach (var item in _lobbyListContainer.GetChildren())
        {
            item.QueueFree();
        }

        foreach (var lobbyId in lobbyIds)
        {
            Button joinLobbyButton = new Button();
            joinLobbyButton.Text = lobbyId.ToString();

            async void OnJoinLobbyButtonPressed()
            {
                if (_isJoining)
                {
                    GD.Print("Already joining a lobby, please wait...");
                    return;
                }

                _isJoining = true;
                _loadingLabel.Visible = true;
                _loadingLabel.Text = "Joining...";
                var result = await GodotSteamworks.Lobby.JoinLobbyAsync(lobbyId);

                if (result)
                {
                    Visible = false;
                    EmitSignal(SignalName.SignalLobbyJoined, lobbyId);
                }
                else
                {
                    _loadingLabel.Text = "Failed to join lobby: " + lobbyId;
                }
                _isJoining = false;
            }

            void CleanupButton()
            {
                joinLobbyButton.Pressed -= OnJoinLobbyButtonPressed;
                joinLobbyButton.TreeExiting -= CleanupButton;
            }

            joinLobbyButton.Pressed += OnJoinLobbyButtonPressed;
            joinLobbyButton.TreeExiting += CleanupButton;

            _lobbyListContainer.AddChild(joinLobbyButton);
        }
    }

}

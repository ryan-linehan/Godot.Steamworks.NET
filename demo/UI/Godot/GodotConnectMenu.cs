using Godot;
using System;

public partial class GodotConnectMenu : Control
{
    [Export]
    public int MaxPlayers = 4;
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
    [Export]
    public GodotHostMenu HostMenu = null!;
    [Export]
    public GodotJoinMenu JoinMenu = null!;
    [Export]
    public Control HostJoinButtonContainer = null!;
    [Export]
    public Button HostButton = null!;
    [Export]
    public Button JoinButton = null!;
    public override void _Ready()
    {
        HostMenu.Visible = false;
        JoinMenu.Visible = false;
        HostMenu.HostGameButton.Pressed += OnStartGameButtonPressed;
        JoinMenu.JoinGameButton.Pressed += OnJoinGameButtonPressed;
        HostButton.Pressed += SetupHostButtonPressed;
        JoinButton.Pressed += OnSetupJoinButtonPressed;

        HostMenu.BackButton.Pressed += OnBackPressed;
        JoinMenu.BackButton.Pressed += OnBackPressed;
    }

    private void SetupHostButtonPressed()
    {
        HostMenu.Visible = true;
        HostJoinButtonContainer.Visible = false;
    }

    private void OnSetupJoinButtonPressed()
    {
        JoinMenu.Visible = true;
        HostJoinButtonContainer.Visible = false;
    }

    private void OnBackPressed()
    {
        HostJoinButtonContainer.Visible = true;
        HostMenu.Visible = false;
        JoinMenu.Visible = false;
    }

    private void OnStartGameButtonPressed()
    {
        StartHosting();
        EmitSignal(SignalName.SignalGameHostReady);
    }

    private void StartHosting()
    {
        GD.Print("Hosting Game Session");
        try
        {
            var godotMultiplayerPeer = new ENetMultiplayerPeer();
            
            string port = HostMenu.Port.Text;
            int portNumber = -1;
            if (string.IsNullOrWhiteSpace(HostMenu.Port.Text))
            {
                GD.Print("Empty port number, using placeholder port");
                portNumber = HostMenu.Port.PlaceholderText.ToInt();
            }
            else if (!int.TryParse(port, out portNumber))
            {
                GD.Print("Invalid port number, using placeholder port");
            }

            var err = godotMultiplayerPeer.CreateServer(portNumber, MaxPlayers);
            if (err == Error.Ok)
            {
                Multiplayer.MultiplayerPeer = godotMultiplayerPeer;
                GD.Print("Hosting via godot enet successful");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to start hosting: {ex.Message}");
        }
    }

    private void OnJoinGameButtonPressed()
    {
        ConnectToGameServer();        
    }

    /// <summary>
    /// Joins a godot multiplayer game via godot's p2p
    /// </summary>
    private void ConnectToGameServer()
    {
        GD.Print("Joining Game Session");

        try
        {
            var multiplayerPeer = new ENetMultiplayerPeer();
            int port = -1;
            if (string.IsNullOrWhiteSpace(JoinMenu.Port.Text) || string.IsNullOrWhiteSpace(JoinMenu.IpAddress.Text))
            {
                GD.Print("IP Address or Port is empty, using placeholders");
                JoinMenu.IpAddress.Text = JoinMenu.IpAddress.PlaceholderText;
                port = JoinMenu.Port.PlaceholderText.ToInt();
            }
            else if (!int.TryParse(JoinMenu.Port.Text, out port))
            {
                GD.Print("Invalid port number, using placeholders");
                port = JoinMenu.Port.PlaceholderText.ToInt();
            }

            GD.Print("Hosting on port: " + port);
            
            var err = multiplayerPeer.CreateClient(JoinMenu.IpAddress.Text, port);
            if (err == Error.Ok)
            {
                // Use the MultiplayerPeer property for Godot compatibility
                Multiplayer.MultiplayerPeer = multiplayerPeer;
                EmitSignal(SignalName.SignalGameJoined);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to join game: {ex.Message}");
        }
    }
}

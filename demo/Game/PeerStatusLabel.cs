using Godot;
using System;

public partial class PeerStatusLabel : Label
{
    MultiplayerPeer.ConnectionStatus status;
    public override void _Process(double delta)
    {
        base._Process(delta);
        var newStatus = Multiplayer.MultiplayerPeer.GetConnectionStatus();
        if (newStatus != status)
        {
            status = newStatus;
            Text = status.ToString();
        }
    }

}

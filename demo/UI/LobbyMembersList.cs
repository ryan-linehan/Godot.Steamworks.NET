using Godot;
using Godot.Steamworks.Net;
using System;

public partial class LobbyMembersList : Panel
{
    [Export]
    VBoxContainer _membersListContainer;
    public override void _Ready()
    {
        Visible = false;
    }

    public void UpdateMembersList(ulong lobbyId)
    {
        foreach (var item in _membersListContainer.GetChildren())
        {
            item.QueueFree();
        }

        var lobbyOwner = GodotSteamworks.Lobby.GetLobbyOwner(lobbyId);
        var lobbyMembers = GodotSteamworks.Lobby.GetLobbyMembers(lobbyId);
        foreach (var memberId in lobbyMembers)
        {
            Label memberLabel = new Label();
            memberLabel.Text += memberId.SteamDisplayName.ToString();
            if(memberId.SteamId == lobbyOwner)
            {
                memberLabel.Text += " (Host)";
            }
            _membersListContainer.AddChild(memberLabel);
        }
    }
}
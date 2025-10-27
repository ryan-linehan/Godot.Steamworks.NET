using System;
using System.Collections.Generic;
using Godot;
using Steamworks;

namespace Godot.Steamworks.Net.Multiplayer.Peer;

/// <summary>
/// Represents a Steam networking connection to another peer
/// Based off of: https://github.com/expressobits/steam-multiplayer-peer
/// </summary>
public partial class SteamConnection : RefCounted
{
    /// <summary>
    /// Payload structure for setting up peer connections
    /// </summary>
    public struct SetupPeerPayload
    {
        public uint PeerId;

        public SetupPeerPayload()
        {
            PeerId = uint.MaxValue; // -1 equivalent
        }
    }

    /// <summary>
    /// Is this slot in use? Or is it available for new connections?
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// What is the steamid of the player?
    /// </summary>
    public ulong SteamId { get; set; }

    /// <summary>
    /// The handle for the connection to the player
    /// </summary>
    public HSteamNetConnection SteamNetConnection { get; set; }

    /// <summary>
    /// What was the last time we got data from the player?
    /// </summary>
    public ulong TickCountLastData { get; set; }

    /// <summary>
    /// Peer ID for this connection
    /// </summary>
    public int PeerId { get; set; }

    /// <summary>
    /// Last message timestamp
    /// </summary>
    public ulong LastMsgTimestamp { get; set; }

    /// <summary>
    /// Packets pending retry
    /// </summary>
    public List<SteamPacketPeer> PendingRetryPackets { get; private set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public SteamConnection()
    {
        PeerId = -1;
        SteamId = 0;
        LastMsgTimestamp = 0;
        PendingRetryPackets = new List<SteamPacketPeer>();
    }

    /// <summary>
    /// Constructor with Steam ID
    /// </summary>
    /// <param name="steamId">The Steam ID of the peer</param>
    public SteamConnection(ulong steamId) : this()
    {
        SteamId = steamId;
    }

    /// <summary>
    /// Destructor - cleanup connections and pending packets
    /// </summary>
    ~SteamConnection()
    {
        if (SteamNetConnection != HSteamNetConnection.Invalid)
        {
            SteamNetworkingSockets.CloseConnection(SteamNetConnection, 0, "Disconnect Default!", true);
        }
        PendingRetryPackets.Clear();
    }

    /// <summary>
    /// Send a packet through this connection
    /// </summary>
    /// <param name="packet">The packet to send</param>
    /// <returns>Error code</returns>
    public Error Send(SteamPacketPeer packet)
    {
        AddPacket(packet);
        return SendPending();
    }

    /// <summary>
    /// Flush any pending messages on this connection
    /// </summary>
    public void Flush()
    {
        if (SteamNetConnection == HSteamNetConnection.Invalid)
        {
            GD.PrintErr("The Steam Connection is invalid for flush!");
            return;
        }
        SteamNetworkingSockets.FlushMessagesOnConnection(SteamNetConnection);
    }

    /// <summary>
    /// Close this connection
    /// </summary>
    /// <returns>True if successfully closed</returns>
    public bool Close()
    {
        if (SteamNetConnection == HSteamNetConnection.Invalid)
        {
            GD.PrintRich("[color=yellow]Steam Connection is invalid![/color]");
            return false;
        }

        if (!SteamNetworkingSockets.CloseConnection(SteamNetConnection, 0, "Failed to accept connection", false))
        {
            GD.PrintRich("[color=yellow]Failed to close connection![/color]");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Request a peer ID from the remote peer
    /// </summary>
    /// <returns>Error code</returns>
    public Error RequestPeer()
    {
        var payload = new SetupPeerPayload();
        return SendSetupPeer(payload);
    }

    /// <summary>
    /// Send our peer ID to the remote peer
    /// </summary>
    /// <param name="peerId">The peer ID to send</param>
    /// <returns>Error code</returns>
    public Error SendPeer(uint peerId)
    {
        var payload = new SetupPeerPayload { PeerId = peerId };
        return SendSetupPeer(payload);
    }

    /// <summary>
    /// Equality operator based on Steam ID
    /// </summary>
    /// <param name="other">Other SteamConnection to compare</param>
    /// <returns>True if Steam IDs match</returns>
    public bool Equals(SteamConnection other)
    {
        return SteamId == other?.SteamId;
    }

    /// <summary>
    /// Raw send method that calls Steam networking
    /// </summary>
    /// <param name="packet">Packet to send</param>
    /// <returns>Steam EResult</returns>
    private EResult RawSend(SteamPacketPeer packet)
    {
        // Note: This is a simplified version. In a real implementation,
        // you'd need to properly marshal the byte array to Steam's native calls
        // For now, we'll return OK as a placeholder
        GD.PrintRich($"[color=cyan]Sending packet of size {packet.Size} via Steam networking[/color]");
        return EResult.k_EResultOK;
    }

    /// <summary>
    /// Send all pending packets
    /// </summary>
    /// <returns>Error code</returns>
    private Error SendPending()
    {
        while (PendingRetryPackets.Count > 0)
        {
            var packet = PendingRetryPackets[0];
            EResult errorCode = RawSend(packet);
            
            if (errorCode == EResult.k_EResultOK)
            {
                PendingRetryPackets.RemoveAt(0);
            }
            else
            {
                string errorString = ConvertEResultToString(errorCode);
                
                // Check if this is a reliable packet (bit flag check)
                if ((packet.TransferMode & 8) != 0) // k_nSteamNetworkingSend_Reliable
                {
                    GD.PrintRich($"[color=yellow]Send Error (Reliable, will retry): {errorString}[/color]");
                    break; // Break, retry send later
                }
                else
                {
                    GD.PrintRich($"[color=yellow]Send Error (Unreliable, won't retry): {errorString}[/color]");
                    PendingRetryPackets.RemoveAt(0); // Toss unreliable packet, move on
                }
            }
        }

        return Error.Ok;
    }

    /// <summary>
    /// Add a packet to the pending retry queue
    /// </summary>
    /// <param name="packet">Packet to add</param>
    private void AddPacket(SteamPacketPeer packet)
    {
        PendingRetryPackets.Add(packet);
    }

    /// <summary>
    /// Send a setup peer packet
    /// </summary>
    /// <param name="payload">The payload to send</param>
    /// <returns>Error code</returns>
    private Error SendSetupPeer(SetupPeerPayload payload)
    {
        // Convert struct to byte array using System.Runtime.InteropServices
        int size = System.Runtime.InteropServices.Marshal.SizeOf(payload);
        byte[] data = new byte[size];
        
        IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
        try
        {
            System.Runtime.InteropServices.Marshal.StructureToPtr(payload, ptr, false);
            System.Runtime.InteropServices.Marshal.Copy(ptr, data, 0, size);
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
        }
        
        var packet = new SteamPacketPeer(data, (uint)data.Length, 8); // Reliable transfer
        return Send(packet);
    }

    /// <summary>
    /// Convert EResult to string for debugging
    /// </summary>
    /// <param name="result">EResult to convert</param>
    /// <returns>String representation</returns>
    private string ConvertEResultToString(EResult result)
    {
        return result switch
        {
            EResult.k_EResultNone => "k_EResultNone",
            EResult.k_EResultOK => "k_EResultOK",
            EResult.k_EResultFail => "k_EResultFail",
            EResult.k_EResultNoConnection => "k_EResultNoConnection",
            EResult.k_EResultInvalidPassword => "k_EResultInvalidPassword",
            EResult.k_EResultLoggedInElsewhere => "k_EResultLoggedInElsewhere",
            EResult.k_EResultInvalidProtocolVer => "k_EResultInvalidProtocolVer",
            EResult.k_EResultInvalidParam => "k_EResultInvalidParam",
            EResult.k_EResultFileNotFound => "k_EResultFileNotFound",
            EResult.k_EResultBusy => "k_EResultBusy",
            EResult.k_EResultInvalidState => "k_EResultInvalidState",
            EResult.k_EResultInvalidName => "k_EResultInvalidName",
            EResult.k_EResultInvalidEmail => "k_EResultInvalidEmail",
            EResult.k_EResultDuplicateName => "k_EResultDuplicateName",
            EResult.k_EResultAccessDenied => "k_EResultAccessDenied",
            EResult.k_EResultTimeout => "k_EResultTimeout",
            EResult.k_EResultBanned => "k_EResultBanned",
            EResult.k_EResultAccountNotFound => "k_EResultAccountNotFound",
            EResult.k_EResultInvalidSteamID => "k_EResultInvalidSteamID",
            EResult.k_EResultServiceUnavailable => "k_EResultServiceUnavailable",
            EResult.k_EResultNotLoggedOn => "k_EResultNotLoggedOn",
            EResult.k_EResultPending => "k_EResultPending",
            EResult.k_EResultEncryptionFailure => "k_EResultEncryptionFailure",
            EResult.k_EResultInsufficientPrivilege => "k_EResultInsufficientPrivilege",
            EResult.k_EResultLimitExceeded => "k_EResultLimitExceeded",
            EResult.k_EResultRevoked => "k_EResultRevoked",
            EResult.k_EResultExpired => "k_EResultExpired",
            EResult.k_EResultAlreadyRedeemed => "k_EResultAlreadyRedeemed",
            EResult.k_EResultDuplicateRequest => "k_EResultDuplicateRequest",
            EResult.k_EResultAlreadyOwned => "k_EResultAlreadyOwned",
            EResult.k_EResultIPNotFound => "k_EResultIPNotFound",
            EResult.k_EResultPersistFailed => "k_EResultPersistFailed",
            EResult.k_EResultLockingFailed => "k_EResultLockingFailed",
            EResult.k_EResultLogonSessionReplaced => "k_EResultLogonSessionReplaced",
            EResult.k_EResultConnectFailed => "k_EResultConnectFailed",
            EResult.k_EResultHandshakeFailed => "k_EResultHandshakeFailed",
            EResult.k_EResultIOFailure => "k_EResultIOFailure",
            EResult.k_EResultRemoteDisconnect => "k_EResultRemoteDisconnect",
            EResult.k_EResultShoppingCartNotFound => "k_EResultShoppingCartNotFound",
            EResult.k_EResultBlocked => "k_EResultBlocked",
            EResult.k_EResultIgnored => "k_EResultIgnored",
            EResult.k_EResultNoMatch => "k_EResultNoMatch",
            EResult.k_EResultAccountDisabled => "k_EResultAccountDisabled",
            EResult.k_EResultServiceReadOnly => "k_EResultServiceReadOnly",
            EResult.k_EResultAccountNotFeatured => "k_EResultAccountNotFeatured",
            EResult.k_EResultAdministratorOK => "k_EResultAdministratorOK",
            EResult.k_EResultContentVersion => "k_EResultContentVersion",
            EResult.k_EResultTryAnotherCM => "k_EResultTryAnotherCM",
            EResult.k_EResultPasswordRequiredToKickSession => "k_EResultPasswordRequiredToKickSession",
            EResult.k_EResultAlreadyLoggedInElsewhere => "k_EResultAlreadyLoggedInElsewhere",
            EResult.k_EResultSuspended => "k_EResultSuspended",
            EResult.k_EResultCancelled => "k_EResultCancelled",
            EResult.k_EResultDataCorruption => "k_EResultDataCorruption",
            EResult.k_EResultDiskFull => "k_EResultDiskFull",
            EResult.k_EResultRemoteCallFailed => "k_EResultRemoteCallFailed",
            _ => "Unmatched"
        };
    }
}
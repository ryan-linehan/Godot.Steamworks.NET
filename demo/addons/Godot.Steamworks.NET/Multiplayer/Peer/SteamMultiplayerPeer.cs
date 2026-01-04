#if GODOT_PC
using Godot;
using System;
using System.Collections.Generic;
using Steamworks;
using Godot.Collections;
using System.Linq;

namespace Godot.Steamworks.Net.Multiplayer.Peer;

/// <summary>
/// A multiplayer peer implementation for Steam networking.
/// Based off of: https://github.com/expressobits/steam-multiplayer-peer
/// </summary>
public partial class SteamMultiplayerPeer : MultiplayerPeerExtension
{
    private const int MaxMessageCount = 255;
    private const int MaxSteamPacketSize = 512 * 1024;
    /// <summary>
    /// If true, disables Nagle's algorithm for sending packets.
    /// Steam recommends you understand the implications before changing this setting. Since
    /// Godot's high level multiplayer batches the packets for us anyways generally this is defaulted to true.
    /// Most real time games will want this enabled to reduce latency.
    /// </summary>
    public bool NoNagle { get; set; } = true;
    /// <summary>
    /// If true, enables TCP_NODELAY on the connection.
    /// Which means - if the message cannot be sent very soon (because the connection is still doing some initial handshaking, route negotiations, etc), then just drop it. 
    /// This is only applicable for unreliable messages. Using this flag on reliable messages is invalid.
    /// </summary>
    public bool NoDelay { get; set; } = false;

    private System.Collections.Generic.Dictionary<ulong, SteamConnection> connectionsBySteamId64 = new System.Collections.Generic.Dictionary<ulong, SteamConnection>();
    private System.Collections.Generic.Dictionary<int, SteamConnection> peerIdToSteamId = new System.Collections.Generic.Dictionary<int, SteamConnection>();
    private int transferMode = (int)TransferModeEnum.Reliable;
    private HSteamListenSocket listenSocket = HSteamListenSocket.Invalid;
    private Queue<SteamPacketPeer> incomingPackets = new Queue<SteamPacketPeer>();
    private ConnectionStatus connectionStatus = ConnectionStatus.Disconnected;
    private MultiplayerPeerMode mode = MultiplayerPeerMode.NONE;
    private int targetPeer = -1;
    private uint uniqueId = 0;

    private Godot.Collections.Dictionary<ESteamNetworkingConfigValue, Variant> _configs = new Godot.Collections.Dictionary<ESteamNetworkingConfigValue, Variant>();
    public Godot.Collections.Dictionary<ESteamNetworkingConfigValue, Variant> Configs
    {
        set
        {
            _configs = new Godot.Collections.Dictionary<ESteamNetworkingConfigValue, Variant>();
            foreach (var item in value)
            {
                _configs.Add(item.Key, item.Value);
            }
        }
        get
        {
            return new Godot.Collections.Dictionary<ESteamNetworkingConfigValue, Variant>(_configs ?? new Godot.Collections.Dictionary<ESteamNetworkingConfigValue, Variant>());
        }
    }

    private Callback<SteamNetConnectionStatusChangedCallback_t>? m_SteamNetConnectionStatusChangedCallback;

    public SteamMultiplayerPeer()
    {
        m_SteamNetConnectionStatusChangedCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnNetworkConnectionStatusChanged);
        Configs = new Godot.Collections.Dictionary<ESteamNetworkingConfigValue, Variant>();
    }

    public Error CreateHost(int localVirtualPort)
    {
        if (IsActive())
        {
            GD.PrintErr("The multiplayer instance is already active");
            return Error.AlreadyInUse;
        }
        SteamNetworkingUtils.InitRelayNetworkAccess();

        listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(localVirtualPort, 0, null);

        if (listenSocket == HSteamListenSocket.Invalid)
        {
            return Error.CantCreate;
        }

        uniqueId = 1;
        mode = MultiplayerPeerMode.HOST;
        connectionStatus = ConnectionStatus.Connected;
        return Error.Ok;
    }

    public Error CreateClient(ulong identityRemote, int remoteVirtualPort)
    {
        if (IsActive())
        {
            GD.PrintErr("The multiplayer instance is already active");
            return Error.AlreadyInUse;
        }
        uniqueId = GenerateUniqueId();
        SteamNetworkingUtils.InitRelayNetworkAccess();

        CSteamID steamId = new CSteamID(identityRemote);
        SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
        identity.SetSteamID(steamId);

        HSteamNetConnection connection = SteamNetworkingSockets.ConnectP2P(ref identity, remoteVirtualPort, 0, null);

        if (connection == HSteamNetConnection.Invalid)
        {
            uniqueId = 0;
            GD.PrintErr("Failed to connect; connection is invalid");
            return Error.CantConnect;
        }

        mode = MultiplayerPeerMode.CLIENT;
        connectionStatus = ConnectionStatus.Connecting;
        return Error.Ok;
    }

    public override byte[] _GetPacketScript()
    {
        if (incomingPackets.Count == 0)
        {
            return new byte[] { };
        }

        SteamPacketPeer nextReceivedPacket = incomingPackets.Dequeue();
        return nextReceivedPacket.Data;
    }

    public override Error _PutPacketScript(byte[] buffer)
    {
        if (!IsActive() || connectionStatus != ConnectionStatus.Connected || !peerIdToSteamId.ContainsKey(Math.Abs(targetPeer)))
        {
            return Error.Unconfigured;
        }

        int packetTransferMode = GetSteamTransferFlag();

        if (targetPeer == 0)
        {
            Error returnValue = Error.Ok;
            foreach (var connection in connectionsBySteamId64)
            {
                SteamPacketPeer packet = new SteamPacketPeer(buffer, transferMode: packetTransferMode);
                Error errorCode = connection.Value.Send(packet);
                if (errorCode != Error.Ok)
                {
                    returnValue = errorCode;
                }
            }
            return returnValue;
        }
        else
        {
            SteamPacketPeer packet = new SteamPacketPeer(buffer, transferMode: packetTransferMode);
            SteamConnection? connection = GetConnectionByPeer(targetPeer);
            if (connection == null)
            {
                return Error.Unconfigured;
            }            
            Error result = connection.Send(packet);
            return result;
        }
    }

    public override int _GetAvailablePacketCount()
    {
        return incomingPackets.Count;
    }

    public override int _GetMaxPacketSize()
    {
        return MaxSteamPacketSize;
    }

    public override TransferModeEnum _GetPacketMode()
    {
        if (!IsActive() || incomingPackets.Count == 0)
        {
            return TransferModeEnum.Reliable;
        }

        return incomingPackets.Peek().TransferMode == Constants.k_nSteamNetworkingSend_Reliable
            ? TransferModeEnum.Reliable
            : TransferModeEnum.Unreliable;
    }

    public override void _SetTransferMode(TransferModeEnum mode)
    {
        transferMode = (int)mode;
    }

    public override TransferModeEnum _GetTransferMode()
    {
        return (TransferModeEnum)transferMode;
    }

    public override void _SetTransferChannel(int pChannel)
    {
        // Channels not implemented yet
    }

    public override int _GetTransferChannel()
    {
        // Channels not implemented yet
        return 0;
    }

    public override void _SetTargetPeer(int peer)
    {
        targetPeer = peer;
    }

    public override int _GetPacketPeer()
    {
        if (!IsActive() || incomingPackets.Count == 0)
        {
            return 1;
        }

        return connectionsBySteamId64[incomingPackets.Peek().Sender].PeerId;
    }

    public override int _GetPacketChannel()
    {
        return 0;
    }

    public override bool _IsServer()
    {
        return uniqueId == 1;
    }

    public bool IsActive()
    {
        return mode != MultiplayerPeerMode.NONE;
    }

    public override void _Poll()
    {
        if (!IsActive())
        {
            return;
        }

        foreach (var entry in connectionsBySteamId64)
        {
            IntPtr[] messages = new IntPtr[MaxMessageCount];
            int numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(entry.Value.ConnectionHandle, messages, MaxMessageCount);

            for (int i = 0; i < numMessages; i++)
            {
                SteamNetworkingMessage_t message = SteamNetworkingMessage_t.FromIntPtr(messages[i]);
                if (GetPeerIdFromSteam64(message.m_identityPeer.GetSteamID64()) != -1)
                {
                    ProcessMessage(message);
                }
                else
                {
                    ProcessPing(message);
                }
                // Release the message after processing
                SteamNetworkingMessage_t.Release(messages[i]);
            }
        }
    }

    public override void _Close()
    {
        if (!IsActive() || connectionStatus != ConnectionStatus.Connected)
        {
            return;
        }

        foreach (var entry in connectionsBySteamId64)
        {
            entry.Value.Close();
        }

        if (_IsServer())
        {
            CloseListenSocket();
        }

        peerIdToSteamId.Clear();
        connectionsBySteamId64.Clear();
        mode = MultiplayerPeerMode.NONE;
        uniqueId = 0;
        connectionStatus = ConnectionStatus.Disconnected;
    }

    public override void _DisconnectPeer(int peer, bool force)
    {
        if (!IsActive() || !peerIdToSteamId.ContainsKey(peer))
        {
            return;
        }

        SteamConnection? connection = GetConnectionByPeer(peer);
        if (connection == null)
        {
            return;
        }

        if (!connection.Close())
        {
            return;
        }

        connection.Flush();
        connectionsBySteamId64.Remove(connection.SteamId);
        peerIdToSteamId.Remove(peer);

        if (mode == MultiplayerPeerMode.CLIENT || mode == MultiplayerPeerMode.HOST)
        {
            SteamConnection? serverConnection = GetConnectionByPeer(0);
            serverConnection?.Flush();
        }
        else if (force)
        {
            connectionsBySteamId64.Clear();
            Close();
        }
    }

    public override int _GetUniqueId()
    {
        if (!IsActive())
        {
            return 0;
        }
        return (int)uniqueId;
    }

    public override bool _IsServerRelaySupported()
    {
        return mode == MultiplayerPeerMode.HOST || mode == MultiplayerPeerMode.CLIENT;
    }

    public override ConnectionStatus _GetConnectionStatus()
    {
        return connectionStatus;
    }



    private int GetSteamTransferFlag()
    {
        TransferModeEnum transferMode = _GetTransferMode();

        int flags = 0;
        if (NoNagle) flags |= Constants.k_nSteamNetworkingSend_NoNagle;
        if (NoDelay) flags |= Constants.k_nSteamNetworkingSend_NoDelay;

        return transferMode switch
        {
            TransferModeEnum.Reliable => Constants.k_nSteamNetworkingSend_Reliable | flags,
            TransferModeEnum.Unreliable => Constants.k_nSteamNetworkingSend_Unreliable | flags,
            _ => throw new InvalidOperationException("Unknown transfer mode")
        };
    }

    private void OnNetworkConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t param)
    {
        ulong steamId = param.m_info.m_identityRemote.GetSteamID64();
        ESteamNetworkingConnectionState oldStateEnum = param.m_eOldState;
        ESteamNetworkingConnectionState newStateEnum = param.m_info.m_eState;

        if (param.m_info.m_hListenSocket != HSteamListenSocket.Invalid &&
            oldStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None &&
            newStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
        {
            EResult result = SteamNetworkingSockets.AcceptConnection(param.m_hConn);
            if (result != EResult.k_EResultOK)
            {
                SteamNetworkingSockets.CloseConnection(param.m_hConn, (int)ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_AppException_Generic, "Failed to accept connection", false);
            }
        }

        if ((oldStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting ||
            oldStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_FindingRoute) &&
            newStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
        {
            AddConnection(steamId, param.m_hConn);
            if (!_IsServer())
            {
                connectionStatus = ConnectionStatus.Connected;
                connectionsBySteamId64[steamId].SendPeer(uniqueId);
            }
        }

        if ((oldStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting ||
            oldStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected) &&
            newStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer)
        {
            if (!_IsServer())
            {
                if (connectionStatus == ConnectionStatus.Connected)
                {
                    EmitSignal("peer_disconnected", 1);
                }
                Close();
            }
            else
            {
                if (connectionsBySteamId64.ContainsKey(steamId))
                {
                    SteamConnection steamConnection = connectionsBySteamId64[steamId];
                    int peerId = steamConnection.PeerId;
                    if (peerId != -1)
                    {
                        EmitSignal("peer_disconnected", peerId);
                        peerIdToSteamId.Remove(peerId);
                    }
                    connectionsBySteamId64.Remove(steamId);
                }
            }
        }

        if ((oldStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting ||
            oldStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected) &&
            newStateEnum == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
        {
            if (!_IsServer())
            {
                if (connectionStatus == ConnectionStatus.Connected)
                {
                    EmitSignal("peer_disconnected", 1);
                }
                Close();
            }
            else
            {
                if (connectionsBySteamId64.ContainsKey(steamId))
                {
                    SteamConnection steamConnection = connectionsBySteamId64[steamId];
                    int peerId = steamConnection.PeerId;
                    if (peerId != -1)
                    {
                        EmitSignal("peer_disconnected", peerId);
                        peerIdToSteamId.Remove(peerId);
                    }
                    connectionsBySteamId64.Remove(steamId);
                }
            }
        }
    }

    public int GetPeerIdFromSteam64(ulong steam64)
    {
        if (connectionsBySteamId64.ContainsKey(steam64))
        {
            return connectionsBySteamId64[steam64].PeerId;
        }
        return -1;
    }

    public ulong GetSteamIdFromPeerId(int peerId)
    {
        if (peerIdToSteamId.ContainsKey(peerId))
        {
            return peerIdToSteamId[peerId].SteamId;
        }
        return 0;
    }

    /// <summary>
    /// Gets the Steam display name for a given peer ID.
    /// </summary>
    /// <param name="peerId"></param>
    /// <returns></returns>
    public string GetSteamDisplayNameFromPeerId(int peerId)
    {
        ulong steamId = GetSteamIdFromPeerId(peerId);
        if (steamId != 0)
        {
            CSteamID cSteamID = new CSteamID(steamId);
            return SteamFriends.GetFriendPersonaName(cSteamID);
        }
        return "";
    }

    private void SetSteamIdPeer(ulong steamId, int peerId)
    {
        if (steamId == SteamUser.GetSteamID().m_SteamID)
        {
            GD.PrintErr("Cannot add self as a new peer");
            return;
        }
        if (!connectionsBySteamId64.ContainsKey(steamId))
        {
            GD.PrintErr("Steam ID missing");
            return;
        }

        SteamConnection connection = connectionsBySteamId64[steamId];
        if (connection.PeerId == -1)
        {
            connection.PeerId = peerId;
            peerIdToSteamId[peerId] = connection;
        }
    }

    private SteamConnection? GetConnectionByPeer(int peerId)
    {
        if (peerIdToSteamId.ContainsKey(peerId))
        {
            return peerIdToSteamId[peerId];
        }
        return null;
    }

    private void ProcessMessage(SteamNetworkingMessage_t message)
    {
        byte[] data = new byte[message.m_cbSize];
        System.Runtime.InteropServices.Marshal.Copy(message.m_pData, data, 0, message.m_cbSize);
        ulong identity = message.m_identityPeer.GetSteamID64();
        incomingPackets.Enqueue(new SteamPacketPeer(data, sender: identity));
    }

    private void ProcessPing(SteamNetworkingMessage_t message)
    {
        byte[] data = new byte[message.m_cbSize];
        System.Runtime.InteropServices.Marshal.Copy(message.m_pData, data, 0, message.m_cbSize);
        int peerId = BitConverter.ToInt32(data);
        ulong steamId = message.m_identityPeer.GetSteamID64();

        SteamConnection connection = connectionsBySteamId64[steamId];

        if (peerId != 0)
        {
            if (connection.PeerId == -1)
            {
                SetSteamIdPeer(steamId, peerId);
            }
            if (_IsServer())
            {
                Error error = connection.SendPeer(uniqueId);
                if (error != Error.Ok)
                {
                    GD.PrintErr("Error sending server peer ID to client: ", error);
                }
            }
            EmitSignal(SignalName.PeerConnected, connection.PeerId);
        }
    }

    private void AddConnection(ulong steamId, HSteamNetConnection connectionHandle)
    {
        if (steamId == SteamUser.GetSteamID().m_SteamID)
        {
            GD.PrintErr("Cannot add self as a new peer");
        }
        if (!connectionsBySteamId64.ContainsKey(steamId))
        {
            SteamConnection connection = new SteamConnection
            {
                SteamId = steamId,
                ConnectionHandle = connectionHandle,
            };
            connectionsBySteamId64.Add(steamId, connection);
        }
    }

    private void CloseListenSocket()
    {
        if (listenSocket != HSteamListenSocket.Invalid)
        {
            SteamNetworkingSockets.CloseListenSocket(listenSocket);
            listenSocket = HSteamListenSocket.Invalid;
        }
    }

    private enum MultiplayerPeerMode
    {
        NONE, HOST, CLIENT
    }

    /// <summary>
    /// A packet to be sent or received via Steam networking.
    /// </summary>
    private class SteamPacketPeer
    {
        public byte[] Data { get; private set; } = new byte[MaxSteamPacketSize];
        public ulong Sender { get; set; }
        public int TransferMode { get; private set; } = Constants.k_nSteamNetworkingSend_Reliable;

        public SteamPacketPeer(byte[] buffer, ulong sender = 0, int transferMode = Constants.k_nSteamNetworkingSend_Reliable)
        {
            if (buffer.Length > MaxSteamPacketSize)
            {
                GD.PrintErr("Error: Tried to send a packet larger than MaxSteamPacketSize");
                return;
            }

            Sender = sender;
            Data = buffer;
            TransferMode = transferMode;
        }
    }

    /// <summary>
    /// A connection to a Steam peer.
    /// </summary>
    private class SteamConnection
    {

        public ulong SteamId { get; set; }
        public HSteamNetConnection ConnectionHandle { get; set; }
        public int PeerId { get; set; } = -1;
        List<SteamPacketPeer> pendingRetryPackets = new List<SteamPacketPeer>();

        ~SteamConnection()
        {
            SteamNetworkingSockets.CloseConnection(ConnectionHandle, (int)ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_App_Generic, "Disconnect Default!", false);
        }

        /// <summary>
        /// Sends a raw packet to the peer.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public EResult RawSend(SteamPacketPeer packet)
        {
            IntPtr pData = System.Runtime.InteropServices.Marshal.AllocHGlobal(packet.Data.Length);
            try
            {
                System.Runtime.InteropServices.Marshal.Copy(packet.Data, 0, pData, packet.Data.Length);
                return SteamNetworkingSockets.SendMessageToConnection(ConnectionHandle, pData, (uint)packet.Data.Length, packet.TransferMode, out long messageOut);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(pData);
            }
        }

        /// <summary>
        /// Sends all pending packets, retrying reliable packets as needed.
        /// </summary>
        /// <returns></returns>
        public Error SendPending()
        {
            while (pendingRetryPackets.Count > 0)
            {
                var packet = pendingRetryPackets.First();
                EResult errorCode = RawSend(packet);
                if (errorCode == EResult.k_EResultOK)
                {
                    pendingRetryPackets.RemoveAt(0);
                }
                else
                {
                    string errorString = ConvertErrorResultToString(errorCode);
                    if ((packet.TransferMode & Constants.k_nSteamNetworkingSend_Reliable) != 0)
                    {
                        GD.PrintErr("Send error (reliable, will retry): ", errorString);
                        break;
                    }
                    else
                    {
                        GD.PrintErr("Send error (unreliable, won't retry): ", errorString);
                        pendingRetryPackets.RemoveAt(0);
                    }
                }
            }
            return Error.Ok;
        }

        public void AddPacket(SteamPacketPeer packet)
        {
            pendingRetryPackets.Add(packet);
        }

        public Error Send(SteamPacketPeer packet)
        {
            AddPacket(packet);
            return SendPending();
        }

        public void Flush()
        {
            if (ConnectionHandle == HSteamNetConnection.Invalid)
            {
                return;
            }
            SteamNetworkingSockets.FlushMessagesOnConnection(ConnectionHandle);
        }

        public bool Close()
        {
            if (ConnectionHandle == HSteamNetConnection.Invalid)
            {
                return false;
            }
            return SteamNetworkingSockets.CloseConnection(ConnectionHandle, (int)ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_App_Generic, "Failed to accept connection", false);
        }

        public override bool Equals(object? obj)
        {
            return obj is SteamConnection other &&
                   SteamId == other.SteamId;
        }

        public override int GetHashCode()
        {
            return SteamId.GetHashCode();
        }

        public Error SendPeer(uint peerId)
        {
            SetupPeerPayload payload = new SetupPeerPayload(peerId);
            return SendSetupPeer(payload);
        }

        private Error SendSetupPeer(SetupPeerPayload payload)
        {
            var packet = new SteamPacketPeer(BitConverter.GetBytes(payload.PeerId), transferMode: Constants.k_nSteamNetworkingSend_Reliable);
            return Send(packet);
        }

        string ConvertErrorResultToString(EResult errorResult)
        {
            return errorResult.ToString();
        }

        struct SetupPeerPayload
        {
            public uint PeerId { get; private set; }

            public SetupPeerPayload(uint peerId)
            {
                PeerId = peerId;
            }
        }
    }
}
#else
// Stub implementation for non-desktop platforms (Android, iOS, Web, etc.)
using Godot;

namespace Godot.Steamworks.Net.Multiplayer.Peer;

/// <summary>
/// Stub multiplayer peer for non-desktop platforms.
/// Steam networking is not supported on mobile platforms.
/// </summary>
public partial class SteamMultiplayerPeer : MultiplayerPeerExtension
{
    public bool NoNagle { get; set; } = true;
    public bool NoDelay { get; set; } = false;

    public SteamMultiplayerPeer()
    {
        GodotSteamworksLogger.LogWarning("Steam multiplayer is not supported on this platform.");
    }

    public Error CreateHost(int localVirtualPort) => Error.Unavailable;
    public Error CreateClient(ulong identityRemote, int remoteVirtualPort) => Error.Unavailable;

    public override byte[] _GetPacketScript() => System.Array.Empty<byte>();
    public override Error _PutPacketScript(byte[] buffer) => Error.Unavailable;
    public override int _GetAvailablePacketCount() => 0;
    public override int _GetMaxPacketSize() => 0;
    public override TransferModeEnum _GetPacketMode() => TransferModeEnum.Reliable;
    public override void _SetTransferMode(TransferModeEnum mode) { }
    public override TransferModeEnum _GetTransferMode() => TransferModeEnum.Reliable;
    public override void _SetTransferChannel(int pChannel) { }
    public override int _GetTransferChannel() => 0;
    public override void _SetTargetPeer(int peer) { }
    public override int _GetPacketPeer() => 0;
    public override int _GetPacketChannel() => 0;
    public override bool _IsServer() => false;
    public bool IsActive() => false;
    public override void _Poll() { }
    public override void _Close() { }
    public override void _DisconnectPeer(int peer, bool force) { }
    public override int _GetUniqueId() => 0;
    public override bool _IsServerRelaySupported() => false;
    public override ConnectionStatus _GetConnectionStatus() => ConnectionStatus.Disconnected;

    public int GetPeerIdFromSteam64(ulong steam64) => -1;
    public ulong GetSteamIdFromPeerId(int peerId) => 0;
    public string GetSteamDisplayNameFromPeerId(int peerId) => "";
}
#endif
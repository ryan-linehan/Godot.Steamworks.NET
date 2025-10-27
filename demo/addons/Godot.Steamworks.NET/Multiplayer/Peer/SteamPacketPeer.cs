using System;
using Godot;
using Steamworks;

namespace Godot.Steamworks.Net.Multiplayer.Peer;

/// <summary>
/// A packet peer implementation for Steam networking that stores packet data and metadata.
/// Based off of: https://github.com/expressobits/steam-multiplayer-peer
/// </summary>
public partial class SteamPacketPeer : RefCounted
{
    /// <summary>
    /// Maximum size for Steam networking packets (512KB)
    /// </summary>
    public const int MaxSteamPacketSize = 524288;

    /// <summary>
    /// The packet data buffer
    /// </summary>
    public byte[] Data { get; private set; } = new byte[MaxSteamPacketSize];

    /// <summary>
    /// The actual size of the data in the packet
    /// </summary>
    public uint Size { get; set; }

    /// <summary>
    /// The Steam ID of the sender (for received packets)
    /// </summary>
    public ulong Sender { get; set; }

    /// <summary>
    /// The transfer mode/send flags for this packet
    /// </summary>
    public int TransferMode { get; set; } = 8; // Default to reliable

    /// <summary>
    /// Default constructor - creates an empty packet
    /// </summary>
    public SteamPacketPeer()
    {
    }

    /// <summary>
    /// Constructor that creates a packet from buffer data
    /// </summary>
    /// <param name="buffer">The data buffer to copy from</param>
    /// <param name="bufferSize">The size of the data to copy</param>
    /// <param name="transferMode">The transfer mode for this packet</param>
    public SteamPacketPeer(byte[] buffer, uint bufferSize, int transferMode = 8)
    {
        if (bufferSize > MaxSteamPacketSize)
        {
            GodotSteamworksLogger.LogError($"Error: Tried to send a packet larger than MAX_STEAM_PACKET_SIZE: {bufferSize}");
            return;
        }

        Data = new byte[MaxSteamPacketSize];
        Size = bufferSize;
        TransferMode = transferMode;

        if (buffer != null && bufferSize > 0)
        {
            Array.Copy(buffer, 0, Data, 0, (int)bufferSize);
        }
    }
}

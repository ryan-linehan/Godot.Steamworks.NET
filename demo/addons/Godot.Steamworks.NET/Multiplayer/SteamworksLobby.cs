using Godot;
using Steamworks;
using System;

namespace Godot.Steamworks.NET.Multiplayer;

public partial class SteamworksLobby : Godot.RefCounted
{

    /// <summary>
    /// Signal emitted when a lobby is created.
    /// </summary>
    /// <param name="lobbyId">The ID of the created lobby</param>
    [Signal]
    public delegate void LobbyCreatedEventHandler(ulong lobbyId);
    /// <summary>
    /// Signal emitted when a lobby is joined.
    /// </summary>
    /// <param name="lobbyId">The ID of the joined lobby</param>
    [Signal]
    public delegate void LobbyJoinedEventHandler(ulong lobbyId);
    /// <summary>
    /// Signal emitted when joining a lobby fails.
    /// </summary>
    /// <param name="lobbyId">The ID of the lobby that failed to join (0 if unknown)</param>
    /// <param name="errorMessage">The error message describing the failure</param>
    [Signal]
    public delegate void LobbyJoinFailedEventHandler(ulong lobbyId, string errorMessage);
    /// <summary>
    /// Signal emitted when a player joins the lobby.
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="playerName"></param>
    [Signal]
    public delegate void PlayerJoinedLobbyEventHandler(ulong steamId, string playerName);
    /// <summary>
    /// Signal emitted when a player leaves the lobby.
    /// </summary>
    /// <param name="steamId"></param>
    [Signal]
    public delegate void PlayerLeftLobbyEventHandler(ulong steamId, string playerName);
    #region Steamworks Callbacks and CallResults
    // These references MUST be kept to prevent garbage collection of Steam callbacks
#pragma warning disable IDE0052 // Remove unread private members
    private CallResult<LobbyCreated_t> lobbyCreatedCallResult = null!;
    private CallResult<LobbyMatchList_t> lobbyListCallResult = null!;
    private CallResult<LobbyEnter_t> lobbyEnterCallResult = null!;

    // Callbacks
    private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback = null!;
    private Callback<LobbyChatUpdate_t> lobbyChatUpdateCallback = null!;
    private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequestedCallback = null!;
#pragma warning restore IDE0052 // Remove unread private members
    #endregion

    public SteamworksLobby()
    {
        lobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyList);
        lobbyEnterCallResult = CallResult<LobbyEnter_t>.Create(OnLobbyEntered);

        // Set up Steam callbacks
        lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        gameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
    }

    private void PrintLobbyInfo(CSteamID lobbyId)
    {
        GodotSteamworksLogger.LogInfo("=== Lobby Data ===");

        // Print known lobby data keys
        string[] knownKeys = { "game_name", "version", "host_name", "player_count", "max_players", "game_state" };

        foreach (string key in knownKeys)
        {
            string value = SteamMatchmaking.GetLobbyData(lobbyId, key);
            if (!string.IsNullOrEmpty(value))
            {
                GodotSteamworksLogger.LogInfo($"  {key}: {value}");
            }
        }

        // List all lobby members
        GodotSteamworksLogger.LogInfo("  lobby_members:");
        int memberCount = GetLobbyMemberCount(lobbyId.m_SteamID);
        for (int i = 0; i < memberCount; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
            string memberName = SteamFriends.GetFriendPersonaName(memberId);
            GodotSteamworksLogger.LogInfo($"    {i + 1}. {memberName} ({memberId})");
        }

        GodotSteamworksLogger.LogInfo("==================");
    }

    /// <summary>
    /// Gets the number of members in the specified lobby
    /// </summary>
    /// <param name="lobbyId"></param>
    /// <returns></returns>
    public int GetLobbyMemberCount(ulong lobbyId)
    {
        CSteamID lobbyCSteamId = new CSteamID(lobbyId);
        return SteamMatchmaking.GetNumLobbyMembers(lobbyCSteamId);
    }

    /// <summary>
    /// Gets the owner's steam id of the specified lobby
    /// </summary>
    public ulong GetLobbyOwner(ulong lobbyId)
    {
        CSteamID lobbyCSteamId = new CSteamID(lobbyId);
        CSteamID ownerCSteamId = SteamMatchmaking.GetLobbyOwner(lobbyCSteamId);
        // TODO: What happens if it is an invalid lobby id? Handle this.
        return ownerCSteamId.m_SteamID;
    }

    /// <summary>
    /// Creates a new lobby
    /// </summary>
    /// <param name="maxMembers">Maximum number of members allowed in the lobby</param>
    /// <param name="lobbyType">Type of lobby (private, friends only, public, etc.)</param>
    public void CreateLobby(int maxMembers, ELobbyType lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly)
    {
        var godotSteam = GodotSteamworks.Instance;
        if (godotSteam == null || !godotSteam.IsInitalized)
        {
            GodotSteamworksLogger.LogError("Steam is not initialized!");
            EmitSignal(SignalName.LobbyJoinFailed, 0UL, "Steam not initialized");
            return;
        }

        GodotSteamworksLogger.LogInfo($"Creating lobby with max members: {maxMembers}, type: {lobbyType}");
        var apiCall = SteamMatchmaking.CreateLobby(lobbyType, maxMembers);
        lobbyCreatedCallResult.Set(apiCall);
    }

    /// <summary>
    /// Joins the specified lobby
    /// </summary>
    /// <param name="lobbyId">The lobby ID to join</param>
    public void JoinLobby(CSteamID lobbyId)
    {
        var godotSteam = GodotSteamworks.Instance;
        if (godotSteam == null || !godotSteam.IsInitalized)
        {
            GodotSteamworksLogger.LogError("Steam is not initialized!");
            EmitSignal(SignalName.LobbyJoinFailed, lobbyId.m_SteamID, "Steam not initialized");
            return;
        }

        GodotSteamworksLogger.LogInfo($"Attempting to join lobby: {lobbyId}");
        var apiCall = SteamMatchmaking.JoinLobby(lobbyId);
        lobbyEnterCallResult.Set(apiCall);
    }

    /// <summary>
    /// Sets the lobby to be joinable by friends
    /// </summary>
    /// <param name="lobbyId">The lobby to make joinable</param>
    /// <param name="joinable">Whether the lobby should be joinable</param>
    public void SetLobbyJoinable(CSteamID lobbyId, bool joinable)
    {
        SteamMatchmaking.SetLobbyJoinable(lobbyId, joinable);
    }

    /// <summary>
    /// Gets data from the specified lobby
    /// </summary>
    /// <param name="lobbyId">The lobby to get data from</param>
    /// <param name="key">Data key</param>
    /// <returns>Data value or empty string if not found</returns>
    public string GetLobbyData(CSteamID lobbyId, string key)
    {
        return SteamMatchmaking.GetLobbyData(lobbyId, key);
    }

    /// <summary>
    /// Sets data for the specified lobby (only lobby owner can do this)
    /// </summary>
    /// <param name="lobbyId">The lobby to set data for</param>
    /// <param name="key">Data key</param>
    /// <param name="value">Data value</param>
    public void SetLobbyData(CSteamID lobbyId, string key, string value)
    {
        SteamMatchmaking.SetLobbyData(lobbyId, key, value);
    }

    /// <summary>
    /// Handler for when a lobby is entered
    /// </summary>
    /// <param name="result"></param>
    /// <param name="bIOFailure"></param>
    private void OnLobbyEntered(LobbyEnter_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            GodotSteamworksLogger.LogError("Failed to join lobby: IO Failure");
            EmitSignal(SignalName.LobbyJoinFailed, result.m_ulSteamIDLobby, "IO Failure");
            return;
        }

        if (result.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            GodotSteamworksLogger.LogError($"Failed to join lobby: {(EChatRoomEnterResponse)result.m_EChatRoomEnterResponse}");
            EmitSignal(SignalName.LobbyJoinFailed, result.m_ulSteamIDLobby, $"Failed to join: {(EChatRoomEnterResponse)result.m_EChatRoomEnterResponse}");
            return;
        }

        var currentLobbyId = new CSteamID(result.m_ulSteamIDLobby);
        GodotSteamworksLogger.LogInfo($"Successfully joined lobby: {currentLobbyId}");

        EmitSignal(SignalName.LobbyJoined, result.m_ulSteamIDLobby);
    }


    private void OnLobbyDataUpdate(LobbyDataUpdate_t result)
    {
        GodotSteamworksLogger.LogInfo($"Lobby data updated for lobby: {result.m_ulSteamIDLobby}");

        // Get the lobby ID as CSteamID for data retrieval
        CSteamID lobbyId = new CSteamID(result.m_ulSteamIDLobby);
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t result)
    {
        CSteamID lobbyId = new CSteamID(result.m_ulSteamIDLobby);
        CSteamID userId = new CSteamID(result.m_ulSteamIDUserChanged);

        string playerName = SteamFriends.GetFriendPersonaName(userId);

        if ((result.m_rgfChatMemberStateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered) != 0)
        {
            GodotSteamworksLogger.LogInfo($"Player joined lobby: {playerName}");
            EmitSignal(SignalName.PlayerJoinedLobby, result.m_ulSteamIDUserChanged, playerName);
        }
        else if ((result.m_rgfChatMemberStateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft) != 0)
        {
            GodotSteamworksLogger.LogInfo($"Player left lobby: {playerName}");
            EmitSignal(SignalName.PlayerLeftLobby, result.m_ulSteamIDUserChanged, playerName);
        }
    }


    private void OnLobbyCreated(LobbyCreated_t result, bool bIOFailure)
    {
        if (bIOFailure || result.m_eResult != EResult.k_EResultOK)
        {
            GodotSteamworksLogger.LogError($"Failed to create lobby: {result.m_eResult}");
            EmitSignal(SignalName.LobbyJoinFailed, result.m_ulSteamIDLobby, $"Failed to create lobby: {result.m_eResult}");
            return;
        }

        var lobbyId = new CSteamID(result.m_ulSteamIDLobby);
        GodotSteamworksLogger.LogInfo($"Lobby created successfully! ID: {lobbyId}");
        EmitSignal(SignalName.LobbyCreated, result.m_ulSteamIDLobby);
    }




    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t result)
    {
        GodotSteamworksLogger.LogInfo($"Received lobby join request: {result.m_steamIDLobby}");
        // Automatically join the requested lobby
        JoinLobby(result.m_steamIDLobby);
    }

    private void OnLobbyList(LobbyMatchList_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            GodotSteamworksLogger.LogError("Failed to get lobby list: IO Failure");
            return;
        }

        GodotSteamworksLogger.LogInfo($"Found {result.m_nLobbiesMatching} lobbies");
        // Process lobby list here if needed
        for (uint i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex((int)i);
            string gameName = SteamMatchmaking.GetLobbyData(lobbyId, "game_name");
            GodotSteamworksLogger.LogInfo($"Lobby {i}: {lobbyId} - {gameName}");
        }
    }
}

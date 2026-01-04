#if GODOT_PC
using Godot;
using Steamworks;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Godot.Steamworks.Net.Models;
using Godot.Steamworks.Net.Util;
namespace Godot.Steamworks.Net.Multiplayer;


/// <summary>
/// Class for managing Steamworks lobbies
/// </summary>
public partial class SteamworksLobby : Godot.RefCounted
{
    /// <summary>
    /// Signal emitted when a lobby is created by the local client
    /// </summary>
    /// <param name="lobbyId">The ID of the created lobby</param>
    [Signal]
    public delegate void LobbyCreatedEventHandler(ulong lobbyId);
    /// <summary>
    /// Signal emitted when local client joins a lobby
    /// </summary>
    /// <param name="lobbyId">The ID of the joined lobby</param>
    [Signal]
    public delegate void LobbyJoinedEventHandler(ulong lobbyId);
    /// <summary>
    /// Signal emitted when local client fails to join a lobby.
    /// </summary>
    /// <param name="lobbyId">The ID of the lobby that failed to join (0 if unknown)</param>
    /// <param name="errorMessage">The error message describing the failure</param>
    [Signal]
    public delegate void LobbyJoinFailedEventHandler(ulong lobbyId, string errorMessage);
    /// <summary>
    /// Signal emitted when the local client leaves the lobby.
    /// </summary>
    /// <param name="lobbyId">The ID of the lobby that was left</param>
    [Signal]
    public delegate void LobbyLeftEventHandler(ulong lobbyId);
    /// <summary>
    /// Signal emitted when a player leaves the lobby.
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="playerName"></param>
    [Signal]
    public delegate void PlayerLeftLobbyEventHandler(ulong lobbyId, ulong steamId, string playerName);
    /// <summary>
    /// Signal emitted when a player joins the lobby
    /// </summary>
    /// <param name="steamId"></param>
    /// <param name="playerName"></param>
    [Signal]
    public delegate void PlayerJoinedLobbyEventHandler(ulong lobbyId, ulong steamId, string playerName);
    /// <summary>
    /// Signal emitted when lobby search completes.
    /// </summary>
    /// <param name="lobbyIds">Array of lobby IDs found</param>
    [Signal]
    public delegate void LobbySearchCompletedEventHandler(Godot.Collections.Array<ulong> lobbyIds);
    /// <summary>
    /// Signal emitted when lobby data is updated.
    /// </summary>
    [Signal]
    public delegate void LobbyDataUpdatedEventHandler();
    /// <summary>
    /// Signal emitted when lobby data is updated and <see cref="DetailedLobbyData"/> is true.
    /// Both <see cref="LobbyDataUpdated"/> and <see cref="LobbyDataUpdatedDetailed"/> are emitted.
    /// </summary>
    [Signal]
    public delegate void LobbyDataUpdatedDetailedEventHandler(Godot.Collections.Dictionary<string, string> lobbyData);
    // These references MUST be kept to prevent garbage collection of Steam callbacks
#pragma warning disable IDE0052 // Remove unread private members
    // CallResults
    private CallResult<LobbyCreated_t> lobbyCreatedCallResult = null!;
    private CallResult<LobbyMatchList_t> lobbyListCallResult = null!;
    private CallResult<LobbyEnter_t> lobbyEnterCallResult = null!;


    // Callbacks
    private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback = null!;
    private Callback<LobbyChatUpdate_t> lobbyChatUpdateCallback = null!;
    private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequestedCallback = null!;
#pragma warning restore IDE0052 // Remove unread private members

    /// <summary>
    /// Indicates if the detailed lobby data should be fetched and provided in the LobbyDataUpdatedDetailed signal.
    /// On by default.
    ///     - Turn off if you want to optimize performance and do not need detailed lobby data
    /// </summary>
    public bool DetailedLobbyData { get; set; } = true;
    public SteamworksLobby()
    {
        lobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyList);
        lobbyEnterCallResult = CallResult<LobbyEnter_t>.Create(OnLobbyEntered);

        lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        gameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
    }

    // TODO: Consider adding a type parameter for getting lobby data out of the key value pairs that would come back
    /// <summary>
    /// Searches for lobbies matching the specified criteria.
    /// Note: Lobbies created with <see cref="ELobbyType.k_ELobbyTypeFriendsOnly"/> will not be visible from this call.
    /// Use <see cref="GetFriendLobbies"/> to find friend lobbies for the current steam app id.
    /// </summary>
    /// <param name="maxResults">Maximum number of lobbies to return</param>
    /// <returns>List of lobby IDs found</returns>
    public async Task<ulong[]> SearchLobbiesAsync(int maxResults = 50)
    {
        var godotSteam = GodotSteamworks.Instance;
        if (godotSteam == null || !godotSteam.IsInitialized)
        {
            GodotSteamworksLogger.LogError("Steam is not initialized!");
            return Array.Empty<ulong>();
        }

        GodotSteamworksLogger.LogInfo($"Searching for lobbies (max results: {maxResults})");

        // Set up search filters if needed
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(maxResults);
        var apiCall = SteamMatchmaking.RequestLobbyList();
        var (result, ioFailure) = await SteamAsyncHelper.CallAsync<LobbyMatchList_t>(apiCall);

        if (ioFailure)
        {
            GodotSteamworksLogger.LogError("Failed to get lobby list: IO Failure");
            return Array.Empty<ulong>();
        }

        GodotSteamworksLogger.LogInfo($"Found {result.m_nLobbiesMatching} lobbies");
        var lobbies = new ulong[result.m_nLobbiesMatching];

        for (uint i = 0; i < result.m_nLobbiesMatching; i++)
        {
            ulong lobbyId = SteamMatchmaking.GetLobbyByIndex((int)i).m_SteamID;
            lobbies[i] = lobbyId;
        }

        return lobbies;
    }

    /// <summary>
    /// Gets the lobby ids for all steam friends created for the current game's steam app id
    /// </summary>
    /// <returns></returns>
    public ulong[] GetFriendLobbies()
    {
        var godotSteam = GodotSteamworks.Instance;
        if (godotSteam == null || !godotSteam.IsInitialized)
        {
            GodotSteamworksLogger.LogError("Steam is not initialized!");
            return Array.Empty<ulong>();
        }

        GodotSteamworksLogger.LogInfo("Searching for friend lobbies");
        var lobbies = new List<ulong>();
        var friends = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
        GodotSteamworksLogger.LogInfo($"Found {friends} friends to check for lobbies");
        for (int i = 0; i < friends; i++)
        {
            FriendGameInfo_t friendGameInfo;
            CSteamID steamIDFriend = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
            if (SteamFriends.GetFriendGamePlayed(steamIDFriend, out friendGameInfo) && friendGameInfo.m_gameID.AppID() == SteamUtils.GetAppID() && friendGameInfo.m_steamIDLobby.IsValid())
            {
                lobbies.Add(friendGameInfo.m_steamIDLobby.m_SteamID);
            }
        }

        GodotSteamworksLogger.LogInfo($"Found {lobbies.Count} friend lobbies");

        return lobbies.ToArray();
    }


    /// <summary>
    /// Searches for lobbies (traditional callback pattern - processes results in OnLobbyList)
    /// </summary>
    /// <param name="maxResults">Maximum number of lobbies to return</param>
    public void SearchLobbies(int maxResults = 50)
    {
        var godotSteam = GodotSteamworks.Instance;
        if (godotSteam == null || !godotSteam.IsInitialized)
        {
            GodotSteamworksLogger.LogError("Steam is not initialized!");
            return;
        }

        GodotSteamworksLogger.LogInfo($"Searching for lobbies (max results: {maxResults})");

        // Set up search filters
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(maxResults);

        var apiCall = SteamMatchmaking.RequestLobbyList();
        lobbyListCallResult.Set(apiCall);
    }

    /// <summary>
    /// Gets the members of the specified lobby
    /// </summary>
    /// <param name="lobbyId">The ID of the lobby</param>
    public Godot.Collections.Array<LobbyMember> GetLobbyMembers(ulong lobbyId)
    {
        Godot.Collections.Array<LobbyMember> members = new Godot.Collections.Array<LobbyMember>();
        CSteamID lobbyCSteamId = new CSteamID(lobbyId);
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyCSteamId);

        for (int i = 0; i < memberCount; i++)
        {
            CSteamID memberCSteamId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyCSteamId, i);
            string memberName = SteamFriends.GetFriendPersonaName(memberCSteamId);
            LobbyMember member = new LobbyMember
            {
                SteamId = memberCSteamId.m_SteamID,
                SteamDisplayName = memberName
            };
            members.Add(member);
        }

        return members;
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
    /// Checks if the current user is the owner of the specified lobby
    /// </summary>
    public bool IsLobbyOwner(ulong lobbyId)
    {
        CSteamID lobbyCSteamId = new CSteamID(lobbyId);
        CSteamID ownerCSteamId = SteamMatchmaking.GetLobbyOwner(lobbyCSteamId);
        return ownerCSteamId == SteamUser.GetSteamID();
    }

    /// <summary>
    /// Creates a new lobby
    /// </summary>
    /// <param name="maxMembers">Maximum number of members allowed in the lobby</param>
    /// <param name="lobbyType">Type of lobby (private, friends only, public, etc.)</param>
    public async Task<ulong?> CreateLobbyAsync(int maxMembers, ELobbyType lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly)
    {
        if (maxMembers > 250)
        {
            GodotSteamworksLogger.LogWarning("Max members exceeds Steam limit of 250. Setting to 250.");
            maxMembers = 250;
        }

        var godotSteam = GodotSteamworks.Instance;
        if (godotSteam == null || !godotSteam.IsInitialized)
        {
            GodotSteamworksLogger.LogError("Steam is not initialized!");
            EmitSignal(SignalName.LobbyJoinFailed, 0UL, "Steam not initialized");
            return null;
        }

        GodotSteamworksLogger.LogInfo($"Creating lobby with max members: {maxMembers}, type: {lobbyType}");
        var apiCall = SteamMatchmaking.CreateLobby(lobbyType, maxMembers);

        var (result, ioFailure) = await SteamAsyncHelper.CallAsync<LobbyCreated_t>(apiCall);

        if (ioFailure || result.m_eResult != EResult.k_EResultOK)
        {
            GodotSteamworksLogger.LogError($"Failed to create lobby: {result.m_eResult}");
            EmitSignal(SignalName.LobbyJoinFailed, 0UL, $"Failed to create lobby: {result.m_eResult}");
            return null;
        }

        var lobbyId = new CSteamID(result.m_ulSteamIDLobby);
        GodotSteamworksLogger.LogInfo($"Lobby created successfully! ID: {lobbyId}");
        EmitSignal(SignalName.LobbyCreated, result.m_ulSteamIDLobby);
        return lobbyId.m_SteamID;
    }

    /// <summary>
    /// Creates a new lobby emits <see cref="LobbyCreated"/> signal when complete
    /// </summary>
    /// <param name="maxMembers">Maximum number of members allowed in the lobby</param>
    /// <param name="lobbyType">Type of lobby (private, friends only, public, etc.)</param>
    public void CreateLobby(int maxMembers, ELobbyType lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly)
    {
        if (maxMembers > 250)
        {
            GodotSteamworksLogger.LogWarning("Max members exceeds Steam limit of 250. Setting to 250.");
            maxMembers = 250;
        }

        var godotSteam = GodotSteamworks.Instance;
        if (godotSteam == null || !godotSteam.IsInitialized)
        {
            GodotSteamworksLogger.LogError("Steam is not initialized!");
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
    public async Task<bool> JoinLobbyAsync(ulong lobbyId)
    {
        var godotSteam = GodotSteamworks.Instance;
        if (godotSteam == null || !godotSteam.IsInitialized)
        {
            GodotSteamworksLogger.LogError("Steam is not initialized!");
            EmitSignal(SignalName.LobbyJoinFailed, lobbyId, "Steam not initialized");
            return false;
        }

        GodotSteamworksLogger.LogInfo($"Attempting to join lobby: {lobbyId}");
        var apiCall = SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));

        var (result, ioFailure) = await SteamAsyncHelper.CallAsync<LobbyEnter_t>(apiCall);

        if (ioFailure)
        {
            GodotSteamworksLogger.LogError("Failed to join lobby: IO Failure");
            EmitSignal(SignalName.LobbyJoinFailed, result.m_ulSteamIDLobby, "IO Failure");
            return false;
        }

        if (result.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            GodotSteamworksLogger.LogError($"Failed to join lobby: {(EChatRoomEnterResponse)result.m_EChatRoomEnterResponse}");
            EmitSignal(SignalName.LobbyJoinFailed, result.m_ulSteamIDLobby, $"Failed to join: {(EChatRoomEnterResponse)result.m_EChatRoomEnterResponse}");
            return false;
        }

        var joinedLobbyId = new CSteamID(result.m_ulSteamIDLobby);
        GodotSteamworksLogger.LogInfo($"Successfully joined lobby: {joinedLobbyId}");
        EmitSignal(SignalName.LobbyJoined, result.m_ulSteamIDLobby);
        return true;
    }

    /// <summary>
    /// Joins the specified lobby emits <see cref="LobbyJoined"/> signal when complete
    /// </summary>
    /// <param name="lobbyId">The lobby ID to join</param>
    public void JoinLobby(CSteamID lobbyId)
    {
        var godotSteam = GodotSteamworks.Instance;
        if (godotSteam == null || !godotSteam.IsInitialized)
        {
            GodotSteamworksLogger.LogError("Steam is not initialized!");
            return;
        }

        GodotSteamworksLogger.LogInfo($"Attempting to join lobby: {lobbyId}");
        var apiCall = SteamMatchmaking.JoinLobby(lobbyId);
        lobbyEnterCallResult.Set(apiCall);
    }

    /// <summary>
    /// Leaves the specified lobby
    /// </summary>
    /// <param name="lobbyId">The lobby ID to leave</param>
    public void LeaveLobby(ulong lobbyId)
    {
        var godotSteam = GodotSteamworks.Instance;
        if (godotSteam == null || !godotSteam.IsInitialized)
        {
            GodotSteamworksLogger.LogError("Steam is not initialized!");
            return;
        }

        GodotSteamworksLogger.LogInfo($"Leaving lobby: {lobbyId}");
        // Leave the lobby - this is immediate on the client side
        SteamMatchmaking.LeaveLobby(new CSteamID(lobbyId));

        // Emit signal to notify that we've left the lobby
        EmitSignal(SignalName.LobbyLeft, lobbyId);
        GodotSteamworksLogger.LogInfo($"Left lobby: {lobbyId}");
    }

    /// <summary>
    /// Sets the lobby to be joinable by friends (Enabled by default according to Steam docs)
    /// </summary>
    /// <param name="lobbyId">The lobby to make joinable</param>
    /// <param name="joinable">Whether the lobby should be joinable</param>
    public void SetLobbyJoinable(ulong lobbyId, bool joinable)
    {
        SteamMatchmaking.SetLobbyJoinable(new CSteamID(lobbyId), joinable);
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
    /// Gets all data from the specified lobby by getting index
    /// </summary>
    public Godot.Collections.Dictionary<string, string> GetAllLobbyData(CSteamID lobbyId)
    {
        Godot.Collections.Dictionary<string, string> lobbyData = new Godot.Collections.Dictionary<string, string>();
        int dataCount = SteamMatchmaking.GetLobbyDataCount(lobbyId);

        for (int i = 0; i < dataCount; i++)
        {
            bool success = SteamMatchmaking.GetLobbyDataByIndex(lobbyId, i, out string key, 256, out string value, 256);
            if (success)
            {
                lobbyData[key] = value;
            }
        }

        return lobbyData;
    }

    /// <summary>
    /// Sets data for the specified lobby (only lobby owner can do this)
    /// </summary>
    /// <param name="lobbyId">The lobby to set data for</param>
    /// <param name="key">Data key</param>
    /// <param name="value">Data value</param>
    public void SetLobbyData(ulong lobbyId, string key, string value)
    {
        SteamMatchmaking.SetLobbyData(new CSteamID(lobbyId), key, value);
    }


    private void OnLobbyDataUpdate(LobbyDataUpdate_t result)
    {
        GodotSteamworksLogger.LogInfo($"Lobby data updated for lobby: {result.m_ulSteamIDLobby}");
        EmitSignal(SignalName.LobbyDataUpdated);
        if (DetailedLobbyData)
        {
            var lobbyData = GetAllLobbyData(new CSteamID(result.m_ulSteamIDLobby));
            EmitSignal(SignalName.LobbyDataUpdatedDetailed, lobbyData);
        }
    }

    /// <summary>
    /// Handler for when a lobby chat update occurs (player joins/leaves)
    /// There are other hooks but currently only join/leave are handled.
    /// </summary>
    /// <param name="result"></param>
    private void OnLobbyChatUpdate(LobbyChatUpdate_t result)
    {
        CSteamID lobbyId = new CSteamID(result.m_ulSteamIDLobby);
        CSteamID userId = new CSteamID(result.m_ulSteamIDUserChanged);

        string playerName = SteamFriends.GetFriendPersonaName(userId);

        if ((result.m_rgfChatMemberStateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered) != 0)
        {
            GodotSteamworksLogger.LogInfo($"Player joined lobby: {playerName}");
            EmitSignal(SignalName.PlayerJoinedLobby, result.m_ulSteamIDLobby, result.m_ulSteamIDUserChanged, playerName);
        }
        else if ((result.m_rgfChatMemberStateChange & (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft) != 0)
        {
            GodotSteamworksLogger.LogInfo($"Player left lobby: {playerName}");
            EmitSignal(SignalName.PlayerLeftLobby, result.m_ulSteamIDLobby, result.m_ulSteamIDUserChanged, playerName);
        }
    }


    private async void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t result)
    {
        GodotSteamworksLogger.LogInfo($"Received lobby join request: {result.m_steamIDLobby}");
        // Automatically join the requested lobby using async method (you could also use JoinLobby for callback pattern)
        await JoinLobbyAsync(result.m_steamIDLobby.m_SteamID);
    }

    private void OnLobbyCreated(LobbyCreated_t result, bool bIOFailure)
    {
        if (bIOFailure || result.m_eResult != EResult.k_EResultOK)
        {
            GodotSteamworksLogger.LogError($"Failed to create lobby: {result.m_eResult}");
            EmitSignal(SignalName.LobbyJoinFailed, 0UL, $"Failed to create lobby: {result.m_eResult}");
            return;
        }

        var lobbyId = new CSteamID(result.m_ulSteamIDLobby);
        GodotSteamworksLogger.LogInfo($"Lobby created successfully! ID: {lobbyId}");
        EmitSignal(SignalName.LobbyCreated, result.m_ulSteamIDLobby);
    }

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

    private void OnLobbyList(LobbyMatchList_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            GodotSteamworksLogger.LogError("Failed to get lobby list: IO Failure");
            EmitSignal(SignalName.LobbySearchCompleted, new Godot.Collections.Array<ulong>());
            return;
        }

        GodotSteamworksLogger.LogInfo($"Found {result.m_nLobbiesMatching} lobbies");
        var lobbyIds = new Godot.Collections.Array<ulong>();

        // Process lobby list and collect IDs
        for (uint i = 0; i < result.m_nLobbiesMatching; i++)
        {
            ulong lobbyId = SteamMatchmaking.GetLobbyByIndex((int)i).m_SteamID;
            lobbyIds.Add(lobbyId);
        }

        EmitSignal(SignalName.LobbySearchCompleted, lobbyIds);
    }
}
#else
// Stub implementation for non-desktop platforms (Android, iOS, Web, etc.)
using Godot;
using System;
using System.Threading.Tasks;
using Godot.Steamworks.Net.Models;

namespace Godot.Steamworks.Net.Multiplayer;

/// <summary>
/// Stub lobby class for non-desktop platforms.
/// Steam lobbies are not supported on mobile platforms.
/// </summary>
public partial class SteamworksLobby : RefCounted
{
    [Signal] public delegate void LobbyCreatedEventHandler(ulong lobbyId);
    [Signal] public delegate void LobbyJoinedEventHandler(ulong lobbyId);
    [Signal] public delegate void LobbyJoinFailedEventHandler(ulong lobbyId, string errorMessage);
    [Signal] public delegate void LobbyLeftEventHandler(ulong lobbyId);
    [Signal] public delegate void PlayerLeftLobbyEventHandler(ulong lobbyId, ulong steamId, string playerName);
    [Signal] public delegate void PlayerJoinedLobbyEventHandler(ulong lobbyId, ulong steamId, string playerName);
    [Signal] public delegate void LobbySearchCompletedEventHandler(Godot.Collections.Array<ulong> lobbyIds);
    [Signal] public delegate void LobbyDataUpdatedEventHandler();
    [Signal] public delegate void LobbyDataUpdatedDetailedEventHandler(Godot.Collections.Dictionary<string, string> lobbyData);

    public bool DetailedLobbyData { get; set; } = true;

    public SteamworksLobby()
    {
        GodotSteamworksLogger.LogWarning("Steam lobbies are not supported on this platform.");
    }

    public Task<ulong[]> SearchLobbiesAsync(int maxResults = 50)
        => Task.FromResult(Array.Empty<ulong>());

    public ulong[] GetFriendLobbies() => Array.Empty<ulong>();

    public void SearchLobbies(int maxResults = 50)
        => EmitSignal(SignalName.LobbySearchCompleted, new Godot.Collections.Array<ulong>());

    public Godot.Collections.Array<LobbyMember> GetLobbyMembers(ulong lobbyId)
        => new Godot.Collections.Array<LobbyMember>();

    public int GetLobbyMemberCount(ulong lobbyId) => 0;
    public ulong GetLobbyOwner(ulong lobbyId) => 0;
    public bool IsLobbyOwner(ulong lobbyId) => false;

    public Task<ulong?> CreateLobbyAsync(int maxMembers, int lobbyType = 1)
    {
        EmitSignal(SignalName.LobbyJoinFailed, 0UL, "Steam not supported on this platform");
        return Task.FromResult<ulong?>(null);
    }

    public void CreateLobby(int maxMembers, int lobbyType = 1)
        => EmitSignal(SignalName.LobbyJoinFailed, 0UL, "Steam not supported on this platform");

    public Task<bool> JoinLobbyAsync(ulong lobbyId)
    {
        EmitSignal(SignalName.LobbyJoinFailed, lobbyId, "Steam not supported on this platform");
        return Task.FromResult(false);
    }

    public void JoinLobby(ulong lobbyId)
        => EmitSignal(SignalName.LobbyJoinFailed, lobbyId, "Steam not supported on this platform");

    public void LeaveLobby(ulong lobbyId)
        => EmitSignal(SignalName.LobbyLeft, lobbyId);

    public void SetLobbyJoinable(ulong lobbyId, bool joinable) { }
    public string GetLobbyData(ulong lobbyId, string key) => "";

    public Godot.Collections.Dictionary<string, string> GetAllLobbyData(ulong lobbyId)
        => new Godot.Collections.Dictionary<string, string>();

    public void SetLobbyData(ulong lobbyId, string key, string value) { }
}
#endif

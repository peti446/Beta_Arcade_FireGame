using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public enum ENetworkState
{
    IDLE, InLobby, JoiningMatch, InMatchLobby, Playing, EndGame
}

public class MainNetworkManager : NetworkManager
{

    #region DATA
    //Variables to be set in the inspector
    [SerializeField]
    private GameObject m_NetworkPlayerPrefab;
    [SerializeField]
    private uint m_MaxPlayersPerMatch = 6;

    //Properties
    /// <summary>
    /// Current state of the network manager.
    /// </summary>
    public ENetworkState State
    {
        get;
        private set;
    }

    /// <summary>
    /// List of all player currently connected.
    /// </summary>
    public IList<NetworkPlayer> PlayersConnected
    {
        get;
        private set;
    }

    /// <summary>
    /// Is the current user the server.
    /// </summary>
    public static bool Is_Server
    {
        get
        {
            return NetworkServer.active;
        }
    }

    /// <summary>
    /// Number of players currently connected (short cut for Playerconected.Count).
    /// </summary>
    public int NumberOfPlayers
    {
        get
        {
            return Is_Server ? numPlayers : PlayersConnected.Count;
        }
    }

    /// <summary>
    /// Checks if the match can start, it rquires at least 2 players to start the game.
    /// </summary>
    public bool CanMatchStart
    {
        get
        {
            return NumberOfPlayers >= 2;
        }
    }

    #region Events
    /// <summary>
    /// Invoked when the server stated.
    /// </summary>
    public event Action ServerStarted;
    /// <summary>
    /// Invoked when the client is stopped.
    /// </summary>
    public event Action ClientShutdown;
    /// <summary>
    /// Invoked when the server is stopped.
    /// </summary>
    public event Action ServerShutdown;
    /// <summary>
    /// Invoked when the server disconected.
    /// </summary>
    public event Action ClientDisconnectedServer;
    /// <summary>
    /// Invoked when the host started.
    /// </summary>
    public event Action HostStarted;
    /// <summary>
    /// Invoked when the host is stopped.
    /// </summary>
    public event Action HostShutdown;
    /// <summary>
    /// Invoked when the current connection to the server is dropped/disconecteed.
    /// </summary>
    public event Action ConnectionDroped;
    /// <summary>
    /// Invoked when a new player has connected to the match and he has been added to the list of clients. Aka a player connected.
    /// </summary>
    public event Action<NetworkPlayer> NetworkPlayerAdded;
    /// <summary>
    /// Invoked when a player is removed from the list of client. Aka a player disconected.
    /// </summary>
    public event Action<NetworkPlayer> NetworkPlayerRemoved;
    /// <summary>
    /// Invoked when a client disconnected. There might not be a player object anymore at this point.
    /// </summary>
    public event Action<NetworkConnection> ClientDisconected;
    /// <summary>
    /// Invoked when a client connected. There is not a player object yet.
    /// </summary>
    public event Action<NetworkConnection> ClientConnected;
    /// <summary>
    /// Invoked when an error happend on a client.
    /// </summary>
    public event Action<NetworkConnection, int> ClientErrorHappend;
    /// <summary>
    /// Invoked when an error happend on the server.
    /// </summary>
    public event Action<NetworkConnection, int> ServerErrorHappend;
    /// <summary>
    /// Invoked when the match has been created.
    /// </summary>
    public event Action<bool, string, MatchInfo> MatchCreated;
    /// <summary>
    /// Invoked when joined a match.
    /// </summary>
    public event Action<bool, string, MatchInfo> MatchJoined;
    /// <summary>
    /// Invoked on the server when all players are ready so we can start the game.
    /// </summary>
    public event Action ServerAllPlayersGotReady;
    #endregion


    //Actions callback to use when creating or joining a server, used monstly for UI porpuses
    private Action<bool, string, MatchInfo> m_OnMatchCreateCallback;
    private Action<bool, string, MatchInfo> m_OnMatchcJoinedCallback;
    #endregion

    #region Instance Handling
    /// <summary>
    /// Instance to this singleton
    /// </summary>
    public static MainNetworkManager _instance
    {
        get;
        private set;
    }

    void Awake()
    {
        //Check if we already got an instance, if so delete the object
        if(_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        //Create a new instance of the instance
        _instance = this;
        //Dont destroy the object on load
        DontDestroyOnLoad(gameObject);
        //Set state
        State = ENetworkState.IDLE;
        PlayersConnected = new List<NetworkPlayer>();
    }

    private void OnDestroy()
    {
        //Make sure we clear the instance static variable to be able to recreate it later
        if (_instance == this)
            _instance = null;
    }
    #endregion


    #region Public Fnctions
    /// <summary>
    /// Disconects the Network manager and returns to an IDLE state. (The state we should be in when first starting the game).
    /// </summary>
    public void Disconect()
    {
        //Switch over the state as depending on the state we disconect in diferent ways
        switch (State)
        {
            case ENetworkState.InLobby:
                //We are just in the match list so just stop the matchmaker, in this state a server should not exist so if we are the server the is somthing wrong
                if (Is_Server)
                    return;
                StopMatchMaker();
                break;
            case ENetworkState.JoiningMatch:
                //Just close the match maker and stop the server/client. Also reset the matchInfo so nobody can acces it or get wrong data.
                StopMatchMaker();
                if (Is_Server)
                    StopServer();
                else
                    StopClient();
                matchInfo = null;
                break;
            case ENetworkState.InMatchLobby:
            case ENetworkState.Playing:
                //If the client/server is playing or in a lobby we need to drop the match as a server or disconect as a client.
                //If we dont have a matchmaker or matchinfo somthing went wrong as there should be. So just to be sure stop every process
                if(Is_Server)
                {
                    if (matchMaker != null && matchInfo != null)
                    {
                        //Delete the match and close everything afterwards
                        matchMaker.DestroyMatch(matchInfo.networkId, 0, (success, info) =>
                        {
                            if (!success)
                            {
                                Debug.LogErrorFormat("Failed to terminate matchmaking game. {0}", info);
                            }
                            StopMatchMaker();
                            StopHost();

                            matchInfo = null;
                        });
                    }
                    else
                    {
                        //Close everything and reset data
                        StopMatchMaker();
                        StopHost();
                        matchInfo = null;
                    }
                }
                else
                {
                    if (matchMaker != null && matchInfo != null)
                    {
                        //Disconect from the match and close everything afterwards
                        matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, (success, info) =>
                        {
                            if (!success)
                            {
                                Debug.LogErrorFormat("Failed to disconnect from matchmaking game. {0}", info);
                            }
                            StopMatchMaker();
                            StopClient();
                            matchInfo = null;
                        });
                    }
                    else
                    {
                        //Close everything and reset data
                        StopMatchMaker();
                        StopClient();
                        matchInfo = null;
                    }
                }
                break;
        }
        //Set the state to IDLE
        State = ENetworkState.IDLE;
    }

    /// <summary>
    /// Adds a NetworkPlayer to the networks manager pool of clients and sets the ID, also the manager will hook to some player events to be able to manage it on state changes.
    /// </summary>
    /// <param name="player">The NetworkPlayer in cuestion</param>
    public void AddNetPlayer(NetworkPlayer player)
    {
        //Add the player to the list and hook the ready event so we can check to start the game
        PlayersConnected.Add(player);
        player.PlayerBecameReady += NetPlayerGotReady;

        //If we are the server update the ID of the players so they are unique
        if(Is_Server)
        {
            UpdatePlayers_ID();
        }

        //TODO: Check where the player is to instanciate the correct prefab
        //Check where the player is to instanciate the correct object to play
        if(MainMenuUIHandler._instance != null)
        {
           player.LobbyLoaded();
        }

        //Fire event
        if (NetworkPlayerAdded != null)
        {
            NetworkPlayerAdded.Invoke(player);
        }
    }

    /// <summary>
    /// Removes a NetworkPlayer from the manager and if it is fired on the server update the IDS.
    /// </summary>
    /// <param name="player">The network player to remove</param>
    public void RemoveNetPlayer(NetworkPlayer player)
    {
        //Rmove the player and uppdate the ids
        PlayersConnected.Remove(player);
        UpdatePlayers_ID();

        //Clean up references
        if (player != null)
        {
            player.PlayerBecameReady -= NetPlayerGotReady;
        }

        //Fire event
        if (NetworkPlayerRemoved != null)
        {
            NetworkPlayerRemoved.Invoke(player);
        }
        
    }

    /// <summary>
    /// Checks if all players in the match are ready currently.
    /// </summary>
    /// <returns>true if all players are ready, false otherwise</returns>
    public bool AreAllPlayersReady()
    {
        //Check if we are not enough players so we dont need to go over the players list
        if (!CanMatchStart)
        {
            return false;
        }

        //check if anyones is not ready
        foreach(NetworkPlayer p in PlayersConnected)
        {
            if (!p.Is_ready)
                return false;
        }

        return true;
    }

    //Function that will be fired when a player get ready
    private void NetPlayerGotReady(NetworkPlayer p)
    {
        if(AreAllPlayersReady() && ServerAllPlayersGotReady != null)
        {
            ServerAllPlayersGotReady.Invoke();
        }
    }

    //Update Ids function easier as to use copy pasto so in case we need to edit it
    private void UpdatePlayers_ID()
    {
        //Just set the ID based on their position in the array
        for (int i = 0; i < PlayersConnected.Count; i++)
            PlayersConnected[i].SetID(i);
    }

    //Resets the ready status for all players
    private void ClearAllPlayersReadyStatus()
    {
        foreach (NetworkPlayer p in PlayersConnected)
            p.ClearReadyStatus();
    }

    /// <summary>
    /// Connects to unity matchmaking server, nececarry to retrive matches info.
    /// </summary>
    public void StartUnityMatchmaking()
    {
        //If we are not in IDLE the matchmaker should already be started or is not needed so dont do anything
        if(State != ENetworkState.IDLE)
        {
            Debug.Log("Can only connect to the Unity Matchmaking server if not already connected to one");
            return;
        }

        State = ENetworkState.InLobby;
        StartMatchMaker();
    }

    /// <summary>
    /// Sends a request to creates a new match, once processed the provided callback will be executed.
    /// </summary>
    /// <param name="name">Name of the match</param>
    /// <param name="onMatchCreatedCallback">The callback that will get executed once the match creation tequest has been procesed by the server</param>
    public void CreateUnityMatchmakingMatch(string name, Action<bool, string, MatchInfo> onMatchCreatedCallback)
    {
        //Check if are in the correct state and are not already in a match
        if(State != ENetworkState.IDLE && State != ENetworkState.InLobby)
        {
            Debug.Log("State is not IDLE, so we are soppoused to be in some kind of game or lobby (?)");
            return;
        }

        //If we already started the matchmaker why do it agian?
        if (State == ENetworkState.IDLE)
            StartMatchMaker();


        //Update the state and local varables/calback
        State = ENetworkState.JoiningMatch;
        m_OnMatchCreateCallback = onMatchCreatedCallback;
        matchName = name;
        //Send request to create the match
        matchMaker.CreateMatch(name, m_MaxPlayersPerMatch, true, string.Empty, string.Empty, string.Empty, 0, 0, OnMatchCreate);
    }

    /// <summary>
    /// Sends a request to join match, once processed the provided callback will be executed.
    /// </summary>
    /// <param name="netID">The match id provided, each server entry will know its ID</param>
    /// <param name="onMatchJoinedCallback">A callback that will get executed once the match join request has been processed by the server</param>
    public void JoinUnityMatchmakingMatch(UnityEngine.Networking.Types.NetworkID netID, Action<bool, string, MatchInfo> onMatchJoinedCallback)
    {
        //To join a server we neet to pass trough the lobby first
        if (State != ENetworkState.InLobby)
        {
            Debug.Log("Not connected to server, connect first");
            return;
        }

        //Update data and set callback
        State = ENetworkState.JoiningMatch;
        m_OnMatchcJoinedCallback = onMatchJoinedCallback;
        //Send request to join the match
        matchMaker.JoinMatch(netID, string.Empty, string.Empty, string.Empty, 0, 0, OnMatchJoined);
    }
    #endregion


    #region Overrides from superclas
    public override void OnMatchCreate(bool success, string info, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, info, matchInfo);

        //Update status if sucesfull
        State = success ? ENetworkState.InMatchLobby : ENetworkState.IDLE;


        //Invoke the caallback
        if(m_OnMatchCreateCallback != null)
        {
            m_OnMatchCreateCallback.Invoke(success, info, matchInfo);
            m_OnMatchCreateCallback = null;
        }

        //Fire the event
        if(MatchCreated != null)
        {
            MatchCreated.Invoke(success, info, matchInfo);
        }
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);

        //Update status if sucesfull
        State = success ? ENetworkState.InMatchLobby : ENetworkState.InLobby;


        //Invoke the callback
        if(m_OnMatchcJoinedCallback != null)
        {
            m_OnMatchcJoinedCallback.Invoke(success, extendedInfo, matchInfo);
            m_OnMatchcJoinedCallback = null;
        }

        //Fire the event
        if(MatchJoined != null)
        {
            MatchJoined.Invoke(success, extendedInfo, matchInfo);
        }
    }

    public override void OnDropConnection(bool sucess, string extraInfo)
    {
        //Base handling
        base.OnDropConnection(sucess, extraInfo);

        //Just fire event
        if(ConnectionDroped != null)
        {
            ConnectionDroped.Invoke();
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        //Make the client conection ready, create local object for the client and sends the info to the server to add us to the match
        ClientScene.Ready(conn);
        ClientScene.AddPlayer(0);

        //Fire the event
        if(ClientConnected != null)
        {
            ClientConnected.Invoke(conn);
        }

    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        //Base handling
        base.OnClientDisconnect(conn);

        //Just fire the event
        if (ClientDisconected != null)
        {
            ClientDisconected.Invoke(conn);
        }
    }

    public override void OnStopClient()
    {
        //Base handling
        base.OnStopClient();

        //Destroy all local players. This will only affect the client stopping not any other client or the server in the match
        foreach (NetworkPlayer p in PlayersConnected)
        {
            if (p != null)
                Destroy(p.gameObject);
        }
        PlayersConnected.Clear();

        //Fire the event
        if (ClientShutdown != null)
        {
            ClientShutdown.Invoke();
        }
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        //Base handling
        base.OnClientError(conn, errorCode);
        //Just fire the event
        if(ClientErrorHappend != null)
        {
            ClientErrorHappend.Invoke(conn, errorCode);
        }
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        //Base handling
        base.OnClientDisconnect(conn);
        //Just fire the event
        if(ServerErrorHappend != null)
        {
            ServerErrorHappend.Invoke(conn, errorCode);
        }
    }

    //Called only on the server when a player is connected and signaled that the connection is ready
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        //Instanciate a new NetworPlayer object and add the player to the connection
        GameObject o = Instantiate(m_NetworkPlayerPrefab);
        NetworkPlayer p = o.GetComponent<NetworkPlayer>();
        if (p == null)
            p = o.AddComponent<NetworkPlayer>();
        //Mark it as dont destory on load as if we switch scene the player object should still exist ( The connection is not dropped)
        DontDestroyOnLoad(p);
        NetworkServer.AddPlayerForConnection(conn, o, playerControllerId);
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        //Vase handling
        base.OnServerRemovePlayer(conn, player);

        //Ge the player from the connection and remove it from the game and the players list
        NetworkPlayer p = conn.playerControllers[0].gameObject.GetComponent<NetworkPlayer>();
        if(p != null)
        {
            Destroy(p.gameObject);
            PlayersConnected.Remove(p);
        }
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        //If the server connects check if we are in the correct state and that we did not hit the player limit. If so disconect
        if (numPlayers >= m_MaxPlayersPerMatch || State != ENetworkState.InMatchLobby)
        {
            conn.Disconnect();
        }
        else
        {
            //Clear players ready status
            ClearAllPlayersReadyStatus();
        }
        base.OnServerConnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //Base handeling
        base.OnServerDisconnect(conn);

        //If in the match lobby clear all the ready status from all players. Anyways the server disconected so the match is been deleted, and so all clients disconected
        if (State == ENetworkState.InMatchLobby)
        {
            ClearAllPlayersReadyStatus();
        }

        //Fire the event
        if (ClientDisconnectedServer != null)
        {
            ClientDisconnectedServer.Invoke();
        }
    }

    public override void OnStartServer()
    {
        //Base handling
        base.OnStartServer();
        //Reset the network scene name
        networkSceneName = string.Empty;

        //Fire event
        if(ServerStarted != null)
        {
            ServerStarted.Invoke();
        }
    }

    public override void OnStopServer()
    {
        //Base handling
        base.OnStopServer();

        //Discconect and destroy all clients on the server and clients
        for (int i = 0; i < PlayersConnected.Count; i++)
        {
            NetworkPlayer p = PlayersConnected[i];
            if (p != null)
                NetworkServer.Destroy(p.gameObject);
        }
        PlayersConnected.Clear();
        //Reset network the scene name
        networkSceneName = string.Empty;

        //Fire evemt
        if (ServerShutdown != null)
        {
            ServerShutdown.Invoke();
        }
    }

    public override void OnStartHost()
    {
        //Base handling
        base.OnStartHost();

        //Just fire the event
        if(HostStarted  != null)
        {
            HostStarted.Invoke();
        }
    }

    public override void OnStopHost()
    {
        //Base handling
        base.OnStopHost();

        //Just fire the event
        if(HostShutdown != null)
        {
            HostShutdown.Invoke();
        }
    }
    #endregion
}

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

public class MainNetworkManager : UnityEngine.Networking.NetworkManager
{
    //https://docs.unity3d.com/ScriptReference/Networking.NetworkManager.html


    #region DATA
    //Variables
    [SerializeField]
    private GameObject m_NetworkPlayerPrefab;
    [SerializeField]
    private uint m_MaxPlayersPerMatch = 6;
    public ENetworkState State
    {
        get;
        private set;
    }

    public IList<NetworkPlayer> PlayersConnected
    {
        get;
        private set;
    }

    public static bool Is_Server
    {
        get
        {
            return NetworkServer.active;
        }
    }

    //Events
    public event Action ServerStarted;
    public event Action ClientShutdown;
    public event Action ServerShutdown;
    public event Action ClientDisconnectedServer;
    public event Action<NetworkPlayer> NetworkPlayerAdded;
    public event Action<NetworkPlayer> NetworkPlayerRemoved;
    public event Action<NetworkConnection> ClientDisconected;
    public event Action<NetworkConnection> ClientConnected;
    public event Action<NetworkConnection, int> ClientErrorHappend;
    public event Action<NetworkConnection, int> ServerErrorHappend;
    public event Action<bool, string, MatchInfo> MatchCreated;


    //Actions
    private Action<bool, string, MatchInfo> m_OnMatchCreateCallback;
    private Action<bool, string, MatchInfo> m_OnMatchcJoinedCallback;
    #endregion

    #region Instance Handling
    //Static instance variable
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
    public void Disconect()
    {
        switch (State)
        {
            case ENetworkState.InLobby:
                if (Is_Server)
                    return;
                StopMatchMaker();
                break;
            case ENetworkState.JoiningMatch:
                StopMatchMaker();
                if (Is_Server)
                    StopServer();
                else
                    StopClient();
                matchInfo = null;
                break;
            case ENetworkState.InMatchLobby:
            case ENetworkState.Playing:
                if(Is_Server)
                {
                    if (matchMaker != null && matchInfo != null)
                    {
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
                        StopMatchMaker();
                        StopHost();
                        matchInfo = null;
                    }
                }
                else
                {
                    if (matchMaker != null && matchInfo != null)
                    {
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
                        StopMatchMaker();
                        StopClient();
                        matchInfo = null;
                    }
                }
                break;
        }
    }

    public void AddNetPlayer(NetworkPlayer player)
    {
        PlayersConnected.Add(player);
        
        if(Is_Server)
        {
            UpdatePlayers_ID();
        }
    }

    public void RemoveNetPlayer(NetworkPlayer player)
    {
        PlayersConnected.Remove(player);
        UpdatePlayers_ID();
        
    }

    public void UpdatePlayers_ID()
    {
        if (!Is_Server)
            return;
        for (int i = 0; i < PlayersConnected.Count; i++)
            PlayersConnected[i].SetID(i);
    }

    public void StartUnityMatchmaking()
    {
        if(State != ENetworkState.IDLE)
        {
            Debug.Log("Can only connect to the Unity Matchmaking server if not already connected to one");
        }

        State = ENetworkState.InLobby;
        StartMatchMaker();
    }

    public void CreateUnityMatchmakingMatch(string name, Action<bool, string, MatchInfo> onMatchCreatedCallback)
    {
        if(State != ENetworkState.IDLE && State != ENetworkState.InLobby)
        {
            Debug.Log("State is not IDLE, so we are soppoused to be in some kind of game or lobby (?)");
        }

        if (State == ENetworkState.IDLE)
            StartMatchMaker();

        State = ENetworkState.JoiningMatch;
        m_OnMatchCreateCallback = onMatchCreatedCallback;

        matchMaker.CreateMatch(name, m_MaxPlayersPerMatch, true, string.Empty, string.Empty, string.Empty, 0, 0, OnMatchCreate);
    }

    public void JoinUnityMatchmakingMatch(UnityEngine.Networking.Types.NetworkID netID, Action<bool, string, MatchInfo> onMatchJoinedCallback)
    {
        if (State != ENetworkState.InLobby)
        {
            Debug.Log("Not connected to server, connect first");
        }

        State = ENetworkState.JoiningMatch;
        m_OnMatchcJoinedCallback = onMatchJoinedCallback;

        matchMaker.JoinMatch(netID, string.Empty, string.Empty, string.Empty, 0, 0, OnMatchJoined);
    }
    #endregion


    #region Overrides from superclas
    public override void OnMatchCreate(bool success, string info, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, info, matchInfo);

        State = success ? ENetworkState.InMatchLobby : ENetworkState.IDLE;
        if(m_OnMatchCreateCallback != null)
        {
            m_OnMatchCreateCallback.Invoke(success, info, matchInfo);
            m_OnMatchCreateCallback = null;
        }

        if(MatchCreated != null)
        {
            MatchCreated.Invoke(success, info, matchInfo);
        }
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);

        State = success ? ENetworkState.InMatchLobby : ENetworkState.InLobby;

        if(m_OnMatchcJoinedCallback != null)
        {
            m_OnMatchcJoinedCallback.Invoke(success, extendedInfo, matchInfo);
            m_OnMatchcJoinedCallback = null;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        foreach(NetworkPlayer p in PlayersConnected)
        {
            if (p != null)
                Destroy(p.gameObject);
        }
        PlayersConnected.Clear();

        if(ClientShutdown != null)
        {
            ClientShutdown.Invoke();
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);


        if (ClientDisconected != null)
        {
            ClientDisconected.Invoke(conn);
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        //Lets add a new player
        GameObject o = Instantiate(m_NetworkPlayerPrefab);
        NetworkPlayer p = o.GetComponent<NetworkPlayer>();
        if (p == null)
            p = o.AddComponent<NetworkPlayer>();

        DontDestroyOnLoad(p);
        NetworkServer.AddPlayerForConnection(conn, o, playerControllerId);
    }
    #endregion
}

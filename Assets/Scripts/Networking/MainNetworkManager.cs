using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public enum ENetworkState
{
    IDLE, InLobby, JoiningMatch, InMatchLobby, Playing, EndGame
}

public class MainNetworkManager : UnityEngine.Networking.NetworkManager
{
    //https://docs.unity3d.com/ScriptReference/Networking.NetworkManager.html

    //Variables
    public ENetworkState state
    {
        get;
        private set;
    }

    //Actions
    private Action<bool, string, MatchInfo> m_OnMatchCreateCallback;
    private Action<bool, string, MatchInfo> m_OnMatchcJoinedCallback;


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
        state = ENetworkState.IDLE;
    }

    private void OnDestroy()
    {
        //Make sure we clear the instance static variable to be able to recreate it later
        if (_instance == this)
            _instance = null;
    }

    public void Disconect()
    {
        state = ENetworkState.IDLE;
        StopMatchMaker();
    }

    public void StartUnityMatchmaking()
    {
        if(state != ENetworkState.IDLE)
        {
            Debug.Log("Can only connect to the Unity Matchmaking server if not already connected to one");
        }

        state = ENetworkState.InLobby;
        StartMatchMaker();
    }

    public void CreateUnityMatchmakingMatch(string name, Action<bool, string, MatchInfo> onMatchCreatedCallback)
    {
        if(state != ENetworkState.IDLE)
        {
            Debug.Log("State is not IDLE, so we are soppoused to be in some kind of game or lobby (?)");
        }

        state = ENetworkState.JoiningMatch;
        m_OnMatchCreateCallback = onMatchCreatedCallback;

        StartMatchMaker();

        matchMaker.CreateMatch(name, 6, true, string.Empty, string.Empty, string.Empty, 0, 0, OnMatchCreate);
    }

    public override void OnMatchCreate(bool success, string info, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, info, matchInfo);

        state = success ? ENetworkState.InMatchLobby : ENetworkState.IDLE;
        if(m_OnMatchCreateCallback != null)
        {
            m_OnMatchCreateCallback.Invoke(success, info, matchInfo);
            m_OnMatchCreateCallback = null;
        }
    }

    public void JoinUnityMatchmakingMatch(UnityEngine.Networking.Types.NetworkID netID, Action<bool, string, MatchInfo> onMatchJoinedCallback)
    {
        if(state != ENetworkState.InLobby)
        {
            Debug.Log("Not connected to server, connect first");
        }

        state = ENetworkState.JoiningMatch;
        m_OnMatchcJoinedCallback = onMatchJoinedCallback;

        matchMaker.JoinMatch(netID, string.Empty, string.Empty, string.Empty, 0, 0, OnMatchJoined);
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);

        state = success ? ENetworkState.InMatchLobby : ENetworkState.InLobby;

        if(m_OnMatchcJoinedCallback != null)
        {
            m_OnMatchcJoinedCallback.Invoke(success, extendedInfo, matchInfo);
            m_OnMatchcJoinedCallback = null;
        }
    }
}

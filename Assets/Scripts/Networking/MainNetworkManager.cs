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
    //Variables
    public ENetworkState state
    {
        get;
        private set;
    }


    //Static instance variable
    public static MainNetworkManager _instance
    {
        get;
        protected set;
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

    public void CreateUnityMatchmakingMatch(string name)
    {
        if(state != ENetworkState.IDLE)
        {
            Debug.Log("State is not IDLE, so we are soppoused to be in some kind of game or lobby (?)");
            throw new Exception("State is not IDLE, this means we should be in some kind of game or lobby.");
        }

        state = ENetworkState.JoiningMatch;

        StartMatchMaker();

        matchMaker.CreateMatch(name, 6, true, string.Empty, string.Empty, string.Empty, 0, 0, OnMatchCreate);
    }

    public override void OnMatchCreate(bool success, string info, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, info, matchInfo);

        state = success ? ENetworkState.InMatchLobby : ENetworkState.IDLE;
    }

    public void JoinUnityMatchmakingMatch(UnityEngine.Networking.Types.NetworkID netID)
    {
        if(state != ENetworkState.InLobby)
        {
            Debug.Log("");
        }

        state = ENetworkState.JoiningMatch;

        matchMaker.JoinMatch(netID, string.Empty, string.Empty, string.Empty, 0, 0, OnMatchJoined);
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);

        state = success ? ENetworkState.InMatchLobby : ENetworkState.InLobby;
    }
}

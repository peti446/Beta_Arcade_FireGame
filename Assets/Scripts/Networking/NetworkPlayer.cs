using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum ETeams
{
    FireFighters, CrazyPeople
}

public class NetworkPlayer : NetworkBehaviour {

    //Prefabs to instanciate and to be controlled by the player
    [SerializeField]
    private GameObject m_LobbyPlayerPref;
    [SerializeField]
    private GameObject m_CrazyManPrefab;
    [SerializeField]
    private GameObject m_FiremanPrefab;

    //Sync vars that get replciated over the network
    [SyncVar(hook = "OnNameChanged")]
    private string m_Name = "";
    [SyncVar(hook = "OnReadyStatusChanged")]
    private bool m_ready = false;
    [SyncVar(hook = "OnTeamChanged")]
    private ETeams m_team = ETeams.FireFighters;
    [SyncVar(hook = "OnInitStatusChanged")]
    private bool m_Initialized = false;
    [SyncVar]
    private int m_ID;

    //Instances of the playeable objects
    //The lobby object that allows to customise you player and lets you 
    private MatchLobbyPlayer m_matchLobbyPlayer;

    /// <summary>
    /// Return the players ready status
    /// </summary>
    public bool Is_ready
    {
        get { return m_ready; }
    }
    /// <summary>
    /// Returns the current player name
    /// </summary>
    public string Player_Name
    {
        get { return m_Name; }
    }
    /// <summary>
    /// Returns the team the player is currently one. It might not match the UI if the UI has not been updated it. You should never use the UI to check for a players team.
    /// </summary>
    public ETeams Player_Team
    {
        get { return m_team; }
    }
    /// <summary>
    /// The ID imposed by the server for this client
    /// </summary>
    public int ID
    {
        get { return m_ID; }
    }

    /// <summary>
    /// Invoked when any of the data of the player instance changed on a client
    /// </summary>
    public event Action<NetworkPlayer> NetworkPlayerDataUpdated;
    /// <summary>
    /// Invoked when the player changed it status to be ready. Only on the server
    /// </summary>
    public event Action<NetworkPlayer> PlayerBecameReady;
    /// <summary>
    /// Invoked when the player changed it status to not be ready. Only on the server
    /// </summary>
    public event Action<NetworkPlayer> PlayerBecameUnReady;
    /// <summary>
    /// Invoked when the player name is updated on the client
    /// </summary>
    public event Action<NetworkPlayer> PlayerNameChanged;

    //Local player setup
    [Client]
    public override void OnStartLocalPlayer()
    {
        //Call the base start for local player
        base.OnStartLocalPlayer();
        //TODO: Proper automatic name generation
        //Generate random name and send the server information aobut ourselves, the name might change on the server.
        String s = Guid.NewGuid().ToString("N");
        CmdSetUpPlayer(s);
        //Set the local player on the manager
        MainNetworkManager._instance.SetLocalPlayerRef(this);
    }

    //Set up on all clients once the server got it setup
    [Client]
    public override void OnStartClient()
    {
        //When the client start on any game make sure its not destroyed on load
        DontDestroyOnLoad(this);
        //Execute base
        base.OnStartClient();

        //Add it to the list o players connected/ and set everything up
        MainNetworkManager._instance.AddNetPlayer(this);
    }

    //Destroys the client
    public override void OnNetworkDestroy()
    {
        //Base destoy
        base.OnNetworkDestroy();

        //Check if there is any playeable object if so destroy it
        if(m_matchLobbyPlayer != null)
        {
            Destroy(m_matchLobbyPlayer.gameObject);
        }

        if (MatchSettings._instance != null)
            MatchSettings._instance.RemovePlayer(this);

        //Send the new data to all clients
        RpcTaemSizesUpdate(MatchSettings._instance.GetTeamMembersId(ETeams.CrazyPeople), MatchSettings._instance.GetTeamMembersId(ETeams.FireFighters));

        //Remove the player from the players list if the network manager still exists
        if (MainNetworkManager._instance != null)
        {
            MainNetworkManager._instance.RemoveNetPlayer(this);
        }
    }

    //Normal object destroy after the network connection has been destroyed
    public void OnDestroy()
    {
        if (m_matchLobbyPlayer != null)
        {
            Destroy(m_matchLobbyPlayer.gameObject);

        }
        if (MatchSettings._instance != null)
            MatchSettings._instance.RemovePlayer(this);
    }

    /// <summary>
    /// Creates a playeable lobby object and inits it. This can only happend if there is nor already a lobby object for this players and the current player is initalized.
    /// </summary>
    [Client]
    public void LobbyLoaded()
    {
        //Check if we can create a lobby object for this player. (Dont have one already and be setted upon the server)
        if (m_Initialized && m_matchLobbyPlayer == null)
            CreateLobbyPlayer();
    }

    /// <summary>
    /// Creates a player instance for this player if we have the authority aka we are the local player
    /// </summary>
    [Client]
    public void GameSceneLoaded()
    {
        if(hasAuthority)
        {
            CmdSpawnPlayerGameObject();
        }
    }

    #region Server side execution only

    /// <summary>
    /// Sets the ID of player. Can only be run on the server
    /// </summary>
    /// <param name="newID">New ID for the player</param>
    [Server]
    public void SetID(int newID)
    {
        m_ID = newID;
    }

    /// <summary>
    /// Changes the  ready status of the client to false. Can only be run on the server.
    /// </summary>
    [Server]
    public void ClearReadyStatus()
    {
        m_ready = false;
    }
    #endregion

    #region Commands, Called on the client and executed on the server

    /// <summary>
    /// Notifies the server that the players wants to set its flag to ready
    /// </summary>
    [Command]
    public void CmdReady()
    {
        //Check if we can start the match, as if we cant we do not change the player status
        if (MainNetworkManager._instance.CanMatchStart)
        {
            //Set ready flag and execute event
            m_ready = true;
            if (PlayerBecameReady != null)
            {
                PlayerBecameReady.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Notifies the server that the players wants to set its flag to not ready
    /// </summary>
    [Command]
    public void CmdUnReady()
    {
        //Update the flag and invoke event
        m_ready = false;
        if (PlayerBecameUnReady != null)
        {
            PlayerBecameUnReady.Invoke(this);
        }
    }

    /// <summary>
    /// Set up the player with all the nececary data
    /// </summary>
    /// <param name="name">The initial players username</param>
    [Command]
    private void CmdSetUpPlayer(string name)
    {
        //Set all the data and initialize it
        m_Name = name;
        m_team = MatchSettings._instance.TryToAddPlayerToTeam(this, MatchSettings._instance.GetNewPlayerStartingTeam());
        m_Initialized = true;
    }

    /// <summary>
    /// Changes the usernamename of the player, if it has not been taken
    /// </summary>
    /// <param name="newName">The new username</param>
    [Command]
    public void CmdChangeName(string newName)
    {
        //Check if there is any player already with the same name
        foreach(NetworkPlayer p in MainNetworkManager._instance.PlayersConnected)
        {
            if(p.Player_Name == newName)
            {
                TargetUsernameChangeError(connectionToClient, newName);
                return;
            }
        }
        //No other player with this name so update
        m_Name = newName;
    }

    /// <summary>
    /// Switches the players team if possible
    /// </summary>
    [Command]
    public void CmdSwitchTeam()
    {
        //Update the team of the player if possible
        switch(m_team)
        {
            case ETeams.CrazyPeople:
                    m_team = MatchSettings._instance.TryToAddPlayerToTeam(this, ETeams.FireFighters);
                break;
            case ETeams.FireFighters:
                    m_team = MatchSettings._instance.TryToAddPlayerToTeam(this, ETeams.CrazyPeople);
                break;
        }
        //Send the new data to all clients
        RpcTaemSizesUpdate(MatchSettings._instance.GetTeamMembersId(ETeams.CrazyPeople), MatchSettings._instance.GetTeamMembersId(ETeams.FireFighters));
        //Reset ready status for everyone
        MainNetworkManager._instance.ClearAllPlayersReadyStatus();
    }

    /// <summary>
    /// Called on the server when the client is loaded up in the game
    /// </summary>
    [Command]
    public void CmdSpawnPlayerGameObject()
    {
        StartCoroutine(SpawnPlayerWhenReady());
    }

    private IEnumerator SpawnPlayerWhenReady()
    {
        while (!connectionToClient.isReady)
        {
            yield return new WaitForSeconds(0.1f);
        }
        SpawnPlayerObject();
    }

    [Server]
    private void SpawnPlayerObject()
    {
        //Based on team spawn different charactr
        GameObject o = null;
        switch (m_team)
        {
            case ETeams.CrazyPeople:
                o = Instantiate(m_CrazyManPrefab);
                break;
            case ETeams.FireFighters:
                o = Instantiate(m_FiremanPrefab);
                break;
        }

        //Just in case
        if (o == null)
        {
            Debug.LogError("Player Object not spawned, even it should of been");
            return;
        }

        //Give the authority to the player and make it spawn on every client
        NetworkServer.SpawnWithClientAuthority(o, connectionToClient);

        //Set the id to the character
        Character c = o.GetComponent<Character>();
        if (c != null)
        {
            c.SetPlayerID(m_ID);
        }
        else
        {
            Debug.LogError("Somthing went wrong, the character prefab does not have a character (?)");
        }

        o.transform.position = SpawnManager._instance.GetSpawnPoint(m_team).position;
    }
    #endregion

    #region Client Rpc
    /// <summary>
    /// Update the local players match settings teams list
    /// </summary>
    /// <param name="teamPlayersId1">players id in team one</param>
    /// <param name="teamPlayersId2">players id in team two</param>
    [ClientRpc]
    public void RpcTaemSizesUpdate(int[] teamPlayersId1, int[] teamPlayersId2)
    {
        if (!MainNetworkManager.Is_Server)
        {
            //Update both teams
            MatchSettings._instance.SetTeamFromIds(teamPlayersId1, ETeams.CrazyPeople);
            MatchSettings._instance.SetTeamFromIds(teamPlayersId2, ETeams.FireFighters);
        }
    }
    #endregion

    #region Target Client RPC
    /// <summary>
    /// Displays an error on the client that the username is already taken
    /// </summary>
    /// <param name="target"></param>
    /// <param name="newUsername"></param>
    [TargetRpc]
    public void TargetUsernameChangeError(NetworkConnection target, string newUsername)
    {
        //Check if there is a lobby player before showing the erro
        if(m_matchLobbyPlayer != null)
        {
            m_matchLobbyPlayer.DisplayUsernameError(newUsername);
        }
    }

    /// <summary>
    /// Makes the client show their loadins creen if it does exist.
    /// </summary>
    /// <param name="target">The player connection to send the message</param>
    [TargetRpc]
    public void TargetEnableLoadingScreen(NetworkConnection target)
    {
        LoadingScreen ls = LoadingScreen._instance;
        if(ls != null)
        {
            ls.Show();
        }
    }
    #endregion

    #region Sync var changed functions
    //All functions are called on the clients with reference to this object when the servers variable change, so in effect its only a setter but over the network
    private void OnNameChanged(string name)
    {
        m_Name = name;
        OnNetworkPlayerDataUpdated();
        if (PlayerNameChanged != null)
            PlayerNameChanged.Invoke(this);
    }

    private void OnReadyStatusChanged(bool newStatus)
    {
        m_ready = newStatus;
        OnNetworkPlayerDataUpdated();
    }

    private void OnTeamChanged(ETeams newTeam)
    {
        m_team = newTeam;
        OnNetworkPlayerDataUpdated();
    }

    private void OnInitStatusChanged(bool newStatus)
    {
        //Only initialize the player if it has not been initialized yet
        //De-initialization cannot occur
        if (!m_Initialized && newStatus)
        {
            m_Initialized = newStatus;
            CreateLobbyPlayer();
            if(MainNetworkManager.Is_Server)
            {
                ///Send team size info to the player
                RpcTaemSizesUpdate(MatchSettings._instance.GetTeamMembersId(ETeams.CrazyPeople), MatchSettings._instance.GetTeamMembersId(ETeams.FireFighters));
            }
        }
    }
    #endregion

    //Helper to invoke the network player changed data event
    private void OnNetworkPlayerDataUpdated()
    {
        if (NetworkPlayerDataUpdated != null)
            NetworkPlayerDataUpdated.Invoke(this);

    }

    //Helper to instantiate a lobby player and init it
    private void CreateLobbyPlayer()
    {
        m_matchLobbyPlayer = Instantiate(m_LobbyPlayerPref).GetComponent<MatchLobbyPlayer>();
        m_matchLobbyPlayer.InitForPlayer(this);
    }

}

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

    [SerializeField]
    private GameObject m_LobbyPlayerPref;
    [SerializeField]
    private GameObject m_CrazyManPrefab;
    [SerializeField]
    private GameObject m_FiremanPrefab;

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

    private MatchLobbyPlayer m_matchLobbyPlayer;

    //Public getters
    public bool Is_ready
    {
        get { return m_ready; }
    }
    public string Player_Name
    {
        get { return m_Name; }
    }
    public ETeams Player_Team
    {
        get { return m_team; }
    }
    public int ID
    {
        get { return m_ID; }
    }

    //Events
    public event Action<NetworkPlayer> NetworkPlayerDataUpdated;
    public event Action<NetworkPlayer> PlayerBecameReady;
    public event Action<NetworkPlayer> PlayerBecameUnReady;

    [Client]
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        String s = Guid.NewGuid().ToString("N");
        CmdSetUpPlayer(s);
    }

    [Client]
    public override void OnStartClient()
    {
        DontDestroyOnLoad(this);
        base.OnStartClient();

        MainNetworkManager._instance.AddNetPlayer(this);
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        if(m_matchLobbyPlayer != null)
        {
            Destroy(m_matchLobbyPlayer.gameObject);
        }

        if (MainNetworkManager._instance != null)
        {
            MainNetworkManager._instance.RemoveNetPlayer(this);
        }
    }

    public void OnDestroy()
    {
        if (m_matchLobbyPlayer != null)
        {
            Destroy(m_matchLobbyPlayer.gameObject);
        }
    }

    [Client]
    public void LobbyLoaded()
    {
        if (m_Initialized && m_matchLobbyPlayer == null)
            CreateLobbyPlayer();
    }

    #region Server side execution only
    [Server]
    public void SetID(int newID)
    {
        m_ID = newID;
    }

    [Server]
    public void ClearReadyStatus()
    {
        m_ready = false;
    }
    #endregion

    #region Commands
    [Command]
    public void CmdReady()
    {
        if (MainNetworkManager._instance.CanMatchStart)
        {
            m_ready = true;
            if (PlayerBecameReady != null)
            {
                PlayerBecameReady.Invoke(this);
            }
        }
    }

    [Command]
    public void CmdUnReady()
    {
        m_ready = false;
        if (PlayerBecameUnReady != null)
        {
            PlayerBecameUnReady.Invoke(this);
        }
    }

    [Command]
    private void CmdSetUpPlayer(string name)
    {
        m_Name = name;
        m_team = MatchSettings._instance.TryToAddPlayerToTeam(this, MatchSettings._instance.GetNewPlayerStartingTeam());
        m_Initialized = true;
        RpcTaemSizesUpdate(MatchSettings._instance.GetTeamMembersId(ETeams.CrazyPeople), MatchSettings._instance.GetTeamMembersId(ETeams.FireFighters));
    }


    [Command]
    public void CmdChangeName(string newName)
    {
        m_Name = newName;
    }

    [Command]
    public void CmdSwitchTeam()
    {
        switch(m_team)
        {
            case ETeams.CrazyPeople:
                    m_team = MatchSettings._instance.TryToAddPlayerToTeam(this, ETeams.FireFighters);
                break;
            case ETeams.FireFighters:
                    m_team = MatchSettings._instance.TryToAddPlayerToTeam(this, ETeams.CrazyPeople);
                break;
        }
        RpcTaemSizesUpdate(MatchSettings._instance.GetTeamMembersId(ETeams.CrazyPeople), MatchSettings._instance.GetTeamMembersId(ETeams.FireFighters));
    }
    #endregion

    #region Client Rpc
    [ClientRpc]
    public void RpcTaemSizesUpdate(int[] teamPlayersId1, int[] teamPlayersId2)
    {
        MatchSettings._instance.SetTeamFromIds(teamPlayersId1, ETeams.CrazyPeople);
        MatchSettings._instance.SetTeamFromIds(teamPlayersId2, ETeams.FireFighters);
    }
    #endregion

    #region Sync var changed functions
    private void OnNameChanged(string name)
    {
        m_Name = name;
        OnNetworkPlayerDataUpdated();
    }

    private void OnReadyStatusChanged(bool newStatus)
    {
        Debug.Log("Ready Status Changed");
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
        if (!m_Initialized && newStatus)
        {
            m_Initialized = newStatus;
            CreateLobbyPlayer();
        }
    }
    #endregion

    private void OnNetworkPlayerDataUpdated()
    {
        if (NetworkPlayerDataUpdated != null)
            NetworkPlayerDataUpdated.Invoke(this);

    }
    private void CreateLobbyPlayer()
    {
        m_matchLobbyPlayer = Instantiate(m_LobbyPlayerPref).GetComponent<MatchLobbyPlayer>();
        m_matchLobbyPlayer.InitForPlayer(this);
    }

}

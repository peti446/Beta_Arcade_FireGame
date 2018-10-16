using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
    [SyncVar(hook = "OnInitStatusChanged")]
    private bool m_Initialized = false;
    [SyncVar]
    private int m_ID;

    private MatchLobbyPlayer m_matchLobbyPlayer;
    public bool Is_ready
    {
        get { return m_ready; }
    }

    public event Action<NetworkPlayer> NetworkPlayerDataUpdated;
    public event Action<NetworkPlayer> PlayerBecameReady;

    [Client]
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdSetUpPlayer(Guid.NewGuid().ToString("N"));
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
    }

    public void OnDestroy()
    {
        if (MainNetworkManager._instance != null)
        {
            MainNetworkManager._instance.RemoveNetPlayer(this);
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
                PlayerBecameReady(this);
            }
        }
    }

    [Command]
    private void CmdSetUpPlayer(string name)
    {
        m_Name = name;
        m_Initialized = true;
    }


    [Command]
    public void CmdChangeName(string newName)
    {
        m_Name = newName;
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
        m_ready = newStatus;
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

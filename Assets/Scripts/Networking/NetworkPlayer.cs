using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayer : NetworkBehaviour {


    [SyncVar]
    private string m_Name = "";
    [SyncVar]
    private bool m_ready = false;
    [SyncVar]
    private bool m_Initialized = false;
    [SyncVar]
    private int m_ID;

    public event Action<NetworkPlayer> NetworkPlayerDataUpdated;

    [Server]
    public void SetID(int newID)
    {
        m_ID = newID;
    }
}

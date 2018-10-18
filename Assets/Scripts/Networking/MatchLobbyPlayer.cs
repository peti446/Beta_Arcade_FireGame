using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchLobbyPlayer : MonoBehaviour {

    [SerializeField]
    private Text m_name;
    [SerializeField]
    private Text m_ready_Text;
    [SerializeField]
    private Text m_waiting_Text;
    private Button m_readyButton;

    private NetworkPlayer m_NetworkPlayer;

    private void OnDestroy()
    {
        if (m_NetworkPlayer != null)
        {
            m_NetworkPlayer.NetworkPlayerDataUpdated -= OnPlayerDataUpdated;
        }
        MainNetworkManager._instance.NetworkPlayerAdded -= PlayerJoined;
        MainNetworkManager._instance.NetworkPlayerRemoved -= PlayerLeft;
    }

    public void InitForPlayer(NetworkPlayer player)
    {
        if (player == null)
        {
            Destroy(this.gameObject);
            return;
        }

        m_NetworkPlayer = player;
        m_NetworkPlayer.NetworkPlayerDataUpdated += OnPlayerDataUpdated;
        MainNetworkManager._instance.NetworkPlayerAdded += PlayerJoined;
        MainNetworkManager._instance.NetworkPlayerRemoved += PlayerLeft;

        m_ready_Text.gameObject.SetActive(false);

        MainMenuUIHandler._instance.AddPlayerToLobby(this);

        UpdateData();
    }

    public void SetReadyButtonReference(Button readyB)
    {
        if (!m_NetworkPlayer.hasAuthority)
        {
            return;
        }
        Debug.Log("SetreadyButton");
        m_readyButton = readyB;
        UpdateButtonState();
    }

    private void PlayerJoined(NetworkPlayer p)
    {
        UpdateButtonState();
    }
    
    private void PlayerLeft(NetworkPlayer p)
    {
        UpdateButtonState();
    }

    private void UpdateData()
    {
        m_name.text = m_NetworkPlayer.Player_Name;
        m_ready_Text.gameObject.SetActive(m_NetworkPlayer.Is_ready);
        m_waiting_Text.gameObject.SetActive(!m_NetworkPlayer.Is_ready);
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (m_readyButton != null )
        {
            m_readyButton.interactable = MainNetworkManager._instance.CanMatchStart;
            m_readyButton.onClick.RemoveAllListeners();
            if (m_NetworkPlayer.Is_ready)
            {
                m_readyButton.onClick.AddListener(() => { m_NetworkPlayer.CmdReady(); });
                m_readyButton.transform.GetChild(0).GetComponent<Text>().text = "UnReady";
            } 
            else
            {
                m_readyButton.onClick.AddListener(() => { m_NetworkPlayer.CmdReady();});
                m_readyButton.transform.GetChild(0).GetComponent<Text>().text = "Ready";
            }
        }
    }

    private void OnPlayerBecameReady(NetworkPlayer _)
    {
        UpdateData();
    }

    private void OnPlayerBecameUnready(NetworkPlayer _)
    {
        UpdateData();
    }

    private void OnPlayerDataUpdated(NetworkPlayer _)
    {
        UpdateData();
    }
}

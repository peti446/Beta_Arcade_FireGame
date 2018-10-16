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
        m_readyButton = readyB;
        m_readyButton.onClick.RemoveAllListeners();
        m_readyButton.onClick.AddListener(m_NetworkPlayer.CmdReady);
        m_readyButton.interactable = MainNetworkManager._instance.CanMatchStart && m_NetworkPlayer.hasAuthority;
    }

    private void PlayerJoined(NetworkPlayer p)
    {
        m_readyButton.interactable = MainNetworkManager._instance.CanMatchStart && m_NetworkPlayer.hasAuthority;
    }
    
    private void PlayerLeft(NetworkPlayer p)
    {
        m_readyButton.interactable = MainNetworkManager._instance.CanMatchStart && m_NetworkPlayer.hasAuthority;
    }

    private void UpdateData()
    {
        m_name.text = m_NetworkPlayer.Player_Name;
        m_ready_Text.gameObject.SetActive(m_NetworkPlayer.Is_ready);
        m_waiting_Text.gameObject.SetActive(!m_NetworkPlayer.Is_ready);
        m_readyButton.interactable = MainNetworkManager._instance.CanMatchStart && m_NetworkPlayer.hasAuthority;
    }

    private void OnPlayerDataUpdated(NetworkPlayer p)
    {
        UpdateData();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchLobbyPlayer : MonoBehaviour {

    [SerializeField]
    private Text m_name;
    [SerializeField]
    private Text m_readyText;
    [SerializeField]
    private Text m_waitingText;
    private Button m_readyButton;
    private Button m_switchButton;

    private NetworkPlayer m_NetworkPlayer;

    public ETeams Team
    {
        get
        {
            return m_NetworkPlayer.Player_Team;
        }
    }

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

        m_readyText.gameObject.SetActive(false);

        if (MainMenuUIHandler._instance.MatchLobbyScritp)
            MainMenuUIHandler._instance.MatchLobbyScritp.AddLobbyPlayer(this);

        UpdateData();
    }

    public void SetSwitchTeamButton(Button switchTeam)
    {
        if (!m_NetworkPlayer.hasAuthority)
        {
            return;
        }
        m_switchButton = switchTeam;
        m_switchButton.onClick.RemoveAllListeners();
        m_switchButton.onClick.AddListener(() => { m_NetworkPlayer.CmdSwitchTeam(); });
        m_switchButton.interactable = false;
        UpdateButtonState();
    }

    public void SetReadyButtonReference(Button readyB)
    {
        if (!m_NetworkPlayer.hasAuthority)
        {
            return;
        }
        m_readyButton = readyB;
        m_readyButton.interactable = false;
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
        m_readyText.gameObject.SetActive(m_NetworkPlayer.Is_ready);
        m_waitingText.gameObject.SetActive(!m_NetworkPlayer.Is_ready);
        MainMenuUIHandler._instance.MatchLobbyScritp.SwitchLobbyPlayerTeamPanel(this);
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
                m_readyButton.onClick.AddListener(() => { m_NetworkPlayer.CmdUnReady(); });
                m_readyButton.transform.GetChild(0).GetComponent<Text>().text = "UnReady";
            } 
            else
            {
                m_readyButton.onClick.AddListener(() => { m_NetworkPlayer.CmdReady();});
                m_readyButton.transform.GetChild(0).GetComponent<Text>().text = "Ready";
            }
        }

        if (m_switchButton != null)
        {
            m_switchButton.interactable = MatchSettings._instance.CanSwitchTeam(m_NetworkPlayer.Player_Team);
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

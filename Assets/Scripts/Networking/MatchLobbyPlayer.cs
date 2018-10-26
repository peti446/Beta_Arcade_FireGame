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
    [SerializeField]
    private Button m_switchTeamButton;
    [SerializeField]
    private Button m_ShowChangeNameButton;
    [SerializeField]
    private InputField m_nameInputfield;

    private Button m_readyButton;

    private NetworkPlayer m_NetworkPlayer;

    public ETeams Team
    {
        get { return m_NetworkPlayer.Player_Team; }
    }

    private void OnDestroy()
    {
        if (m_NetworkPlayer != null)
        {
            m_NetworkPlayer.NetworkPlayerDataUpdated -= OnPlayerDataUpdated;
        }

        if(MainNetworkManager._instance != null)
        {
            MainNetworkManager._instance.NetworkPlayerAdded -= PlayerJoined;
            MainNetworkManager._instance.NetworkPlayerRemoved -= PlayerLeft;
        }
    }

    public void InitForPlayer(NetworkPlayer player)
    {
        if (player == null)
        {
            Destroy(this.gameObject);
            return;
        }

        //Set events
        m_NetworkPlayer = player;
        m_NetworkPlayer.NetworkPlayerDataUpdated += OnPlayerDataUpdated;
        MainNetworkManager._instance.NetworkPlayerAdded += PlayerJoined;
        MainNetworkManager._instance.NetworkPlayerRemoved += PlayerLeft;

        //By default the player is not the local one
        m_readyText.gameObject.SetActive(false);
        m_switchTeamButton.gameObject.SetActive(false);
        m_ShowChangeNameButton.gameObject.SetActive(false);
        m_nameInputfield.gameObject.SetActive(false);
        m_switchTeamButton.onClick.RemoveAllListeners();
        m_ShowChangeNameButton.onClick.RemoveAllListeners();
        m_nameInputfield.onEndEdit.RemoveAllListeners();

        //Add the player to the UI
        if (MainMenuUIHandler._instance.MatchLobbyScritp)
            MainMenuUIHandler._instance.MatchLobbyScritp.AddLobbyPlayer(this);

        //If it is the local player enable buttons to chage their players data
        if (m_NetworkPlayer.hasAuthority)
        {
            m_name.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            m_switchTeamButton.onClick.AddListener(OnSwitchTeamClicked);
            m_ShowChangeNameButton.onClick.AddListener(OnShowNameInputClicked);
            m_nameInputfield.onEndEdit.AddListener(OnEndEditChangeName);
            m_switchTeamButton.gameObject.SetActive(true);
            m_ShowChangeNameButton.gameObject.SetActive(true);
            m_nameInputfield.gameObject.SetActive(false);
        }

        //Update the data of the player
        UpdateData();
    }


    public void DisplayUsernameError(string error)
    {
       // if (m_inputField != null && m_inputError != null)
        {
          //  m_inputError.gameObject.SetActive(true);
          //  m_inputError.text = string.Format("Name {0} is already taken...", error);
          //  m_inputField.text = m_NetworkPlayer.Player_Name;
          //  m_inputField.gameObject.SetActive(false);
          //  CoroutineUtilities.DelaySeconds(() => { m_inputError.gameObject.SetActive(false); }, 2.5f);
        }
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

    private void OnSwitchTeamClicked()
    {
        m_NetworkPlayer.CmdSwitchTeam();
    }

    private void OnShowNameInputClicked()
    {
        if(!m_nameInputfield.gameObject.activeSelf)
        {
            m_nameInputfield.gameObject.SetActive(true);
        }
        else
        {
            m_nameInputfield.gameObject.SetActive(false);
            m_nameInputfield.onEndEdit.Invoke(m_nameInputfield.text);
        }
    }

    private void OnEndEditChangeName(string text)
    {
        m_NetworkPlayer.CmdChangeName(text);
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

        if (m_NetworkPlayer.hasAuthority)
        {
            m_switchTeamButton.interactable = MatchSettings._instance.CanSwitchToTeam(m_NetworkPlayer.Player_Team == ETeams.CrazyPeople ? ETeams.FireFighters : ETeams.CrazyPeople);
        }
    }

    private void OnPlayerDataUpdated(NetworkPlayer _)
    {
        UpdateData();
    }
}

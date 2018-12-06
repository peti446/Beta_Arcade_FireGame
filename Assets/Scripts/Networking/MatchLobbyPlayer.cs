using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchLobbyPlayer : MonoBehaviour {

    //Variables of the UI elements to control the player setted in the unity inspector
    #region UI References (Set in unity)
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
    [SerializeField]
    private Text m_nameErrorMsgText;
    #endregion

    //The player to control this lobby player
    private NetworkPlayer m_NetworkPlayer;
    //This button is always, if the player is the local player on the mashine it will contain the ready button
    private Button m_readyButton;
    /// <summary>
    /// Returns the current team the player is on
    /// </summary>
    public ETeams Team { get { return m_NetworkPlayer.Player_Team; } }

    private void OnDestroy()
    {
        //Clear up the event
        if (m_NetworkPlayer != null)
        {
            m_NetworkPlayer.NetworkPlayerDataUpdated -= OnPlayerDataUpdated;
        }
        if(MainNetworkManager._instance != null)
        {
            MainNetworkManager._instance.NetworkPlayerAdded -= PlayerJoined;
            MainNetworkManager._instance.NetworkPlayerRemoved -= PlayerLeft;
        }
        if(MatchSettings._instance != null)
            MatchSettings._instance.TeamsChanged -= MatchTeamsChanged;
    }

    /// <summary>
    /// Init the match lobby object for the player
    /// </summary>
    /// <param name="player">The network player that belonges to the instance of this match lobby player</param>
    public void InitForPlayer(NetworkPlayer player)
    {
        //Wait, null player ? Whot?
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
        MatchSettings._instance.TeamsChanged += MatchTeamsChanged;

        //By default the player is not the local one
        m_readyText.gameObject.SetActive(false);
        m_switchTeamButton.gameObject.SetActive(false);
        m_ShowChangeNameButton.gameObject.SetActive(false);
        m_nameInputfield.gameObject.SetActive(false);
        m_nameErrorMsgText.gameObject.SetActive(false);
        m_switchTeamButton.onClick.RemoveAllListeners();
        m_ShowChangeNameButton.onClick.RemoveAllListeners();
        m_nameInputfield.onEndEdit.RemoveAllListeners();

        //Add the player to the UI
        if (MainMenuUIHandler._instance.MatchLobbyScritp)
        {
            MainMenuUIHandler._instance.MatchLobbyScritp.AddLobbyPlayer(this);
        }

        //If we have the autority of the player enable buttons to chage their players data
        if (m_NetworkPlayer.hasAuthority)
        {
            m_name.color = Color.blue;
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


    /// <summary>
    /// Displays an error message for the player about the username alreaby been taken
    /// </summary>
    /// <param name="name">The username that the player tried changing to</param>
    public void DisplayUsernameError(string name)
    {
        //Activate the error msg and make it wanish after a certain amount of time
        m_nameInputfield.gameObject.SetActive(false);
        m_nameErrorMsgText.gameObject.SetActive(true);
        m_nameErrorMsgText.text = string.Format("Name {0} is already taken...", name);
        CoroutineUtilities.DelaySeconds(() => { m_nameErrorMsgText.gameObject.SetActive(false); }, 2.5f);
    }

    /// <summary>
    /// Sets the ready button reference if it is the local player only
    /// </summary>
    /// <param name="readyB">The ready button from the UI</param>
    public void SetReadyButtonReference(Button readyB)
    {
        //We dont have authority
        if (!m_NetworkPlayer.hasAuthority)
        {
            return;
        }
        //Set the reference and update the UI status
        m_readyButton = readyB;
        m_readyButton.interactable = false;
        UpdateButtonState();
    }

    //Logic for the swithc team click
    private void OnSwitchTeamClicked()
    {
        m_NetworkPlayer.CmdSwitchTeam();
    }

    //Logic for the change name button
    private void OnShowNameInputClicked()
    {
        //If we press the button and the iput field is inactive active it, if it is active deactivate it and perform the name changing
        if(!m_nameInputfield.gameObject.activeSelf)
        {
            m_nameInputfield.gameObject.SetActive(true);
            m_nameErrorMsgText.gameObject.SetActive(false);
        }
        else
        {
            m_nameErrorMsgText.gameObject.SetActive(false);
            m_nameInputfield.gameObject.SetActive(false);
            m_nameInputfield.onEndEdit.Invoke(m_nameInputfield.text);
        }
    }

    private void OnEndEditChangeName(string text)
    {
        //Check if the name is in the correct format and execute na,e change
        if(text != string.Empty && text != m_name.text)
            m_NetworkPlayer.CmdChangeName(text);
    }

    private void UpdateData()
    {
        //Set all the new data to the respective ui elements
        m_name.text = m_NetworkPlayer.Player_Name;
        m_readyText.gameObject.SetActive(m_NetworkPlayer.Is_ready);
        m_waitingText.gameObject.SetActive(!m_NetworkPlayer.Is_ready);
        //Switch team panel if nececary
        MainMenuUIHandler._instance.MatchLobbyScritp.SwitchLobbyPlayerTeamPanel(this);
        //Update the button state
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (m_readyButton != null )
        {
            //Button is null when its no the local player so, once we know its not null we know that it is the local player
            m_readyButton.interactable = MainNetworkManager._instance.CanMatchStart;
            //Set the button click event and text to the correct state
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

        //As every instance of lobby player has these buttons we need to check if we have authority over ti
        if (m_NetworkPlayer.hasAuthority)
        {
            //Only allow the player to switch if there is space in the other team and we dident say we are ready
            m_switchTeamButton.interactable = MatchSettings._instance.CanSwitchToTeam(m_NetworkPlayer.Player_Team == ETeams.CrazyPeople ? ETeams.FireFighters : ETeams.CrazyPeople) && !m_NetworkPlayer.Is_ready;
            m_ShowChangeNameButton.interactable = !m_NetworkPlayer.Is_ready;
        }
    }

    #region Networking events handling
    //events to update data or buttons
    private void OnPlayerDataUpdated(NetworkPlayer _)
    {
        UpdateData();
    }

    private void PlayerJoined(NetworkPlayer p)
    {
        UpdateButtonState();
    }

    private void PlayerLeft(NetworkPlayer p)
    {
        UpdateButtonState();
    }

    private void MatchTeamsChanged()
    {
        UpdateButtonState();
    }
    #endregion
}

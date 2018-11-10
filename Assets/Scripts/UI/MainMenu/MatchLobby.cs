using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MatchLobby : MonoBehaviour {

    #region UI References (Set in unity)
    [SerializeField]
    private Text m_MatchTitle;
    [SerializeField]
    private RectTransform m_PayerList_Team1;
    [SerializeField]
    private RectTransform m_PayerList_Team2;
    [SerializeField]
    private Button m_readyButton;
    #endregion

    private void OnEnable()
    {
        //Ready button is controlled by the local player
        m_readyButton.interactable = false;
        //Remove all old players objects if there is any
        foreach (RectTransform c in m_PayerList_Team1)
        {
            Destroy(c.gameObject);
        }
        foreach (RectTransform c in m_PayerList_Team2)
        {
            Destroy(c.gameObject);
        }
        //Set the match title
        m_MatchTitle.text = MainNetworkManager._instance.matchName;

        //Add events from the network manager so we can handle exceptions correctly
        MainNetworkManager._instance.ClientErrorHappend += OnClientErrorHappened;
        MainNetworkManager._instance.ServerErrorHappend += OnServerErrorHappened;
        MainNetworkManager._instance.ClientDisconected += OnClientDisconected;
        MainNetworkManager._instance.ConnectionDroped += OnConnectionDropped;
        MainNetworkManager._instance.ServerAllPlayersGotReady += OnAllClientsGotReady;
    }

    private void OnDisable()
    {
        //Clean up references to events
        if (MainNetworkManager._instance != null)
        {
            MainNetworkManager._instance.ClientErrorHappend -= OnClientErrorHappened;
            MainNetworkManager._instance.ServerErrorHappend -= OnServerErrorHappened;
            MainNetworkManager._instance.ClientDisconected -= OnClientDisconected;
            MainNetworkManager._instance.ConnectionDroped -= OnConnectionDropped;
            MainNetworkManager._instance.ServerAllPlayersGotReady -= OnAllClientsGotReady;
        }
    }


    /// <summary>
    /// Adds a lobby object to the correct team frame and sets up UI references. After this call the lobby player is compleatly setted up
    /// </summary>
    /// <param name="player">The lobby object we want to add to the ui frame</param>
    public void AddLobbyPlayer(MatchLobbyPlayer player)
    {
        //Sets the reference to button and adds it to the correct team panel
        player.SetReadyButtonReference(m_readyButton);
        switch(player.Team)
        {
            case ETeams.CrazyPeople:
                player.transform.SetParent(m_PayerList_Team1, false);
                break;
            case ETeams.FireFighters:
                player.transform.SetParent(m_PayerList_Team2, false);
                break;
        }
    }

    /// <summary>
    /// Ajust the players lobby object to be on the right team panel in the UI
    /// </summary>
    /// <param name="player">The match lobby object to modify/check/adjust</param>
    public void SwitchLobbyPlayerTeamPanel(MatchLobbyPlayer player)
    {
        switch (player.Team)
        {
            case ETeams.CrazyPeople:
                //Check if the player is still in the wrong panel if so then ajust
                if(IsPlayerInTeamPanel(player, m_PayerList_Team2))
                {
                    player.transform.SetParent(m_PayerList_Team1, false);
                }
                break;
            case ETeams.FireFighters:
                //Check if the player is still in the wrong panel if so then ajust
                if (IsPlayerInTeamPanel(player, m_PayerList_Team1))
                {
                    player.transform.SetParent(m_PayerList_Team2, false);
                }
                break;
        }
    }

    /// <summary>
    /// Function wrapper for the leave button to bring you to the correct panel.
    /// </summary>
    public void OnLeaveClick()
    {
        //Show the lobby panel
        MainMenuUIHandler._instance.ShowPanel(eMainMenuScreens.Lobby);
    }

    //Chck if the players is in the given panel
    private bool IsPlayerInTeamPanel(MatchLobbyPlayer p, RectTransform panel)
    {
        //Check for every child to check if the match lobby player exists within
        foreach(RectTransform t in panel)
        {
            if(t.GetComponent<MatchLobbyPlayer>() == p)
            {
                //Found the player so return
                return true;
            }
        }
        //No player found
        return false;
    }

    //Fired on the server once all clients are ready
    private void OnAllClientsGotReady()
    {
        MainNetworkManager._instance.StartGame();
    }

    #region Networking events handling disconnection
    //Handle networking events. All types of events will just bring the user back to the lobby
    private void OnClientErrorHappened(NetworkConnection con, int errorCode)
    {
        MainMenuUIHandler._instance.ShowPanel(eMainMenuScreens.Lobby);
    }

    private void OnServerErrorHappened(NetworkConnection con, int errorCode)
    {
        MainMenuUIHandler._instance.ShowPanel(eMainMenuScreens.Lobby);
    }

    private void OnClientDisconected(NetworkConnection conn)
    {
        MainMenuUIHandler._instance.ShowPanel(eMainMenuScreens.Lobby);
    }

    private void OnConnectionDropped()
    {
        MainMenuUIHandler._instance.ShowPanel(eMainMenuScreens.Lobby);
    }
    #endregion
}

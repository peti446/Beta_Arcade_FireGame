using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MatchLobby : MonoBehaviour {

    [SerializeField]
    private RectTransform m_PayerList_Team1;
    [SerializeField]
    private RectTransform m_PayerList_Team2;
    [SerializeField]
    private Button m_readyButton;
    [SerializeField]
    private Button m_leaveButton;
    [SerializeField]
    private Button m_switchTeam;

    private void OnEnable()
    {
        m_readyButton.interactable = false;
        MainNetworkManager._instance.ClientErrorHappend += OnClientErrorHappened;
        MainNetworkManager._instance.ServerErrorHappend += OnServerErrorHappened;
        MainNetworkManager._instance.ClientDisconected += OnClientDisconected;
        MainNetworkManager._instance.ConnectionDroped += OnConnectionDropped;
    }

    private void OnDisable()
    {
        MainNetworkManager._instance.ClientErrorHappend -= OnClientErrorHappened;
        MainNetworkManager._instance.ServerErrorHappend -= OnServerErrorHappened;
        MainNetworkManager._instance.ClientDisconected -= OnClientDisconected;
        MainNetworkManager._instance.ConnectionDroped -= OnConnectionDropped;
    }

    public void AddLobbyPlayer(MatchLobbyPlayer player)
    {
        player.SetReadyButtonReference(m_readyButton);
        player.SetSwitchTeamButton(m_switchTeam);
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

    public void SwitchLobbyPlayerTeamPanel(MatchLobbyPlayer player)
    {
        switch (player.Team)
        {
            case ETeams.CrazyPeople:
                if(IsPlayerInTeamPanel(player, m_PayerList_Team2))
                {
                    player.transform.SetParent(m_PayerList_Team1, false);
                }
                break;
            case ETeams.FireFighters:
                if (IsPlayerInTeamPanel(player, m_PayerList_Team1))
                {
                    player.transform.SetParent(m_PayerList_Team2, false);
                }
                break;
        }
    }

    public void OnLeaveClick()
    {
        MainMenuUIHandler._instance.ShowPanel(eMainMenuScreens.Lobby);
    }

    private bool IsPlayerInTeamPanel(MatchLobbyPlayer p, RectTransform panel)
    {
        foreach(RectTransform t in panel)
        {
            if(t.GetComponent<MatchLobbyPlayer>() == p)
            {
                return true;
            }
        }
        return false;
    }

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
}

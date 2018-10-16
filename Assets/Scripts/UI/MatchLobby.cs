using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void OnEnable()
    {
        m_readyButton.interactable = false;
    }

    public void AddLobbyPlayer(MatchLobbyPlayer player)
    {
        player.SetReadyButtonReference(m_readyButton);
        if(m_PayerList_Team1.transform.childCount < 3)
        {
            player.transform.SetParent(m_PayerList_Team1, false);
            return;
        }
        player.transform.SetParent(m_PayerList_Team2, false);
    }
}

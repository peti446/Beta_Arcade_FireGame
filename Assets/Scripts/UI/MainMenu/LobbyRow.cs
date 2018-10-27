using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class LobbyRow : MonoBehaviour {

    [SerializeField]
    private Text m_matchName;
    [SerializeField]
    private Text m_matchMode;
    [SerializeField]
    private Text m_playerCount;
    [SerializeField]
    private Button m_joinButton;

    private ulong m_netID;

    /// <summary>
    /// Inits the current match lobby row .
    /// </summary>
    /// <param name="info">MatchInfoSnapshot provided by the server</param>
    /// <param name="is_odd">shoud this row use the odd colour sheme</param>
    public void SetMatchInfoData(MatchInfoSnapshot info, bool is_odd)
    {
        //Set the match name and player ocunt
        m_matchName.text = info.name;
        m_playerCount.text = string.Format("{0}/{1}", info.currentSize, info.maxSize);

        //Set the internal value to join the server later
        m_netID = (ulong)info.networkId;

        //If odd use different colour
        if(is_odd)
        {
            gameObject.GetComponent<Image>().color = new Color(255, 137, 137, 100);
        }

        //Set the listener reference for the join button
        m_joinButton.onClick.RemoveAllListeners();
        m_joinButton.onClick.AddListener(JoinMatch);

        //If the match is full, dont allow people to join
        if(info.currentSize >= info.maxSize)
        {
            m_joinButton.interactable = false;
        }
    }

    //Button on click function
    private void JoinMatch()
    {
        //Execute mentwork manager join function
        MainNetworkManager._instance.JoinUnityMatchmakingMatch((NetworkID)m_netID, (success, extraData, matchInfo) =>
        {
            //Did we join the match ?
            if(success)
            {
                //set the match name info and show the panel match lobby
                Debug.Log("Connected");
                MainNetworkManager._instance.matchName = m_matchName.text;
                MainMenuUIHandler._instance.ShowPanel(eMainMenuScreens.MatchLobby);
            }
            else
            {
                //could not join
                //TODO: Show some kind of error message
                Debug.Log("Not Connected");
            }
        }
        );
    }
}

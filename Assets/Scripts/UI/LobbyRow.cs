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

    public void SetMatchInfoData(MatchInfoSnapshot info, bool is_odd)
    {
        m_matchName.text = info.name;
        m_playerCount.text = string.Format("{0}/{1}", info.currentSize, info.maxSize);

        m_netID = (ulong)info.networkId;
        if(is_odd)
        {
            gameObject.GetComponent<Image>().color = new Color(255, 137, 137, 100);
        }

        m_joinButton.onClick.RemoveAllListeners();
        m_joinButton.onClick.AddListener(JoinMatch);

        if(info.currentSize >= info.maxSize)
        {
            m_joinButton.interactable = false;
        }
    }

    public void JoinMatch()
    {
        MainNetworkManager._instance.JoinUnityMatchmakingMatch((NetworkID)m_netID, (success, extraData, matchInfo) =>
        {
            if(success)
            {
                Debug.Log("Connected");
            }
            else
            {
                Debug.Log("Not Connected");
            }
        }
        );
    }
}

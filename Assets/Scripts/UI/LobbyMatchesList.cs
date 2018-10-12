using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class LobbyMatchesList : MonoBehaviour {

    [SerializeField]
    private RectTransform m_ServerListLivingObject;
    [SerializeField]
    private GameObject m_MatchPrefab;

    public void RefreshMachesList()
    {
        if(MainNetworkManager._instance != null)
        {
            MainNetworkManager._instance.matchMaker.ListMatches(0, 10, string.Empty, false, 0, 0, OnMatchListRecived);
        }
    }

    public void OnMatchListRecived(bool flag, string info, List<MatchInfoSnapshot> matchList)
    {
        if (matchList == null)
            return;
    }
}

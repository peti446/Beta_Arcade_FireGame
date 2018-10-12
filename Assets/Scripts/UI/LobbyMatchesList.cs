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
    [SerializeField]
    private Button m_nextButton;
    [SerializeField]
    private Button m_prevButton;

    private const int m_MatchesPerPage = 10;
    private int m_currentPage = 0;
    private int m_previusPage = 0; 

    public void RefreshMachesList()
    {
        if(MainNetworkManager._instance != null)
        {
            MainNetworkManager._instance.matchMaker.ListMatches(m_currentPage, m_MatchesPerPage, string.Empty, false, 0, 0, OnMatchListRecived);
        }
    }

    public void OnNextPress()
    {
        m_previusPage = m_currentPage;
        m_currentPage++;
        m_prevButton.interactable = false;
        m_nextButton.interactable = false;
        RefreshMachesList();
    }

    public void OnPreviusPress()
    {
        if(m_currentPage > 0)
        {
            m_previusPage = m_currentPage;
            m_currentPage--;
            m_nextButton.interactable = false;
            RefreshMachesList();
        }
        m_prevButton.interactable = false;
    }

    public void OnMatchListRecived(bool flag, string info, List<MatchInfoSnapshot> matchList)
    {
        if (matchList == null)
            return;

        foreach(Transform t in m_ServerListLivingObject)
        {
            Destroy(t.gameObject);
        }
        if (m_currentPage == 0)
            m_prevButton.interactable = false;
        if(matchList.Count == 0)
        {
            m_nextButton.interactable = false;
            if (m_currentPage == m_previusPage)
            {
                m_currentPage--;
                m_previusPage = m_currentPage;
            }
            else
                m_currentPage = m_previusPage;
            RefreshMachesList();
            return;
        }
        if(matchList.Count == m_MatchesPerPage)
            m_nextButton.interactable = true;

        for(int i = 0; i < matchList.Count; i++)
        {
            GameObject o = Instantiate(m_MatchPrefab, m_ServerListLivingObject);
            o.GetComponent<LobbyMatchRow>().SetMatchInfoData(matchList[i], (i % 2 != 0));
        }
    }
}

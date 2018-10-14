using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class Lobby : MonoBehaviour {

    [SerializeField]
    private RectTransform m_ServerListLivingObject;
    [SerializeField]
    private GameObject m_LobbyRowPrefab;
    [SerializeField]
    private Button m_nextButton;
    [SerializeField]
    private Button m_prevButton;
    [SerializeField]
    private Text m_lookingForText;


    private const int m_MatchesPerPage = 11;
    private int m_currentPage = 0;
    private int m_previusPage = 0;
    private int m_PageToProcess = 0;

    private void OnEnable()
    {
        m_prevButton.interactable = false;
        m_nextButton.interactable = false;
        MainNetworkManager._instance.
        StartCoroutine(CoroutineUtilities.DelaySeconds(() => { RefreshMachesList(); }, 2));
    }

    public void RefreshMachesList()
    {
        if(MainNetworkManager._instance != null)
        {
            m_lookingForText.text = "Looking for servers...";
            m_lookingForText.enabled = true;
            MainNetworkManager._instance.matchMaker.ListMatches(m_PageToProcess, m_MatchesPerPage, string.Empty, false, 0, 0, OnMatchListRecived);
        }
    }

    public void OnNextPress()
    {
        m_PageToProcess++;
        m_prevButton.interactable = false;
        m_nextButton.interactable = false;
        RefreshMachesList();
    }

    public void OnPreviusPress()
    {
        m_PageToProcess = Mathf.Max(0, m_PageToProcess--);
        m_nextButton.interactable = false;
        m_prevButton.interactable = false;
        RefreshMachesList();
    }

    public void OnMatchListRecived(bool flag, string info, List<MatchInfoSnapshot> matchList)
    {
        if (matchList == null)
            return;

        m_lookingForText.text = "No server found...";
        m_lookingForText.enabled = true;
        m_prevButton.interactable = false;
        m_nextButton.interactable = false;
        m_previusPage = m_currentPage;
        m_currentPage = m_PageToProcess;

        foreach (Transform t in m_ServerListLivingObject)
        {
            Destroy(t.gameObject);
        }

        if(matchList.Count == 0)
        {
            if (m_currentPage <= 0)
            {
                m_currentPage = 0;
                m_previusPage = 0;
                m_PageToProcess = 0;
                return;
            }

            m_currentPage = m_previusPage;
            RefreshMachesList();
            return;
        }

        m_lookingForText.enabled = false;
        m_nextButton.interactable = matchList.Count == m_MatchesPerPage;

        for(int i = 0; i < matchList.Count; i++)
        {
            GameObject o = Instantiate(m_LobbyRowPrefab, m_ServerListLivingObject);
            o.GetComponent<LobbyRow>().SetMatchInfoData(matchList[i], (i % 2 != 0));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class Lobby : MonoBehaviour {

    //Unity UI references
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

    //Variables to control the page been displayed and how many entries will get displayed
    private const int m_MatchesPerPage = 11;
    private int m_currentPage = 0;
    private int m_previusPage = 0;
    private int m_PageToProcess = 0;

    private void OnEnable()
    { 
        //By degfualt make the next and previus not interactuable
        m_prevButton.interactable = false;
        m_nextButton.interactable = false;
        //Destroy all previous server entries game objects
        foreach (Transform t in m_ServerListLivingObject)
        {
            Destroy(t.gameObject);
        }
        //Start to search for a server after a delay
        StartCoroutine(CoroutineUtilities.DelaySeconds(() => { RefreshMachesList(); }, .2f));
    }

    /// <summary>
    /// Refresh the current server list showing the matches on the current page we are on
    /// </summary>
    public void RefreshMachesList()
    {
        //Check if the network manager is in the correct state
        if(MainNetworkManager._instance != null && MainNetworkManager._instance.State == ENetworkState.InLobby)
        {
            //Activate info text
            m_lookingForText.text = "Looking for servers...";
            m_lookingForText.gameObject.SetActive(true);
            //Request match info
            MainNetworkManager._instance.matchMaker.ListMatches(m_PageToProcess, m_MatchesPerPage, string.Empty, false, 0, 0, OnMatchListRecived);
        } else
        {
            //Error ocurred display it to the user
            m_lookingForText.text = "Error occured trying to find servers, either you are in a different status or there is no network isntance...";
            m_lookingForText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Increases the current page and refreshes the list.
    /// </summary>
    public void OnNextPress()
    {
        m_PageToProcess++;
        m_prevButton.interactable = false;
        m_nextButton.interactable = false;
        RefreshMachesList();
    }

    /// <summary>
    /// Decreases the current page if possible and refreshes the list.
    /// </summary>
    public void OnPreviusPress()
    {
        m_PageToProcess = Mathf.Max(0, m_PageToProcess--);
        m_nextButton.interactable = false;
        m_prevButton.interactable = false;
        RefreshMachesList();
    }

    //New set of matches has been recived so update the ui
    private void OnMatchListRecived(bool flag, string info, List<MatchInfoSnapshot> matchList)
    {
        //If the match list is null there has been a problem
        if (matchList == null)
            return;


        //Update the UI text elements as by default if no servers are in the list we just return
        m_lookingForText.text = "No server found...";
        m_lookingForText.gameObject.SetActive(true);
        m_prevButton.interactable = false;
        m_nextButton.interactable = false;
        //Set the page variable to be the one that we just processed
        m_previusPage = m_currentPage;
        m_currentPage = m_PageToProcess;

        //Destroy any other match ui element from the screen
        foreach (Transform t in m_ServerListLivingObject)
        {
            Destroy(t.gameObject);
        }

        //Check if there are not any matches
        if(matchList.Count == 0)
        {
            //IF we somhow got to the page -1 or less reset everything to 0
            if (m_currentPage <= 0)
            {
                m_currentPage = 0;
                m_previusPage = 0;
                m_PageToProcess = 0;
                return;
            }

            //set the current page to the previus and refresh the list as the current page does not contain any more data
            m_currentPage = m_previusPage;
            RefreshMachesList();
            return;
        }

        //Deactivate the server not found text
        m_lookingForText.gameObject.SetActive(false);
        //next page can only be used if we are at the maximum entries
        m_nextButton.interactable = matchList.Count == m_MatchesPerPage;

        //Display the matches entries
        for(int i = 0; i < matchList.Count; i++)
        {
            GameObject o = Instantiate(m_LobbyRowPrefab, m_ServerListLivingObject);
            o.GetComponent<LobbyRow>().SetMatchInfoData(matchList[i], (i % 2 != 0));
        }
    }
}

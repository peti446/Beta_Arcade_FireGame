using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eMainMenuScreens
{
    LoadingScreen, MainMenu, MatchCreation, Lobby, MatchLobby
}

public class MainMenuUIHandler : MonoBehaviour {

    //All UI references
    [SerializeField]
    private Canvas m_MatchListLobby;
    [SerializeField]
    private Canvas m_MatchCreation;
    [SerializeField]
    private Canvas m_MainMenu;
    [SerializeField]
    private Canvas m_MatchLobby;

    //Variables to control where the user is at in the UI
    private Canvas m_currentActiveCanvas;
    private eMainMenuScreens m_currentActiveScreen;
    //Action to perform once the network manager got disconected
    private Action m_DisconectedTask;

    /// <summary>
    /// MatchLobby script quick acces
    /// </summary>
    public MatchLobby MatchLobbyScritp
    {
        get
        {
            return m_MatchLobby.GetComponent<MatchLobby>();
        }
    }

    #region Instance handling
    public static MainMenuUIHandler _instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        //Check if we already got an instance, if so delete the object
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        //Create a new instance of the instance
        _instance = this;
    }

    private void OnDestroy()
    {
        //Make sure we clear the instance static variable to be able to recreate it later
        if (_instance == this)
            _instance = null;

        //Tidy up event references
        if(MainNetworkManager._instance != null)
        MainNetworkManager._instance.ClientShutdown -= OnClientStopped;
    }
    #endregion

    private void OnEnable()
    {
        //Make sure eveything is disabled
        foreach(Transform t in gameObject.transform)
        {
            t.gameObject.SetActive(false);
        }
        //Show the main menu
        ShowPanel(m_MainMenu);

        //Add event to the network manager
        MainNetworkManager._instance.ClientShutdown += OnClientStopped;
    }

    /// <summary>
    /// Shows a specific UI panel and performs all types of disconection/connection if needed
    /// </summary>
    /// <param name="screenToShow">The ui we want to switch</param>
    public void ShowPanel(eMainMenuScreens screenToShow)
    {
        //TODO: Check current screen to further improve the way we disconect from the server
        switch(screenToShow)
        {
            case eMainMenuScreens.LoadingScreen:
                break;
            case eMainMenuScreens.MainMenu:
                //We want to go to tje main menu so make sure everyhing is disconected before we switch
                if (MainNetworkManager._instance.isNetworkActive || MainNetworkManager._instance.matchMaker != null)
                {
                    m_DisconectedTask = () => {
                        ShowPanel(m_MainMenu);
                    };
                    MainNetworkManager._instance.Disconect();
                }
                else
                {
                    ShowPanel(m_MainMenu);
                }
                break;
            case eMainMenuScreens.MatchCreation:
                //Before we show the match creation need to make sure the network manager is on
                if(!MainNetworkManager._instance.isNetworkActive || MainNetworkManager._instance.matchMaker != null)
                {
                    MainNetworkManager._instance.StartUnityMatchmaking();
                }
                ShowPanel(m_MatchCreation);
                break;
            case eMainMenuScreens.Lobby:
                //If we want to show the lobby first disconect everything as we dont know ecactly the state we are in then start the manager again.
                if(MainNetworkManager._instance.isNetworkActive || MainNetworkManager._instance.matchMaker != null)
                {
                    m_DisconectedTask = () => {
                        MainNetworkManager._instance.StartUnityMatchmaking();
                        ShowPanel(m_MatchListLobby);
                    };
                    MainNetworkManager._instance.Disconect();
                }
                else
                {
                    MainNetworkManager._instance.StartUnityMatchmaking();
                    ShowPanel(m_MatchListLobby);
                }
                break;
            case eMainMenuScreens.MatchLobby:
                //Show only the match lobby if the network is active, if not how did we even get here ?
                if (MainNetworkManager._instance.isNetworkActive)
                {
                    ShowPanel(m_MatchLobby);
                }
                else
                {
                    Debug.LogError("Error cannot enter match lobby without and active network connection");
                }
                break;
        }
    }

    //Buttons wrappers to show specific uis 
    public void ShowMatchListUI()
    {
        ShowPanel(eMainMenuScreens.Lobby);
    }

    public void ShowCreateMatchUI()
    {
        ShowPanel(eMainMenuScreens.MatchCreation);
    }
    
    public void ShowMainMenu()
    {
        ShowPanel(eMainMenuScreens.MainMenu);
    }

    private void ShowPanel(Canvas canvas)
    {
        //Hides the last canvas we were showing
        if(m_currentActiveCanvas != null)
            m_currentActiveCanvas.gameObject.SetActive(false);

        //Set the active canvas to the one given and shows it
        m_currentActiveCanvas = canvas;
        if (canvas != null)
            canvas.gameObject.SetActive(true);
    }

    private void OnClientStopped()
    {
        //IF there is a disconected task execute it as the network manager just stopped
        if(m_DisconectedTask != null)
        {
            m_DisconectedTask.Invoke();
            m_DisconectedTask = null;
        }
    }
}

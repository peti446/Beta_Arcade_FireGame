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

    private Canvas m_currentActiveCanvas;
    private eMainMenuScreens m_currentActiveScreen;
    private Action m_DisconectedTask;

    public MatchLobby MatchLobbyScritp
    {
        get
        {
            return m_MatchLobby.GetComponent<MatchLobby>();
        }
    }

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
    }

    private void OnEnable()
    {
        //Make sure eveything is disabled
        foreach(Transform t in gameObject.transform)
        {
            t.gameObject.SetActive(false);
        }
        ShowPanel(m_MainMenu);

        MainNetworkManager._instance.ClientShutdown += OnClientStopped;
    }

    public void ShowPanel(eMainMenuScreens screenToShow)
    {
        switch(screenToShow)
        {
            case eMainMenuScreens.LoadingScreen:
                break;
            case eMainMenuScreens.MainMenu:
                if (MainNetworkManager._instance.isNetworkActive)
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
                if(!MainNetworkManager._instance.isNetworkActive)
                {
                    MainNetworkManager._instance.StartUnityMatchmaking();
                }
                ShowPanel(m_MatchCreation);
                break;
            case eMainMenuScreens.Lobby:
                if(MainNetworkManager._instance.isNetworkActive)
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
        if(m_currentActiveCanvas != null)
            m_currentActiveCanvas.gameObject.SetActive(false);

        m_currentActiveCanvas = canvas;
        if (canvas != null)
            canvas.gameObject.SetActive(true);
    }

    private void OnClientStopped()
    {
        if(m_DisconectedTask != null)
        {
            m_DisconectedTask.Invoke();
            m_DisconectedTask = null;
        }
    }
}

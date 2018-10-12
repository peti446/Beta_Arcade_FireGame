using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUIHandler : MonoBehaviour {

    //All UI references
    [SerializeField]
    private Canvas m_MatchListLobby;
    [SerializeField]
    private Canvas m_MatchCreation;

    private Canvas m_currentActiveCanvas;

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
        //Debuging for now
        ShowMatchListUI();
    }


    private void ShowPanel(Canvas canvas)
    {
        if(m_currentActiveCanvas != null)
            m_currentActiveCanvas.gameObject.SetActive(false);

        m_currentActiveCanvas = canvas;
        if (canvas != null)
            canvas.gameObject.SetActive(true);
    }

    public void ShowCreateMachUI()
    {
        if (!MainNetworkManager._instance.isNetworkActive)
        {
            MainNetworkManager._instance.Disconect();
        }
        ShowPanel(m_MatchCreation);
    }

    public void ShowMatchListUI()
    {
        if (!MainNetworkManager._instance.isNetworkActive)
        {
            MainNetworkManager._instance.StartUnityMatchmaking();
        } else
        {
            MainNetworkManager._instance.Disconect();
        }
        ShowPanel(m_MatchListLobby);
    }
}

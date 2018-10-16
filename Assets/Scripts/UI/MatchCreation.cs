using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchCreation : MonoBehaviour {

    [SerializeField]
    private InputField m_inputField;

    
    public void OnCreatePress()
    {
        if (m_inputField.text == string.Empty)
        {
            return;
        }
        MainNetworkManager._instance.CreateUnityMatchmakingMatch(m_inputField.text, (succes, extraInfo, matchInfo) =>
        {
            if(succes)
            {
                Debug.Log("Game created");
                MainMenuUIHandler._instance.ShowMatchLobby();
            } else
            {
                Debug.Log("Error generating game");
            }
        });
    }

    public void OnCancelPress()
    {

    }
}

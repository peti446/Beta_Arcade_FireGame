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
        LoadingScreen ls = LoadingScreen._instance;
        if (ls != null)
        {
            ls.Show();
        }
        MainNetworkManager._instance.CreateUnityMatchmakingMatch(m_inputField.text, (succes, extraInfo, matchInfo) =>
        {
            if (ls != null)
            {
                ls.Hide();
            }
            if (succes)
            {
                Debug.Log("Game created");
                MainMenuUIHandler._instance.ShowPanel(eMainMenuScreens.MatchLobby);
            } else
            {
                Debug.Log("Error generating game");
                MainMenuUIHandler._instance.ShowPanel(eMainMenuScreens.MainMenu);
            }
        });
    }

    public void OnCancelPress()
    {
        MainMenuUIHandler._instance.ShowPanel(eMainMenuScreens.MainMenu);
    }
}

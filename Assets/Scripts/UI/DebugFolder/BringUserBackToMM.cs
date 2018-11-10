using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class BringUserBackToMM : MonoBehaviour {

    private void Start()
    {
        if (MainNetworkManager._instance != null && !MainNetworkManager.Is_Server)
        {
            MainNetworkManager._instance.Disconect();
        }
    }

    public void OnClick()
    {
        MainNetworkManager._instance.Disconect();
        SceneManager.LoadScene(0);
    }
}

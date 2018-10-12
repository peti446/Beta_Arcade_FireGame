using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUIHandler : MonoBehaviour {


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
}

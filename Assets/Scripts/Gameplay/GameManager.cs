using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EGameState  
{
  InGame, InMenu, LoadingGame, InLobby, OnPauseScreen
};



public class GameManager : MonoBehaviour
{

  public static GameManager instance = null;

  private void Awake()
  {
    if (instance == null)
      instance = this;

    else if (instance != this)
      Destroy(gameObject);

    DontDestroyOnLoad(this);

    //initialise - game start loading calling 
  }


}

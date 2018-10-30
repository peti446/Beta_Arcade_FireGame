using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EGameState  
{
  InGame, InMainMenu, LoadingGame, InLobby, OnPauseScreen
};

public class GameManager : MonoBehaviour
{

  public static GameManager instance = null;
    /// <summary>
    /// Keeps the GameManager object alive throughout the whole game
    /// and prevents it from being duplicated
    /// </summary>
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

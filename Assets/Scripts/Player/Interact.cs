/*
   .
   |\      ,--.
 . \ `\   /  _ \__
 |\_\  `-'   "  |
 |   \          |
 \    `.        |
  \     `--    /
   \          /
    `,-.____/
  _ / |
   |  |
     /| 
     kony pyony was here
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CharacterInteractEvent : UnityEvent<Character> { }

public class Interact : MonoBehaviour
{
   public CharacterInteractEvent ClientInteraction;
  public CharacterInteractEvent ServerInteraction;


  public void ClientInteract(Character character)
  {
    if (MainNetworkManager._instance.LocalPlayer.Player_ID == character.ControllingPlayerID && ClientInteraction != null)
    {
      ClientInteraction.Invoke(character);
    }
  }

  public void ServerInteract(Character character)
  {
    if (MainNetworkManager.Is_Server && ServerInteraction != null)
    {
      ServerInteraction.Invoke(character);
    }
  }
}

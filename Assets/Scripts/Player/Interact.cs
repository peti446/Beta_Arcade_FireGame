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

public class Interact : MonoBehaviour
{
  public UnityEvent<Character> ClientInteraction;
  public UnityEvent<Character> ServerInteraction;


  public void ClientInteract(Character character)
  {
    if (MainNetworkManager._instance.LocalPlayer.ID == character.ControllingPlayerID && ClientInteraction != null)
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

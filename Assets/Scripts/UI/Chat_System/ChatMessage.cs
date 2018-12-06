﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ChatMessage : MonoBehaviour {

    //Just to make sure very chat message has the same formatting
    private const string ChatTextFormat = "{0} : {1}";
    //Variables from the message recived
    private string m_message;
    private NetworkPlayer m_sender;
    //The actual text to display the message
    private Text m_Text;

    private void Awake()
    {
        m_Text = GetComponent<Text>();
    }

    private void OnDestroy()
    {
        //Clean up referecnes to the player
        if(m_sender != null)
            m_sender.PlayerNameChanged -= OnPlayerChangedName;
    }

    /// <summary>
    /// Sets the message info and updates its ui
    /// </summary>
    /// <param name="playerID">The player id who sended the message</param>
    /// <param name="message">The message in question</param>
    public void InitMessage(int playerID, string message)
    {
        //Find the network player instance
        foreach(NetworkPlayer p in MainNetworkManager._instance.PlayersConnected)
        {
            if (p.Player_ID == playerID)
            {
                m_sender = p;
                break;
            }
        }

        //Sets the message
        m_message = message;

        //Format the text
        string name = m_sender.isLocalPlayer ? "<color=#0000ff>" + m_sender.Player_Name + "</color>" : m_sender.Player_Name;
        m_Text.text = string.Format(ChatTextFormat, name, m_message);

        m_sender.PlayerNameChanged += OnPlayerChangedName;
    }

    private void OnPlayerChangedName(NetworkPlayer _)
    {
        //Format the text
        string name = m_sender.isLocalPlayer ? "<color=#ffffff>" + m_sender.Player_Name + "</color>" : m_sender.Player_Name;
        m_Text.text = string.Format(ChatTextFormat, name, m_message);
    }
}

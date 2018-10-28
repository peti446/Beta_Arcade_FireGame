using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChatMessagePacket : MessageBase
{
    public int SenderID;
    public string Message;
    public bool ServerReplication = false;

    public ChatMessagePacket(int id, string msg)
    {
        SenderID = id;
        Message = msg;
    }

    public ChatMessagePacket()
    {
        SenderID = -1;
        Message = "";
    }
}
[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(NetworkIdentity))]
public class ChatManager : NetworkBehaviour {
    //Reference to the chat message game object
    [SerializeField]
    private GameObject m_messagePrefab;
    //Max backlog of the chat, we dont want to keep too many messages as they will slow down the game.
    [SerializeField]
    [Range(1, 1000)]
    private uint m_maxChatHistoryLenght = 1000;
    //The rect view to add the messages
    private RectTransform m_ViewContent;
    //Store all messages that are been displayed to later destroy them
    private LinkedList<ChatMessage> m_messagesList = new LinkedList<ChatMessage>();
    //To know if we are ready or not
    private bool m_isChatSetup = false;

    private void Awake()
    {
        //Get the view
        m_ViewContent = GetComponent<ScrollRect>().content;
        //Hook to player added
        if(MainNetworkManager._instance != null)
        {
            MainNetworkManager._instance.ClientConnected += OnClientStart;
        }
    }

    private void OnClientStart(NetworkConnection c)
    {
        if(MainNetworkManager._instance.client.isConnected)
        {
            InitChat();
            MainNetworkManager._instance.ClientConnected -= OnClientStart;
        }
    }

    private void OnDestroy()
    {
        DeInitChat();
    }
    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        DeInitChat();
    }

    /// <summary>
    /// Sends a chat message to the server
    /// </summary>
    /// <param name="msg">The message</param>
    public void SendChatMessage(string msg)
    {
        //If the chat system is not ready wtf?
        if (!m_isChatSetup)
            return;

        //Creates a new Chat message and sends it to the server
        ChatMessagePacket s = new ChatMessagePacket(MainNetworkManager._instance.LocalPlayer.ID, msg.Replace("\n", "").Replace("\r", ""));
        MainNetworkManager._instance.client.Send(CustomNetMessages.ChatNetMesage, s);
    }

    /// <summary>
    /// Inits the chat system and prepares it to recive messages
    /// </summary>
    public void InitChat()
    {
        Debug.Log("Init chat");
        //We are already ready why do you want to set it up again bro?
        if (m_isChatSetup)
            return;
        //Register Message listeners
        MainNetworkManager._instance.client.RegisterHandler(CustomNetMessages.ChatNetMesage, OnReceivedChatMessage);

        //if this is the server
        if (MainNetworkManager.Is_Server)
        {
            //register server message listener
            NetworkServer.RegisterHandler(CustomNetMessages.ChatNetMesage, OnReceivedChatMessage);
        }
        //Set the flag and enable the object
        m_isChatSetup = true;
    }

    private void DeInitChat()
    {
        //If we are not set why deiniting it ?
        if (!m_isChatSetup)
            return;

        if (MainNetworkManager.Is_Server)
            NetworkServer.UnregisterHandler(CustomNetMessages.ChatNetMesage);
        else
            if(MainNetworkManager._instance != null)
                MainNetworkManager._instance.client.UnregisterHandler(CustomNetMessages.ChatNetMesage);

        m_isChatSetup = false;
    }

    private void OnReceivedChatMessage(NetworkMessage message)
    {
        //Read the chat messages and add it to the chat frame
        ChatMessagePacket chatMessagePacket = message.ReadMessage<ChatMessagePacket>();

        //If there isent any message dont do anything
        if (chatMessagePacket == null || chatMessagePacket.SenderID == -1)
            return;

        //If the server is reciving the message replciate it over to the rest of the clients
        if(MainNetworkManager.Is_Server && !chatMessagePacket.ServerReplication)
        {
            chatMessagePacket.ServerReplication = true;
            NetworkServer.SendToAll(CustomNetMessages.ChatNetMesage, chatMessagePacket);
        }
        else
        {
            //Is the client so add the message to the ui
            AddMessageToChat(chatMessagePacket);
        }
    }

    private void AddMessageToChat(ChatMessagePacket msgPacket)
    {
        //Creates a new message and adds it to the list
        GameObject newMessageObject = Instantiate(m_messagePrefab, m_ViewContent);
        ChatMessage newCM = newMessageObject.GetComponent<ChatMessage>();
        if (newCM == null)
        {
            Debug.LogError("Chat message prefab does not have a ChatMessage component, cannot add the recived message: " + msgPacket.Message);
            Destroy(newMessageObject);
            return;
        }
        //Init the message
        newCM.InitMessage(msgPacket.SenderID, msgPacket.Message);
        m_messagesList.AddLast(newCM);

        //If the chat history is full delete the last one
        if (m_messagesList.Count > m_maxChatHistoryLenght)
        {
            //Get the first message in the list, then remove it form the list and remvoe it from the ui
            LinkedListNode<ChatMessage> firstMessageNode = m_messagesList.First;
            ChatMessage cm = firstMessageNode.Value;
            m_messagesList.Remove(firstMessageNode);
            Destroy(cm.gameObject);
        }

        //Autoscroll
        Canvas.ForceUpdateCanvases();
        StartCoroutine(CoroutineUtilities.DoOnNextFrame(() => { GetComponent<ScrollRect>().verticalScrollbar.value = 0.0f; }));
    }
}

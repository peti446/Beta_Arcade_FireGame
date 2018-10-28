using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public struct ChatMessagePacket
{
    public int SenderID;
    public string Message;

    public ChatMessagePacket(int id, string msg)
    {
        SenderID = id;
        Message = msg;
    }
}

public class ChatMessagePacketBase : MessageBase
{
    public ChatMessagePacket entry;

    public ChatMessagePacketBase(ChatMessagePacket entry)
    {
        this.entry = entry;
    }

    public ChatMessagePacketBase()
    {
        entry = new ChatMessagePacket();
    }

    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(entry.Message);
        writer.Write(entry.SenderID);
    }

    public override void Deserialize(NetworkReader reader)
    {
        entry.Message = reader.ReadString();
        entry.SenderID = reader.ReadInt32();
    }
}

[RequireComponent(typeof(ScrollRect))]
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

    private void Awake()
    {
        //Get the view
        m_ViewContent = GetComponent<ScrollRect>().content;
    }

    private void OnEnable()
    {
       NetworkServer.RegisterHandler(100, ReceivedMessage);
    }

    private void OnDisable()
    {
        NetworkServer.UnregisterHandler(100);
    }

    /// <summary>
    /// Sends a chat message to the server
    /// </summary>
    /// <param name="msg">The message</param>
    public void SendChatMessage(string msg)
    {
        //Creates a new Chat message and sends it to the server
        ChatMessagePacket s = new ChatMessagePacket(MainNetworkManager._instance.LocalPlayer.ID, msg.Replace("\n", "").Replace("\r", ""));
        MainNetworkManager._instance.client.Send(100, new ChatMessagePacketBase(s));
    }

    private void ReceivedMessage(NetworkMessage message)
    {
        //Read the chat messages and add it to the chat frame
        ChatMessagePacketBase chatMessageBase = message.ReadMessage<ChatMessagePacketBase>();
        AddMessageToChat(chatMessageBase.entry);
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
    }
}

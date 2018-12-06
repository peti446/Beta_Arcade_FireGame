using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class ChatInputManager : MonoBehaviour {

    //Chat manager
    [SerializeField]
    private ChatManager m_cmanager;
    //The input field
    private InputField m_inputField;

    private void Awake()
    {
        m_inputField = GetComponent<InputField>();
		m_inputField.onValueChanged.RemoveAllListeners();
		m_inputField.onValueChanged.AddListener(delegate { OnValueChanged(); });
    }

    public void OnValueChanged()
    {
        if(m_inputField.text.Contains("\n"))
        {
            m_cmanager.SendChatMessage(m_inputField.text.Replace("\n", string.Empty).Replace("\r", string.Empty));
            m_inputField.text = string.Empty;
            m_inputField.ActivateInputField();
        }
    }
}

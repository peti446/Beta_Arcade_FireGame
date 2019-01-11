using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameDialog : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_title;
	[SerializeField]
	private TextMeshProUGUI m_body;

	public void SetDialogContent(string title, string body)
	{
		m_title.text = title;
		m_body.text = body;
	}
}

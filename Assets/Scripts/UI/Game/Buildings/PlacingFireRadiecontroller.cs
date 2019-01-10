using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlacingFireRadiecontroller : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;
	[SerializeField]
	private Image m_fillerImage;
	[SerializeField]
	private Image m_backgroundFiller;

	public void SetFiller(int secondsLeft, float porcent)
	{
		m_backgroundFiller.gameObject.SetActive(true);
		m_fillerImage.fillAmount = porcent;
		m_text.text = string.Format("Building will be on fire in {0} sec", secondsLeft);
		if(porcent >= 1.0f)
		{
			m_backgroundFiller.gameObject.SetActive(false);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameProgressBars : MonoBehaviour
{
	[SerializeField]
	private Image m_burnedProgress;

	private float m_maxSize;

	public void Start()
	{
		m_maxSize = m_burnedProgress.GetComponent<RectTransform>().sizeDelta.x;
		m_burnedProgress.GetComponent<RectTransform>().sizeDelta = new Vector2(0, m_burnedProgress.GetComponent<RectTransform>().sizeDelta.y);
	}

	/// <summary>
	/// Sets the porcent of the bars
	/// </summary>
	/// <param name="porcent">The porcent from 0-1</param>
	public void SetBurnedCityPorcent(float porcent)
	{
		m_burnedProgress.GetComponent<RectTransform>().sizeDelta = new Vector2(m_maxSize * porcent, m_burnedProgress.GetComponent<RectTransform>().sizeDelta.y);
	}
}

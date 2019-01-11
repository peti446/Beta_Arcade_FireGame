using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionHandler : MonoBehaviour
{
	[SerializeField]
	public Button m_left;
	[SerializeField]
	public Button m_right;
	[SerializeField]
	private Image m_background;
	[SerializeField]
	private MapInfo[] m_validMaps;
	[SerializeField]
	private TextMeshProUGUI m_text;

	private int m_currentMap = 0;

	private void Awake()
	{
		m_left.onClick.AddListener(LeftClicked);
		m_right.onClick.AddListener(RightClicked);
	}

	public void OnEnable()
	{
		MatchSettings._instance.MapInfoChanged += MapInfoChanged;
		MapInfoChanged();
	}

	public void OnDisable()
	{
		MatchSettings._instance.MapInfoChanged -= MapInfoChanged;
	}

	//Update the map info
	private void MapInfoChanged()
	{
		foreach(MapInfo info in m_validMaps)
		{
			if(info.name == MatchSettings._instance.MapID)
			{
				m_background.sprite = info.Mapsprite;
				m_text.text = info.Name;
			}
		}
	}

	private void LeftClicked()
	{
		//Check if we can change map
		if (m_currentMap == 0)
		{
			m_left.gameObject.SetActive(false);
			return;
		}

		//Increase it by one
		m_currentMap--;
		MatchSettings._instance.SetMap(m_validMaps[m_currentMap].Name);
		m_background.sprite = m_validMaps[m_currentMap].Mapsprite;
		m_text.text = m_validMaps[m_currentMap].Name;
		m_right.gameObject.SetActive(true);
	}

	private void RightClicked()
	{
		//Check if we can change map
		if(m_currentMap >= (m_validMaps.Length-1))
		{
			m_right.gameObject.SetActive(false);
			return;
		}

		//Increase it by one
		m_currentMap++;
		MatchSettings._instance.SetMap(m_validMaps[m_currentMap].Name);
		m_background.sprite = m_validMaps[m_currentMap].Mapsprite;
		m_text.text = m_validMaps[m_currentMap].Name;
		m_left.gameObject.SetActive(true);
	}

}

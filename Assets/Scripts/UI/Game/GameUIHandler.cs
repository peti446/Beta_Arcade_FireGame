using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIHandler : MonoBehaviour
{
	//UI variables
	[SerializeField]
	private GameObject m_NotificationObjectPrefab;
	[SerializeField]
	private RectTransform m_resourceCanvas;
	[SerializeField]
	private GameProgressBars m_progressBars;
	[SerializeField]
	private TextMeshProUGUI m_gameTime;
	[SerializeField]
	private VerticalLayoutGroup m_notificationLayout;
	[SerializeField]
	private TextMeshProUGUI m_InteractNotification;

	//Variable to control the update of this elements
	private bool isSetup = false;

	//Vehicle object reference if it is set update based on the info from it
	private Vehicle m_fireTruckRef;

	public static GameUIHandler _instance
	{
		get;
		private set;
	}
	public void Awake()
	{
		//Instance handling and creation
		if(_instance != null)
		{
			Destroy(gameObject);
			return;
		}

		_instance = this;

		//Disable all elements
		m_resourceCanvas.gameObject.SetActive(false);
		m_progressBars.gameObject.SetActive(false);
		m_gameTime.gameObject.SetActive(false);
		m_notificationLayout.gameObject.SetActive(false);
		m_InteractNotification.gameObject.SetActive(false);
	}

	public void OnDestroy()
	{
		if (_instance == this)
			_instance = null;
	}


	public void Update()
	{
		//Dont update if we are not setup
		if (!isSetup)
			return;

		//Update common ui variables
		UpdateTime(GameManager._instance.GameSecondsLeft);

		//Update character specific info
		if(m_resourceCanvas.gameObject.activeSelf)
		{
			if (m_fireTruckRef != null)
			{
				m_resourceCanvas.GetChild(0).gameObject.GetComponent<Slider>().value = 0;
			}
			else
			{
				m_resourceCanvas.GetChild(0).gameObject.GetComponent<Slider>().value = 0;
			}
		}
	}

	/// <summary>
	/// Sets up the ui to be used in character mode, it will always use the local characters
	/// </summary>
	public void SetUpUIForCharacter()
	{
		//Active the elements responsable for this character
		if (MainNetworkManager._instance.LocalPlayer.Player_Team == ETeams.FireFighters)
		{
			m_resourceCanvas.gameObject.SetActive(true);
		}
		m_progressBars.gameObject.SetActive(true);
		m_gameTime.gameObject.SetActive(true);
		m_notificationLayout.gameObject.SetActive(true);
		m_InteractNotification.gameObject.SetActive(false);

		m_fireTruckRef = null;
		isSetup = true;
	}

	/// <summary>
	/// Will set up the ui to display the vheicle info
	/// </summary>
	/// <param name="vheicleInfo"> The vehivle that should be used to update the ui</param>
	public void SetUpUIForVehicle(Vehicle vheicleInfo)
	{
		m_fireTruckRef = vheicleInfo;
	}

	/// <summary>
	/// Adds a notification to the area
	/// </summary>
	/// <param name="text">The text of the n otification</param>
	/// <param name="textColor">thhe colour</param>
	/// <param name="timeToRemove">Time that the notificaion will live</param>
	/// <param name="speed">speed that the notification will fade away</param>
	public void DisplayNotification(string text, Color color,  float timeUntilDestroy, NotificationObject.FadeSpeed speed = NotificationObject.FadeSpeed.Normal)
	{
		GameObject newNotification = GameObject.Instantiate(m_NotificationObjectPrefab, m_notificationLayout.transform);
		NotificationObject script = newNotification.GetComponent<NotificationObject>();
		if(script)
		{
			script.DisplayNotification(text, color, timeUntilDestroy, speed);
		}
		else
		{
			Destroy(newNotification);
		}
	}

	/// <summary>
	/// Shows the interaction warning to the user
	/// </summary>
	/// <param name="show">Hide(<c>false</c>) or show(<c>true</c>) the warning</param>
	public void ShowInteractWarning(bool show)
	{
		m_InteractNotification.gameObject.SetActive(show);
	}

	/// <summary>
	/// Updates the game time in the UI.
	/// </summary>
	/// <param name="secondsPassed">Time that is left for the game to finish</param>
	private void UpdateTime(float secondsPassed)
	{
		TimeSpan time = TimeSpan.FromSeconds(secondsPassed);
		m_gameTime.text = string.Format("{0:D2}:{1:D2}", time.Hours, time.Seconds);
	}
}

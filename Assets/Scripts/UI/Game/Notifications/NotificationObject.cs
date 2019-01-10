using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationObject : MonoBehaviour
{
	public enum FadeSpeed
	{
		Slow, Normal, Fast, NoFading
	}

	//The text that will display the notification
	[SerializeField]
	private TextMeshProUGUI m_text;

	//Variables to make the text fade away
	private float m_timeToRemove;
	private float m_timeStartedDisplay;
	private FadeSpeed m_fadeSpeed;
	private bool m_settetUp = false;

	/// <summary>
	/// Sets up the notification and displays it
	/// </summary>
	/// <param name="text">The text of the n otification</param>
	/// <param name="textColor">thhe colour</param>
	/// <param name="timeToRemove">Time that the notificaion will live</param>
	/// <param name="speed">speed that the notification will fade away</param>
	public void DisplayNotification(string text, Color textColor, float timeToRemove, FadeSpeed speed = FadeSpeed.Normal)
	{
		//Set the time
		m_timeToRemove = timeToRemove;
		m_timeStartedDisplay = Time.time;

		//Set the text variables
		m_text.text = text;
		m_text.color = textColor;
		m_fadeSpeed = speed;
		m_text.gameObject.SetActive(true);
		m_settetUp = true;
	}

	private void Update()
	{
		if(m_settetUp && Time.time - m_timeStartedDisplay > m_timeToRemove)
		{
			//Fade away the text
			if(m_fadeSpeed == FadeSpeed.NoFading)
			{
				//No fading means destoy the object
				Destroy(gameObject);
			}
			else
			{
				//Start the corroutine to fade away
				StartCoroutine(StartTransition());
				m_settetUp = false;
			}
		}
	}

	private IEnumerator StartTransition()
	{
		//Get the fading time based on the given speed
		float timeToFade = 3.5f;
		switch(m_fadeSpeed)
		{
			case FadeSpeed.Fast:
				timeToFade = 1;
				break;
			case FadeSpeed.Normal:
				timeToFade = 2.5f;
				break;
		}
		//Variables to control fading speed
		float startFadeTime = Time.time;
		float passedTime = startFadeTime - Time.time;
		//Fade away
		while(passedTime < timeToFade)
		{
			//Update alpha so we fade away
			Color currentColor = m_text.color;
			currentColor.a = 1 - (passedTime / timeToFade);
			m_text.color = currentColor;
			//Update the time
			passedTime = startFadeTime - Time.time;
			yield return null;
		}
		//Destroy the notification
		Destroy(gameObject);
		yield return null;
	}
}

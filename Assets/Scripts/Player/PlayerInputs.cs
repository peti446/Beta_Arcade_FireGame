using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class PlayerInputs : MonoBehaviour
{
	private Character m_character;
	// Use this for initialization
	private void Awake()
	{
		m_character = GetComponent<Character>();
	}

	// Update is called once per frame
	void Update()
	{

		//Only exeute if moving
		if(m_character.State == EPlayerStatus.Idle || m_character.State == EPlayerStatus.Moving)
			m_character.SetInputs(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

		//Onlcy move camera if pressing the buggon
		if(Input.GetMouseButton(0))
			m_character.RotatePlayer(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

		if (Input.GetKeyDown(KeyCode.F))
		{
			m_character.ToggleHose(true);
      m_character.m_animator.SetBool("using_hose", true);
      Debug.Log("F");
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
			m_character.InteractRay();
		}

		if (Input.GetKeyUp(KeyCode.E))
		{
			m_character.StopInteraction();
		}

		if (Input.GetKeyUp(KeyCode.F))
		{
			m_character.ToggleHose(false);
      m_character.m_animator.SetBool("using_hose", false);

    }
  }
}

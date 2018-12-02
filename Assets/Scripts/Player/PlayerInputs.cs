using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class PlayerInputs : MonoBehaviour
{
  private Character m_character;
  private Rigidbody playerRigibody;
  private float m_horizontalInput;
  private float m_verticalInput;
  private float m_mouseInputX;
  private float m_mouseInputY;
  // Use this for initialization
  private void Awake()
  {
    m_character = GetComponent<Character>();
    playerRigibody = GetComponent<Rigidbody>();
  }

  // Update is called once per frame
  void Update()
  {
    m_horizontalInput = Input.GetAxis("Horizontal");
    m_verticalInput = Input.GetAxis("Vertical");
    m_mouseInputX = Input.GetAxis("Mouse X");
    m_mouseInputY = Input.GetAxis("Mouse Y");




    if (Input.GetKeyDown(KeyCode.F))
    {
      m_character.ToggleHose(true);
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
    }



  }

  private void FixedUpdate()
  {
    m_character.SetInputs(m_horizontalInput, m_verticalInput);

    m_character.RotatePlayer(m_mouseInputX, m_mouseInputY);
  }

}

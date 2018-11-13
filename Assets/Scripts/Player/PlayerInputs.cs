using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class PlayerInputs : MonoBehaviour
{
  
  //[SerializeField]
  //float camSensY = 0.0f; //How sensitive it with mouse
  //[SerializeField]
  //float camSensX = 0.5f; //How sensitive it with mouse

  //private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)

  [SerializeField]
  private Character m_character;

  private Rigidbody playerRigibody;

  private float horizontalInput;
  private float verticalInput;


  // Use this for initialization
  private void Awake()
  {
    m_character = GetComponent<Character>();
    playerRigibody = GetComponent<Rigidbody>();
  }
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

    horizontalInput = Input.GetAxis("Horizontal");
    verticalInput = Input.GetAxis("Vertical");


    
    if (Input.GetKeyDown(KeyCode.E))
    {
      m_character.ToggleHose(true);
    }

    if (Input.GetKey(KeyCode.E))
    {
      m_character.InteractRay();
    }

    if (Input.GetKeyUp(KeyCode.E))
    {
      m_character.ToggleHose(false);
    }


  }

  private void FixedUpdate()
  {

    //m_character.MovePlayer(verticalInput, horizontalInput);


  //  if (verticalInput != 0 || horizontalInput != 0)
  //  {
  //    playerRigibody.velocity = (gameObject.transform.forward * verticalInput * playerSpeed) + (gameObject.transform.right * horizontalInput * playerSpeed);

  //  }

  //  lastMouse = Input.mousePosition - lastMouse;
  //  lastMouse = new Vector3(-lastMouse.y * camSensY, lastMouse.x * camSensX, 0);
  //  lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
  //  transform.eulerAngles = lastMouse;
  //  lastMouse = Input.mousePosition;

  //}

}
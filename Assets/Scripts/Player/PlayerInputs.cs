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

  private Character m_character;
    private Rigidbody playerRigibody;
  // Use this for initialization
  private void Awake()
  {
    m_character = GetComponent<Character>();
    playerRigibody = GetComponent<Rigidbody>();
  }

    // Update is called once per frame
    void Update()
    {



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

        m_character.SetInputs(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
    }

    private void FixedUpdate()
    {
            //  }

            //  lastMouse = Input.mousePosition - lastMouse;
            //  lastMouse = new Vector3(-lastMouse.y * camSensY, lastMouse.x * camSensX, 0);
            //  lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
            //  transform.eulerAngles = lastMouse;
            //  lastMouse = Input.mousePosition;

            //}
    }

}

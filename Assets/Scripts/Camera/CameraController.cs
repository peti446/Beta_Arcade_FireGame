using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    
    [SerializeField]
    float camSensY = 0.25f; //How sensitive it with mouse
    [SerializeField]
    float camSensX = 0.0f; //How sensitive it with mouse

    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)


    void Update()
    {


        if (Input.GetKey(KeyCode.LeftShift))
        {
            camSensX = 0.5f;
           

        }
        else
        {
            camSensX = 0.0f;
            // transform.eulerAngles = new Vector3(0, 0, 0);

            // transform.rotation = Quaternion.Euler(0, 0, 0); // this is 90 degrees around y axis
            //Debug.Log(transform.rotation.y);
            //if(transform.rotation.y > 0)
            //{
            //    transform.Rotate(Vector3.right * 10, Time.deltaTime, Space.World);
            //}

        }
        lastMouse = Input.mousePosition - lastMouse;
        lastMouse = new Vector3(-lastMouse.y * camSensY, lastMouse.x * camSensX, 0);
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
        transform.eulerAngles = lastMouse;
        lastMouse = Input.mousePosition;
        




    }



}

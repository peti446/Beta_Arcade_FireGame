using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleInputs : MonoBehaviour {

    

    [SerializeField]
    private float vehicleMaxSpeed = 20.0f;
    [SerializeField]
    private float vehicleAcceleration = 5.0f;

    [SerializeField]
    private float maxVehicleTurn = 2.0f;
    [SerializeField]
    private float vehicleBrake = 3.0f;
    [SerializeField]
    private float vehicleReverseMaxSpeed = 5.0f;
    [SerializeField]
    private float vehicleReverseAcceleration = 0.1f;

    //Don't change this values on editor, just showed on editor for debug information
    [SerializeField]
    private float vehicleSpeed = 0.0f;
    [SerializeField]
    private float vehicleTurn = 0.0f;

    //private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)

    private Vehicle m_vehicle;
    private Rigidbody vehicleRigibody;

    private float horizontalInput;
    private float verticalInput;



    private void Awake()
    {
        m_vehicle = GetComponent<Vehicle>();
        vehicleRigibody = GetComponent<Rigidbody>();
    }
    // Use this for initialization
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");


    }

    private void FixedUpdate()
    {
        //Vehicle Acceleration
        if (verticalInput > 0)
        {
            vehicleSpeed += vehicleAcceleration;
            vehicleSpeed = Mathf.Clamp(vehicleSpeed, 0, vehicleMaxSpeed);

           
        //Vehicle break and reverse move
        }else if (verticalInput < 0)
        {
            if (vehicleSpeed > 0)
            {
                vehicleSpeed -= vehicleBrake;
                vehicleSpeed = Mathf.Clamp(vehicleSpeed, 0, vehicleMaxSpeed);
            }
            else
            {                
                vehicleSpeed -= vehicleReverseAcceleration;
                vehicleSpeed = Mathf.Clamp(vehicleSpeed, vehicleReverseMaxSpeed * -1, 0);
            }
        //Put vehicle speed slowly to 0 if you don't press any button
        }else
        {
            if(vehicleSpeed > 0)
            {
                vehicleSpeed -= vehicleAcceleration;
                vehicleSpeed = Mathf.Clamp(vehicleSpeed, 0, vehicleMaxSpeed);
            }
            else
            {
                vehicleSpeed += vehicleReverseAcceleration;
                vehicleSpeed = Mathf.Clamp(vehicleSpeed, vehicleReverseMaxSpeed * -1, 0);
            }

        }

        //Vehicle turn
        if (horizontalInput != 0)
        {
            vehicleTurn = vehicleSpeed * 0.1f;
            vehicleTurn = Mathf.Clamp(vehicleTurn, maxVehicleTurn * -1, maxVehicleTurn);
            transform.Rotate(0, horizontalInput * vehicleTurn, 0);

        }

        vehicleRigibody.velocity = gameObject.transform.forward * vehicleSpeed;
    }



    //protected void LateUpdate()
    //{
    //    transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    //    if (verticalInput == 0)
    //    {
    //        vehicleRigibody.velocity = new Vector3(vehicleRigibody.velocity.x * 0.80f, vehicleRigibody.velocity.y * 0.80f, vehicleRigibody.velocity.z * 0.80f);
    //    }
    //}
}

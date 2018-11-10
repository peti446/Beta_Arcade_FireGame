using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EVehicleStatus
{
    IDLE, AcceleratingFoward, AcceleratingBack, DesceleratingFoward, DesceleratingBack
}

[RequireComponent(typeof(Rigidbody))]

public class Vehicle : MonoBehaviour {

    private Rigidbody vehicleRigibody;
    //private Vector3 Dir;
    //[SerializeField]
    //private float m_MaxVelocity;
    //[SerializeField]
    //private float m_CurrentVelocity;
    //[SerializeField]
    //private float m_Acceleration;
    //[SerializeField]
    //private float m_maxBackAccel;


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




    public EVehicleStatus State
    {
        get;
        private set;
    }

    private void Awake()
    {
        vehicleRigibody = GetComponent<Rigidbody>();
    }
    // Use this for initialization
    void Start () {
		//vehicleVelocity = vehicleRigibody.velocity.
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate()
    {

        UpdateVehicleVelocity();
        //Add gravity
        //vehicleRigibody.velocity += Physics.gravity;
    }

    //private void UpdateVehicleVelocity()
    //{
    //    if(Dir.y > 0)
    //    {
    //        m_CurrentVelocity = Mathf.Clamp(m_CurrentVelocity + m_Acceleration, 0, m_MaxVelocity);
    //    }
    //    else if(Dir.y < 0)
    //    {
    //        m_CurrentVelocity = Mathf.Clamp(m_CurrentVelocity - m_Acceleration, m_maxBackAccel * -1, m_MaxVelocity);
    //    }

    private void UpdateVehicleVelocity()
    {
        switch(State)
        {
            case EVehicleStatus.AcceleratingFoward:
                vehicleSpeed += vehicleAcceleration;
                vehicleSpeed = Mathf.Clamp(vehicleSpeed, 0, vehicleMaxSpeed);
                break;
            default:
                break;
        }
    }



    //    vehicleRigibody.velocity = gameObject.transform.forward * m_CurrentVelocity;
    //}

    //public void SetDirection(Vector3 dirToMove)
    //{
    //    //Check if we are actually moving
    //    if (dirToMove.sqrMagnitude < 1.0f)
    //    {
    //        State = EVehicleStatus.IDLE;
    //        Dir = new Vector3(0, 0, 0);
    //        return;
    //    }
    //    Dir = dirToMove.normalized;
    //    State = EVehicleStatus.AcceleratingFoward;
    //}

    public void SetInputs(float horizontalInput, float verticalInput)
    {
        //Vehicle Acceleration
        if (verticalInput > 0)
        {
            State = EVehicleStatus.AcceleratingBack;

            vehicleSpeed += vehicleAcceleration;
            vehicleSpeed = Mathf.Clamp(vehicleSpeed, 0, vehicleMaxSpeed);



            //Vehicle break and reverse move
        }
        else if (verticalInput < 0)
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
        }
        else
        {
            if (vehicleSpeed > 0)
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
        //vehicleRigibody.velocity += Physics.gravity;
    }
   
}

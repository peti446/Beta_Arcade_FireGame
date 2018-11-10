using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EVehicleStatus
{
    IDLE, Accelerating, Cooldown
}

[RequireComponent(typeof(Rigidbody))]
public class Vehicle : MonoBehaviour {

    private Rigidbody vehicleRigibody;
    private Vector3 Dir;
    private float m_MaxVelocity;
    private float CurrentVelocity;
    private float m_Acceleration;
    private float m_maxBackAccel;

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
        switch(State)
        {
            case EVehicleStatus.Accelerating:
                UpdateVehicleVelocity();
                break;
            default:
                break;
        }

        //Add gravity
        vehicleRigibody.velocity += Physics.gravity;
    }

    private void UpdateVehicleVelocity()
    {
        if(Dir.y > 0)
        {
            CurrentVelocity = Mathf.Clamp(CurrentVelocity+m_Acceleration, 0, m_MaxVelocity);
        }
        else if(Dir.y < 0)
        {
            CurrentVelocity = Mathf.Clamp(CurrentVelocity - m_Acceleration, m_maxBackAccel * -1, m_MaxVelocity);
        }



        //vehicleRigibody.velocity = gameObject.transform.forward * vehicleSpeed;
    }

    public void SetDirection(Vector3 dirToMove)
    {
        //Check if we are actually moving
        if (dirToMove.sqrMagnitude < 1.0f)
        {
            State = EVehicleStatus.IDLE;
            Dir = new Vector3(0, 0, 0);
            return;
        }
        Dir = dirToMove.normalized;
        State = EVehicleStatus.Accelerating;
    }
}

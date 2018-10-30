using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {

    private Rigidbody vehicleRigibody;

    [SerializeField]
    private float vehicleSpeed;

    [SerializeField]
    private float vehicleTurn;

    private float vehicleVelocity;

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

    public void MoveVehicle(float m_verticalInput, float m_horizontalInput)
    {
        vehicleRigibody.AddRelativeForce(0, 0, m_verticalInput * vehicleSpeed, ForceMode.Force);
        //playerRigibody.AddRelativeForce(horizontalInput * playerSpeed, 0, 0,  ForceMode.Force);
        transform.Rotate(0, vehicleTurn * m_horizontalInput, 0);
    }
}

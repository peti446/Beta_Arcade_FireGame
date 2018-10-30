using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleInputs : MonoBehaviour {

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

        m_vehicle.MoveVehicle(verticalInput, horizontalInput);
    }

    protected void LateUpdate()
    {
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        if (verticalInput == 0)
        {
            vehicleRigibody.velocity = new Vector3(vehicleRigibody.velocity.x * 0.80f, vehicleRigibody.velocity.y * 0.80f, vehicleRigibody.velocity.z * 0.80f);
        }
    }
}

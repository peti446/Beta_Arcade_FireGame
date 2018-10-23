using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleInputs : MonoBehaviour {

    [SerializeField]
    private float speed;
    [SerializeField]
    private float carTurn;

    private Rigidbody vehicleRigibody;

    private float horizontalMovement;
    private float verticalMovement;


    // Use this for initialization
    void Start()
    {
        vehicleRigibody = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");


    }

    private void FixedUpdate()
    {
        //vehicleRigibody.velocity = new Vector3(vehicleRigibody.velocity.x, vehicleRigibody.velocity.y, verticalMovement * speed);
        //vehicleRigibody.AddForce(horizontalMovement * speed,0,verticalMovement*speed, ForceMode.Force);
        vehicleRigibody.AddRelativeForce(0, 0, verticalMovement * speed, ForceMode.Force);

        if (horizontalMovement != 0 && vehicleRigibody.velocity.z != 0)
            transform.Rotate(0, carTurn * horizontalMovement, 0);

        Debug.Log(vehicleRigibody.velocity.z);
        //transform.Rotate(Vector3.right * Time.deltaTime);

    }

    protected void LateUpdate()
    {
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour {

    [SerializeField]
    private float playerSpeed;

    [SerializeField]
    private float playerTurn;


    private Rigidbody playerRigibody;

    private float horizontalInput;
    private float verticalInput;
    // Use this for initialization
    void Start()
    {
        playerRigibody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        if (verticalInput == 0)
        {
            playerRigibody.velocity = new Vector3(playerRigibody.velocity.x * 0.80f, playerRigibody.velocity.y * 0.80f, playerRigibody.velocity.z * 0.80f);
        }
    }

    private void FixedUpdate()
    {
        playerRigibody.AddRelativeForce(0, 0, verticalInput * playerSpeed, ForceMode.Force);
        //playerRigibody.AddRelativeForce(horizontalInput * playerSpeed, 0, 0,  ForceMode.Force);
        transform.Rotate(0, playerTurn * horizontalInput, 0);




    }

    protected void LateUpdate()
    {
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class PlayerInputs : MonoBehaviour {
       
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



    }

    private void FixedUpdate()
    {
        //playerRigibody.AddRelativeForce(0, 0, verticalInput * playerSpeed, ForceMode.Force);
        ////playerRigibody.AddRelativeForce(horizontalInput * playerSpeed, 0, 0,  ForceMode.Force);
        //transform.Rotate(0, playerTurn * horizontalInput, 0);

        m_character.MovePlayer(verticalInput, horizontalInput);


    }

    protected void LateUpdate()
    {
        //TODO call the character function instead of doing it directly here
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        if (verticalInput == 0)
        {
            playerRigibody.velocity = new Vector3(playerRigibody.velocity.x * 0.80f, playerRigibody.velocity.y * 0.80f, playerRigibody.velocity.z * 0.80f);
        }
    }
}
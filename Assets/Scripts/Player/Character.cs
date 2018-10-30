using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    private Rigidbody playerRigibody;

    [SerializeField]
    private float playerSpeed;
    [SerializeField]
    private float playerTurn;


    private void Awake()
    {
          playerRigibody = GetComponent<Rigidbody>();
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void MovePlayer(float verticalInput, float horizontalInput)
    {
        if (verticalInput == 0 && horizontalInput == 0)
            return;
        //playerRigibody.AddRelativeForce(0, 0, verticalInput * playerSpeed, ForceMode.Force);
        //playerRigibody.AddRelativeForce(horizontalInput * playerSpeed, 0, 0,  ForceMode.Force);
        //transform.Rotate(0, playerTurn * horizontalInput, 0);
        if (verticalInput == 1)
            playerRigibody.velocity = gameObject.transform.forward * playerSpeed;
        else if (verticalInput == 0)
            playerRigibody.velocity = new Vector3(0, 0, 0);
        else
            playerRigibody.velocity = gameObject.transform.forward * -playerSpeed;
    }


    /// <summary>
    /// Sets the direction of the characters
    /// </summary>
    /// <param name="newDir"><c>Vector3</c> The new direciton of this characters</param>
    public void SetDir(Vector3 newDir)
    {
        

    }
}

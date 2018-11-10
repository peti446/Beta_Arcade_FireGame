using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

  private Rigidbody playerRigibody;

  [SerializeField]
  private float playerSpeed;
  [SerializeField]
  private float playerTurn;

  private GameObject hose;


  private void Awake()
  {
    playerRigibody = GetComponent<Rigidbody>();
    //TODO kony: change this to properly find the particle object 
    hose = transform.Find("HoseParticles").gameObject;
  }
  // Use this for initialization

  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void MovePlayer(float verticalInput, float horizontalInput)
  {
    if (verticalInput == 0 && horizontalInput == 0)
      return;
    if (verticalInput == 1)
      playerRigibody.velocity = gameObject.transform.forward * playerSpeed;
    else if (verticalInput == 0)
      playerRigibody.velocity = new Vector3(0, 0, 0);
    else
      playerRigibody.velocity = gameObject.transform.forward * -playerSpeed;
  }


  public RaycastHit hit;
  public void ToggleHose(bool open)
  {
    if (open)
    {
      hose.GetComponent<ParticleSystem>().enableEmission = true;
      hose.GetComponent<ParticleSystem>().Play();
    }
    else if (!open)
    {
      hose.GetComponent<ParticleSystem>().enableEmission = false;
    }
  }

  public void InteractRay()
  {
    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 10, Color.red);
    Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10);
    if (hit.collider != null)
    {
      switch (hit.collider.tag)
      {
        case "Building":

          if (hit.collider.GetComponent<BuildingStatus>())
            hit.collider.GetComponent<BuildingStatus>().Extinguish();
          else
            Debug.Log("hose raycast hit against object with tag building but without building script");
          break;

        case "Burnling":

          break;

        default:
          //even if u want to do nothing with the default, better to specify it
          break;
      }
    }
  }


  /// <summary>
  /// Sets the direction of the characters
  /// </summary>
  /// <param name="newDir"><c>Vector3</c> The new direciton of this characters</param>
  public void SetDir(Vector3 newDir)
  {


  }

}

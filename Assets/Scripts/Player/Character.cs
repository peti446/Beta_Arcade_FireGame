using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum EPlayerStatus
{
    IDLE, Moving, Casting, Other
}

public class Character : NetworkBehaviour
{

    private Rigidbody playerRigibody;

    [SerializeField]
    private float playerSpeed;
    [SerializeField]
    float camSensY = 0.0f; //How sensitive it with mouse
    [SerializeField]
    float camSensX = 0.5f; //How sensitive it with mouse

    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)

    public EPlayerStatus State
    {
        get;
        private set;
    }
    private GameObject hose;


    //Variables for the current character status
    [SyncVar(hook = "OnPlayerIdChanged")]
    private int m_controllingPlayerID = -1;
    private bool m_isSetup = false;

    private void Awake()
    {
        playerRigibody = GetComponent<Rigidbody>();
        //TODO kony: change this to properly find the particle object 
        hose = transform.Find("HoseParticles").gameObject;
    }


    //Start function when object spawned, called on all client.
    public override void OnStartClient()
    {
        base.OnStartClient();
        //Init the player
        InitPlayer();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        InitPlayer();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        //Get the camera when we get authority
        if (!hasAuthority)
            return;
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        camera.transform.SetParent(transform, false);
        gameObject.GetComponent<PlayerInputs>().enabled = true;
    }

    //Only init the players on the client
    private void InitPlayer()
    {
        //Dont init the player again
        if (m_isSetup || m_controllingPlayerID == -1)
            return;

        //Set camera and enable input
        if (hasAuthority)
        {
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            camera.transform.SetParent(transform, false);
            gameObject.GetComponent<PlayerInputs>().enabled = true;
        }

        //Set the position to a valid spawn position
        transform.position = SpawnManager._instance.GetSpawnPoint(MainNetworkManager._instance.PlayersConnected[m_controllingPlayerID].Player_Team).position;

        //Set the character script to be settet up
        m_isSetup = true;
    }

    //PlayerID changed so init the player and set the instance ID player id to the player
    private void OnPlayerIdChanged(int newID)
    {
        m_controllingPlayerID = newID;
        InitPlayer();
    }

    /// <summary>
    /// Can only be executed on server, sets the controlling player id for this character.
    /// </summary>
    /// <param name="id">The player id that will be controling this character.</param>
    public void SetPlayerID(int id)
    {
        if(m_controllingPlayerID == -1)
        {
            m_controllingPlayerID = id;
        }
    }

  //public void MovePlayer(float verticalInput, float horizontalInput)
  //{
  //  if (verticalInput == 0 && horizontalInput == 0)
  //    return;
  //  if (verticalInput == 1)
  //    playerRigibody.velocity = gameObject.transform.forward * playerSpeed;
  //  else if (verticalInput == 0)
  //    playerRigibody.velocity = new Vector3(0, 0, 0);
  //  else
  //    playerRigibody.velocity = gameObject.transform.forward * -playerSpeed;
  //}


        

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

  [Client]
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
        case "Firetruck":
           CmdGetUpThefireTruck(hit.transform.gameObject);
           break;
        default:
          //even if u want to do nothing with the default, better to specify it
          break;
      }
    }
  }

    [Command]
    private void CmdGetUpThefireTruck(GameObject fireTruck)
    {
        Debug.Log("Trying to acces vechicle");
        if (fireTruck.GetComponent<NetworkIdentity>().clientAuthorityOwner == null)
        {
            fireTruck.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
            RpcGetUpToVehicle(fireTruck);
        }

    }

    [ClientRpc]
    private void RpcGetUpToVehicle(GameObject firetruck)
    {
        GetComponent<PlayerInputs>().enabled = false;
        firetruck.GetComponent<Vehicle>().GainControl(this);
    }

    public void SetInputs(float horizontalInput, float verticalInput)
    {
        playerRigibody.velocity = new Vector3(horizontalInput * 20, 0, verticalInput * 20);
        playerRigibody.velocity += (Physics.gravity);

    }

}

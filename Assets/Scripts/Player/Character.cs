/*
 
                                      _                             
                             _.      .' `-.                          
                           .'  `-.::/_.    `.                        
                          /   _.   .'        \                       
                        .' .'\  `./      `.   \                      
                     .-'  /   '.    .'.    \   ;                     
                  .-'  .-'      \ .'   \    .  |                     
               .-'   .'      .-._'_.-.  \    . :                     
              /     /     .-'    '    `. '.     \         :          
             :     :     /              `. '.    `.      /|          
              \    |    /                /`. '-.   `-._.' ;          
               ;   |  .;                :   \   '-.      /           
       `.     /    :.'/      _       _   \   \     \  _.'            
        \`._.'    // :    .s$$P     T$$s. '.  \     ; \              
         `._.   .':  |  .dP'           `Tb. \  \    |  `.            
            /  /  |  | dP  .-.       .-.  Tb ;  .   |    `-.         
        `_.'  :   |  |'   'd$b       d$b`   `|` |   |     | `.       
         '.   |   |  :   ':$$$       $$$:`   '\ |   '    /|   \      
           `-'|   |  :` ; |T$P       T$P| : '  :|  /   .' ;    ;     
              :   :   \\`-:__.       .__:-'/ .' |.'\_.'  /     |     
              /\   \   .\        s        / :   /|      /      :     
             .  '.  \  | \     .___.     /   \ : |    .'      /      
           .'     \  'X   '.           .'\    '.\|\.-'      .'       
         .'        \   '.  |`.       .'|  :      `.'.    .-'|        
        /           '    '-:  `-._.-'; |   `.      \ '-.'   ;        
       :             '      \       /  |--.._J.-.    '.  './    -.   
        \             \      \     :         / . `-.  \   '.     \`. 
         `.      .     ;      ;             /   `-. `-.;    \     : .
           `-._.'      |      |          __/          /|     \._.'  '
            .' .       ;      :     _.-"'.'         .: |      ;    / 
           /   |\     /      /         .' .-'    .-'  \|      |   /  
          /    : '._.'     .'.       .'  /    .-'     .'      : .'   
         :      \        .'-. `.   .' .-'  .-'       /       /"'     
     .:' |      |`-.__.-'`-. \_ `s'.-'  .-'         /      .'        
   .'/   :      :           `._.' `._.-'           :    .-'          
 .' :     \      \             `._.'               |   (             
.   :     /`.     `-.           ;|:                :    \            
:    `._.'   `-._    `-.       , : .               |\    '.          
 `.               `.     `.     ;'. ;               ' `.    `-.__.   
   `-._   _.'      ;\      `.   | : |              /    `*+.__.-'    
   .'  `"'        /  ;       \  |.' |             '                  
  /             .'   |        . : ` ;--._     _.-'                   
 /   :      _.-'     :        |  \ /                                 
:    |\             /         ;   V                                  
|    `.`.        _.'         /                                       
:      \ `-.._.-'          .'                                        

kony was here
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum EPlayerStatus
{
  Idle, Moving, Casting, Stunned, Alive, Dead
}

public class Character : NetworkBehaviour
{

  private Rigidbody playerRigibody;

  [SerializeField]
  private float m_playerSpeed = 10;
  [SerializeField]
  float m_PlayerRotationX = 5.0f; //How sensitive is x with mouse
  [SerializeField]
  float m_PlayerRotationY = 1.2f; //How sensitive is x with mouse

  //private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)

  public EPlayerStatus State
  {
    get;
    private set;
  }
  private GameObject hose;


  //Variables for the current character status
  [SyncVar(hook = "OnPlayerIdChanged")]
  private int m_controllingPlayerID = -1;


  public int ControllingPlayerID
  {
    get
    {
      return m_controllingPlayerID;
    }
  }
  private bool m_isSetup = false;
  private bool m_spawned = false;

  private void Awake()
  {
    playerRigibody = GetComponent<Rigidbody>();
    //TODO kony: change this to properly find the particle object 
    //hose = transform.Find("HoseParticles").gameObject;
  }


  //Start function when object spawned, called on all client.
  public override void OnStartClient()
  {
    base.OnStartClient();
    if (!m_isSetup && m_controllingPlayerID != -1)
    {
      //Init the player
      InitPlayer();
    }
  }

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
    camera.transform.SetParent(transform, false);
    camera.transform.localPosition = new Vector3(0, 0, 0);
    gameObject.GetComponent<PlayerInputs>().enabled = true;
    CmdSpawn();
  }

  //Init the player if it has valid values
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
      camera.transform.localPosition = new Vector3(0, 0, 0);
      gameObject.GetComponent<PlayerInputs>().enabled = true;

      //Ask server to spawn us
      CmdSpawn();
    }

    //Set the character script to be settet up
    m_isSetup = true;
  }

  /// <summary>
  /// Asks the server to give a valid spawn location for this character
  /// </summary>
  [Command]
  private void CmdSpawn()
  {
    //Pass it back to the game manager to spawn this object
    GameManager._instance.SpawnPlayer(this);
  }

  /// <summary>
  /// Spawns the player at the correct position.
  /// </summary>
  /// <param name="target">The target player connection. Needs to be the player with authority</param>
  /// <param name="pos">The pos we want the player to spawn on</param>
  [TargetRpc]
  public void TargetSpawnPlayerAt(NetworkConnection target, Vector3 pos)
  {
    //Just in case make sure we do have authority
    if (!hasAuthority)
      return;

    //Set the pos and that we spawned
    transform.position = pos;
    m_spawned = true;
  }

  //PlayerID changed so init the player and set the instance ID player id to the player
  private void OnPlayerIdChanged(int newID)
  {
    m_controllingPlayerID = newID;
    Debug.Log("ID: " + m_controllingPlayerID);
    InitPlayer();
  }

  /// <summary>
  /// Can only be executed on server, sets the controlling player id for this character.
  /// </summary>
  /// <param name="id">The player id that will be controling this character.</param>
  [Server]
  public void SetPlayerID(int id)
  {
    m_controllingPlayerID = id;
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

  [Client]
  public void InteractRay()
  {
    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 10, Color.red);
    Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10);
    if (hit.collider != null)
    {
      if (hit.collider.GetComponent<Interact>())
      {
        CmdInteractServer();
        hit.collider.GetComponent<Interact>().ClientInteract(this);
      }
    }
  }

  [Command]
  private void CmdInteractBuilding(GameObject building)
  {
    BuildingStatus bs = building.GetComponent<BuildingStatus>();
    if (bs.IsAblaze)
    {
      bs.Extinguish();
    }
    else
    {
        BuildingStatus bs = building.GetComponent<BuildingStatus>();
        if (bs.IsAblaze)
        {
            bs.Extinguish();
        }
        else
        {
            bs.ServerStartingFire(this);
        }
    }
  }


  [Command]
  private void CmdInteractServer()
  {
    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 10, Color.red);
    Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10);
    if (hit.collider != null)
    {
      if (hit.collider.GetComponent<Interact>())
      {
        hit.collider.GetComponent<Interact>().ServerInteract(this);
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


  /// <summary>
  /// Move the player depends on horizontal and vertical inputs
  /// </summary>
  /// <param name="horizontalInput">Side Move</param>
  /// <param name="verticalInput">Foward Move</param>
  public void SetInputs(float horizontalInput, float verticalInput)
  {
    //playerRigibody.velocity = new Vector3(horizontalInput * m_playerSpeed, 0, verticalInput * m_playerSpeed);
    Vector3 newFowardMove = transform.forward * m_playerSpeed * verticalInput;
    Vector3 newSideMove = transform.right * m_playerSpeed * horizontalInput;
    Vector3 newMove = newFowardMove + newSideMove;
    newMove.y = playerRigibody.velocity.y;
    playerRigibody.velocity = newMove;

  }
  /// <summary>
  /// Rotate Player depends on Mouse Inputs
  /// </summary>
  /// <param name="horizontalRotation">Mouse X</param>
  /// <param name="verticalRotation">Mouse Y</param>
  public void RotatePlayer(float horizontalRotation, float verticalRotation)//TODO: do we need vertical rotation for the head?
  {
    transform.Rotate(new Vector3(0, horizontalRotation * m_PlayerRotationX, 0));
  }



}

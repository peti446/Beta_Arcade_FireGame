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


using UnityEngine;
using UnityEngine.Networking;

public enum EPlayerStatus
{
	Idle, Moving, Interacting, Stunned, Dead
}

public class Character : NetworkBehaviour
{
	[SerializeField]
	private float m_movingSpeed = 8.5f;
	[SerializeField]
	private float m_movingRotationSpeed = 2.5f;
	[SerializeField]
	private float m_cameraRotationSpeed = 2.0f;
	[SerializeField]
	private GameObject m_cameraPivot;
	[SerializeField]
	private GameObject m_graphics;
	[SerializeField]
	private GameObject m_collision;

	//Last object that has been interacted with
	private GameObject m_lastInteractedObj;
	//Quick acces for the rigid body
	private Rigidbody m_rigidBodyComp;

	/// <summary>
	/// Status of the current player
	/// </summary>
	public EPlayerStatus State
	{
		get;
		private set;
	}

	//Variables for movement
	private Vector3 m_movingDirection;
	private Quaternion m_movingRotation;
	//Variables for camera movement
	private Vector2 m_cameraRotation;


	//The Network ID of the player that is currently controlling the character
	[SyncVar(hook = "OnPlayerIdChanged")]
	private int m_controllingPlayerID = -1;
	/// <summary>
	/// The Network ID of the player that is currently controlling the character
	/// </summary>
	public int ControllingPlayerID
	{
		get
		{
			return m_controllingPlayerID;
		}
	}

	//Variables to know if the player is compleatly setted up
	private bool m_isSetup = false;
	private bool m_spawned = false;
	private bool m_autoritySet = false;

	private void Awake()
	{
		m_rigidBodyComp = GetComponent<Rigidbody>();
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
		SetUpLocalPlayer();
	}

	//Usless variables
	private Vector3 DampVelocityPosition;
	private Vector2 DampVelocityCamera;
	[ClientCallback]
	private void LateUpdate()
	{
		if (State == EPlayerStatus.Moving)
		{
			//Rotate the player
			m_rigidBodyComp.MoveRotation(m_movingRotation);
			//Set the velocity, taking into account the gravity of the world
			m_rigidBodyComp.velocity = (m_movingDirection * m_movingSpeed) + new Vector3(0, m_rigidBodyComp.velocity.y, 0);

			m_cameraPivot.transform.rotation = transform.rotation;
			m_cameraRotation = Vector2.SmoothDamp(m_cameraRotation, Vector2.zero, ref DampVelocityCamera, 0.2f, 99999, Time.deltaTime);
		}


		//Update camera rotation and position for the 3 person
		Quaternion rot = Quaternion.Euler(m_cameraRotation.x, m_cameraRotation.y, 0);
		Vector3 cameraTargetPos = (transform.position + transform.up * 8) - rot * transform.forward * 15;//(gameObject.transform.position + gameObject.transform.up * 8) + newRotation * new Vector3(0, 0, -10);

		//Check if a object would block the camera
		RaycastHit hit;
		if (Physics.Linecast(gameObject.transform.position, cameraTargetPos, out hit, ~(1 << 29 | 1 << 28)))
		{
			Debug.Log(hit.transform.name);
			cameraTargetPos = hit.point + hit.normal;
		}

		//Update the position and rotation
		m_cameraPivot.transform.position = Vector3.SmoothDamp(m_cameraPivot.transform.position, cameraTargetPos, ref DampVelocityPosition, 0.05f);
		m_cameraPivot.transform.LookAt(gameObject.transform);
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
			SetUpLocalPlayer();
		}

		//Set the character script to be settet up
		m_isSetup = true;
	}

	/// <summary>
	/// Sets up the local player such as the camera and spawning of the player
	/// </summary>
	[Client]
	private void SetUpLocalPlayer()
	{
		if (m_autoritySet && !hasAuthority)
			return;
		Debug.Log("Player got authority ID:" + m_controllingPlayerID);
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		camera.transform.SetParent(m_cameraPivot.transform, false);
		camera.transform.localPosition = new Vector3(0, 0, 0);
		gameObject.GetComponent<PlayerInputs>().enabled = true;
		CmdSpawn();
		m_autoritySet = true;
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

	public void ToggleHose(bool open)
	{
		if (open)
		{
			//hose.GetComponent<ParticleSystem>().enableEmission = true;
			//hose.GetComponent<ParticleSystem>().Play();
		}
		else if (!open)
		{
			//hose.GetComponent<ParticleSystem>().enableEmission = false;
		}
	}

	///TODO: JOE KONY COMENTA LAS COSDAS POR DIOS (FIRMADO KONY)
	/// <summary>
	/// 11
	/// </summary>
	[Client]
	public void InteractRay()
	{
		Debug.DrawRay(transform.position, transform.forward * 10, Color.red, 5.0f);
		RaycastHit hit;
		Physics.Raycast(transform.position + transform.up, transform.forward, out hit, 10);
		if (hit.collider != null)
		{
			if (hit.collider.GetComponent<Interact>() != null)
			{
				CmdInteractServer();
				hit.collider.GetComponent<Interact>().ClientInteract(this);
				m_lastInteractedObj = hit.collider.gameObject;
				
			}
		}
	}

	[Command]
	private void CmdInteractServer()
	{
		Debug.DrawRay(transform.position, transform.forward * 10, Color.red);
		RaycastHit hit;
		Physics.Raycast(transform.position + transform.up, transform.forward, out hit, 10);
		if (hit.collider != null)
		{
			if (hit.collider.GetComponent<Interact>() != null)
			{
				hit.collider.GetComponent<Interact>().ServerInteract(this);
				m_lastInteractedObj = hit.collider.gameObject;
			}
		}
	}

	[Client]
	public void StopInteraction()
	{
		if (m_lastInteractedObj != null && m_lastInteractedObj.GetComponent<Interact>() != null)
		{
			CmdServerStopInteract();
			m_lastInteractedObj.GetComponent<Interact>().ClientStopInteract.Invoke(this);
		}
	}

	[Command]
	private void CmdServerStopInteract()
	{
		if (m_lastInteractedObj != null && m_lastInteractedObj.GetComponent<Interact>() != null)
		{
			m_lastInteractedObj.GetComponent<Interact>().ServerStopInteract.Invoke(this);
		}
	}

	//----------------------------------------------------------

	[TargetRpc]
	public void TargetUpdatePos(NetworkConnection target, Vector3 pos, Quaternion rotation)
	{
		//Just in case make sure we do have authority
		if (!hasAuthority)
			return;

		transform.position = pos;
		transform.rotation = rotation;
	}

	[ClientRpc]
	public void RpcSetCaracterActive(bool active)
	{
		// gameObject.SetActive(active);
		m_graphics.SetActive(active);
		m_collision.SetActive(active);

		//Update the camera
		if (active && hasAuthority)
		{
			gameObject.GetComponent<PlayerInputs>().enabled = active;
			GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
			camera.transform.SetParent(m_cameraPivot.transform, false);
			camera.transform.position = m_cameraPivot.transform.position;
			camera.transform.rotation = m_cameraPivot.transform.rotation;
		}
	}

	/// <summary>
	/// Move the player depends on horizontal and vertical inputs
	/// </summary>
	/// <param name="horizontalInput">Side Move</param>
	/// <param name="verticalInput">Foward Move</param>
	public void SetInputs(float horizontalInput, float verticalInput)
	{
		if (!(State == EPlayerStatus.Idle || State == EPlayerStatus.Moving))
			return;
		//Sets the direction to the current forward
		m_movingDirection = transform.forward * verticalInput;
		//Get the new rotation that will be used to modify the forward, eventually this sould become the forward
		m_movingRotation = transform.rotation * Quaternion.Euler(0, horizontalInput * m_movingRotationSpeed, 0);

		//Change the state based on input
		if (horizontalInput == 0.0f && verticalInput == 0.0f)
		{
			//Set the state to IDLE
			State = EPlayerStatus.Idle;
		}
		else
		{
			//Set the state to moving as we are
			State = EPlayerStatus.Moving;
		}
		/*Vector3 newFowardMove = transform.forward * m_playerSpeed * verticalInput;
		Vector3 newSideMove = transform.right * m_playerSpeed * horizontalInput;
		Vector3 newMove = newFowardMove + newSideMove;
		newMove.y = m_rigidBodyComp.velocity.y;
		m_rigidBodyComp.velocity = newMove;*/

	}
	/// <summary>
	/// Rotate Player depends on Mouse Inputs
	/// </summary>
	/// <param name="horizontalRotation">Mouse X</param>
	/// <param name="verticalRotation">Mouse Y</param>
	public void RotatePlayer(float horizontalRotation, float verticalRotation)
	{ 
		//If we are moving move the character
		if(State == EPlayerStatus.Moving)
		{
			transform.Rotate(new Vector3(0, horizontalRotation * m_cameraRotationSpeed, 0));
		}
		//Move the camera
		//Clamp the x rotation
		m_cameraRotation += new Vector2(verticalRotation * m_cameraRotationSpeed, horizontalRotation * m_cameraRotationSpeed);
		if (m_cameraRotation.x < -360)
			m_cameraRotation.x += 360;
		if (m_cameraRotation.x > 360)
			m_cameraRotation.x -= 360;
		m_cameraRotation.x = Mathf.Clamp(m_cameraRotation.x, -80, 80);
	}
}

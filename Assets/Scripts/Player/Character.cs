using UnityEngine;
using UnityEngine.Networking;

public enum EPlayerStatus
{
	Idle, Moving, Interacting, Stunned, Dead
}

[RequireComponent(typeof(Animator))]
public class Character : NetworkBehaviour
{

	private Animator m_animator;
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
	[SerializeField]
	private GameObject m_minimapCameraPivot;

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
		m_animator = gameObject.GetComponent<Animator>();
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

	//Usless variables just ignore as the functions require these :shrug: y.y
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

			  //ANIMATOR
			  m_animator.SetFloat("speed", m_rigidBodyComp.velocity.y + m_movingSpeed);

			//END OF ANIMATOR
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
			cameraTargetPos = hit.point + hit.normal;
		}

		//Update the position and rotation
		m_cameraPivot.transform.position = Vector3.SmoothDamp(m_cameraPivot.transform.position, cameraTargetPos, ref DampVelocityPosition, 0.05f);
		m_cameraPivot.transform.LookAt(gameObject.transform);
	}

	//Check if we can interact with an object or not
	[ClientCallback]
	private void Update()
	{
		RaycastHit hit;
		Physics.Raycast(transform.position + transform.up, transform.forward, out hit, 10);
		Debug.DrawRay(transform.position + transform.up, transform.forward, Color.black, 10f);
		Debug.Log("Show-1w");
		if (hit.collider != null)
		{
			Debug.Log("Show1");
			if (hit.collider.GetComponent<Interact>() != null)
			{
				Debug.Log("Show");
				GameUIHandler._instance.ShowInteractWarning(hit.collider.GetComponent<Interact>().CanClientInteract(this));
			}
		}
		else
		{
			GameUIHandler._instance.ShowInteractWarning(false);
		}
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
		if (m_autoritySet || !hasAuthority)
			return;

		Debug.Log("Player got authority ID:" + m_controllingPlayerID);
		//Set up main Camera
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		camera.transform.SetParent(m_cameraPivot.transform, false);
		camera.transform.localPosition = new Vector3(0, 0, 0);
		//Set up minimap camera
		GameObject minimapCamera = GameObject.FindGameObjectWithTag("MinimapCamera");
		minimapCamera.transform.SetParent(m_minimapCameraPivot.transform, false);
		minimapCamera.transform.localPosition = new Vector3(0, 0, 0);
		minimapCamera.transform.localRotation = Quaternion.identity;
		if (m_minimapCameraPivot.GetComponent<MinimapCameraRotationFix>() == null)
			m_minimapCameraPivot.AddComponent<MinimapCameraRotationFix>();

		//Enable the player Input and notify the server that we are ready
		gameObject.GetComponent<PlayerInputs>().enabled = true;
		GameManager._instance.LocalPlayerSettetUp(this);
		m_autoritySet = true;
	}


	/// <summary>
	/// Spawns the player at the correct position.
	/// </summary>
	/// <param name="target">The target player connection. Needs to be the player with authority</param>
	/// <param name="pos">The pos we want the player to spawn on</param>
	[TargetRpc]
	public void TargetSpawnPlayerAt(NetworkConnection target, Vector3 pos)
	{
		//If we are spawned return
		if (m_spawned)
			return;
		//Just in case make sure we do have authority
		if (!hasAuthority)
			return;

		GameUIHandler._instance.SetUpUIForCharacter();

		//Set the pos and that we spawned
		transform.position = pos;
		m_spawned = true;
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
	[Server]
	public void SetPlayerID(int id)
	{
		m_controllingPlayerID = id;
	}

	/// <summary>
	/// Raycast in client side from the player to the interactuable objects.
	/// Works by calling the server interaction function as well and calling the
	/// functions of the interactuable objects that all have the Interact script
	/// in common.
	/// </summary>
	[Client]
	public void InteractRay()
	{
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

	/// <summary>
	/// Raycast in server side from the player to the interactuable objects.
	/// Called by the client's side version of this method.
	/// </summary>
	[Command]
	private void CmdInteractServer()
	{
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

	/// <summary>
	/// Method to be called when the player's interaction with the interactuable gameobjects
	/// should be stopped, on the client side. This works by calling said gameobject's stop interact event.
	/// </summary>
	[Client]
	public void StopInteraction()
	{
		if (m_lastInteractedObj != null && m_lastInteractedObj.GetComponent<Interact>() != null)
		{
			CmdServerStopInteract();
			m_lastInteractedObj.GetComponent<Interact>().ClientStopInteract.Invoke(this);
		}
	}

	/// <summary>
	/// Method to be called when the player's interaction with the interactuable gameobjects
	/// should be stopped, on the server side. This works by calling said gameobject's stop interact event.
	/// </summary>
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

			GameObject minimapCamera = GameObject.FindGameObjectWithTag("MinimapCamera");
			minimapCamera.transform.SetParent(m_minimapCameraPivot.transform, false);
			minimapCamera.transform.localPosition = new Vector3(0, 0, 0);
			minimapCamera.transform.localRotation = Quaternion.identity;
			if(m_minimapCameraPivot.GetComponent<MinimapCameraRotationFix>() == null)
				m_minimapCameraPivot.AddComponent<MinimapCameraRotationFix>();
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
		if (State == EPlayerStatus.Moving)
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

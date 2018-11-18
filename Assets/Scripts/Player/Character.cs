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
		hose = transform.Find("HoseParticles").gameObject;
	}


	//Start function when object spawned, called on all client.
	public override void OnStartClient()
	{
		base.OnStartClient();
		if(!m_isSetup && m_controllingPlayerID != -1)
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

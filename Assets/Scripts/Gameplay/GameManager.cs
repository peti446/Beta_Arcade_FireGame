using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


//May not be nececary
public enum EGameState  
{
	WaitingForPlayers, Playing , GameEnded , EveryoneDisconnected
};

public struct GameManagerPlayerListSync
{
	public Character character;
}

public class SyncListCharacters : SyncListStruct<GameManagerPlayerListSync> { }

[RequireComponent(typeof(NetworkIdentity))]
public class GameManager : NetworkBehaviour
{
	[SerializeField]
	private GameObject m_fireTruckPrefab;
	
	//Vheicle spawn manager
	private float m_timeLastTruckDied;
	//The truck object
	private GameObject m_spawnedTruck = null;

	[SerializeField]
	private int m_maxGameLenghtSecond = 180;
	private float m_timeGameStarted;

	//Syncvars
	[SyncVar]
	private float m_gameTimeLeft;
	[SyncVar]
	private SyncListCharacters m_characterList = new SyncListCharacters();

	/// <summary>
	/// Time in seconds left 
	/// </summary>
	public int GameSecondsLeft
	{
		get
		{
			return Mathf.FloorToInt(m_gameTimeLeft);
		}
	}

	/// <summary>
	/// Current state of the game
	/// </summary>
	public EGameState State
	{
		get;
		private set;
	}

	/// <summary>
	/// The local character for this client, ir can be null if the player is not playiing
	/// </summary>
	public Character LocalCharacter
	{
		get;
		private set;
	}

	public static GameManager _instance
    {
        get;
        private set;
    }

    /// <summary>
    /// Keeps the GameManager object alive throughout the whole game
    /// and prevents it from being duplicated
    /// </summary>
    private void Awake()
	{
		if (_instance == null)
			_instance = this;
		else if (_instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(this);
		//initialise - game start loading calling 
		m_timeLastTruckDied = float.PositiveInfinity;
		//Suscribe to events to the network manager
		if(MainNetworkManager._instance != null)
		{
			MainNetworkManager._instance.ClientDisconected += OnDisconnect;
			MainNetworkManager._instance.ClientErrorHappend += OnError;
			MainNetworkManager._instance.ServerErrorHappend += OnError;
			MainNetworkManager._instance.ConnectionDroped += OnDrop;
		}
		//Set the state
		State = EGameState.WaitingForPlayers;
		m_timeGameStarted = Time.time;

	}

	private void OnDestroy()
	{
		//Suscribe to events to the network manager
		if (MainNetworkManager._instance != null)
		{
			MainNetworkManager._instance.ClientDisconected -= OnDisconnect;
			MainNetworkManager._instance.ClientErrorHappend -= OnError;
			MainNetworkManager._instance.ServerErrorHappend -= OnError;
			MainNetworkManager._instance.ConnectionDroped -= OnDrop;
		}

		if (_instance = this)
			_instance = null;
	}

	[ServerCallback]
	private void Update()
	{
		switch(State)
		{
			case EGameState.Playing:
				{
					//Check for truck spawn
					if (m_spawnedTruck != null)
					{
						if (Time.time - m_timeLastTruckDied > 6000)
						{
							SpawnFiretruk();
						}
					}
				}
				break;
		}
	}

	[ServerCallback]
	private void FixedUpdate()
	{
		m_gameTimeLeft = (Time.time - m_timeGameStarted) - (float)m_maxGameLenghtSecond;
	}

	[ServerCallback]
	private void LateUpdate()
	{
		
	}

	/// <summary>
	/// Makes sure the truck reference is destroyed so that we can spawn a new one in 6000 seconds
	/// </summary>
	/// <param name="truck">The truck that has been destroyed</param>
	[Server]
	public void TruckDestroyed(GameObject truck)
	{
		if(m_spawnedTruck == truck)
		{
			m_timeLastTruckDied = Time.time;
			m_spawnedTruck = null;
		}
	}

	/// <summary>
	/// Spawns a characte in the world. Can only be called on server
	/// </summary>
	/// <param name="c">The character to spawn</param>
	[Server]
	public void SpawnPlayer(Character c)
	{
		if(SpawnManager._instance != null)
		{
			SpawnPoint spawnPoint = SpawnManager._instance.GetSpawnPoint(c);
			if (spawnPoint != null)
			{
				c.TargetSpawnPlayerAt(MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].connectionToClient, spawnPoint.transform.position);
			}
			else
			{
				Debug.Log("No free spawn point available");
			}
		}
	}

	/// <summary>
	/// Spawn the fire truck in the station, afterwards notifies the player. Can only be called on server
	/// </summary>
	[Server]
	private void SpawnFiretruk()
	{
		//Cannot spawn if we already have it
		if (m_spawnedTruck == null)
			return;

		GameObject ft = Instantiate(m_fireTruckPrefab);
		SpawnPoint sp = SpawnManager._instance.GetFireTruckSpawnPoint(ft);
		if (sp != null)
			ft.transform.position = sp.transform.position;

		m_spawnedTruck = ft;
		NetworkServer.Spawn(ft);
	}

	[Command]
	private void CmdPlayerSettedUp(GameObject player)
	{
		m_characterList.Add(new GameManagerPlayerListSync() { character = player.GetComponent<Character>() });
		SpawnPlayer(player.GetComponent<Character>());
	}

	[Client]
	public void LocalPlayerSettetUp(Character localCharacteR)
	{
		//If it is not the local JA! Return
		if (!localCharacteR.hasAuthority)
			return;
		//Set the local player
		LocalCharacter = localCharacteR;
		//Send a command that the player is ready
		CmdPlayerSettedUp(localCharacteR.gameObject);
	}

	#region Network Events
	private void OnDisconnect(NetworkConnection conn)
	{

	}

	private void OnDrop()
	{

	}

	private void OnError(NetworkConnection conn, int code)
	{

	}
	#endregion
}

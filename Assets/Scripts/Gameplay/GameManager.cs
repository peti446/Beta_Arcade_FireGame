using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


//May not be nececary
public enum EGameState  
{
	WaitingForPlayers, Playing , GameEnded , EveryoneDisconnected
};

public struct GameObjectListObject
{
	public GameObject o;
}

public class GameObjectSyncList : SyncListStruct<GameObjectListObject> { }

[RequireComponent(typeof(NetworkIdentity))]
public class GameManager : NetworkBehaviour
{
	//Game time variables
	[SerializeField]
	private int m_maxGameLenghtSecond = 180;
	private float m_timeGameStarted;

	//Starting the game timers
	[SerializeField]
	private int m_maxWaitingTimeSeconds = 30;
	private float m_timeStartedWaiting;

	//Firetruck variables
	[SerializeField]
	private int m_FiretruckRespainTime = 60;
	[SerializeField]
	private GameObject m_fireTruckPrefab;
	//Vheicle spawn manager
	private float m_timeLastTruckDied;
	//The truck object
	private GameObject m_spawnedTruck = null;

	//Building game ending variables
	private List<BuildingStatus> m_AliveBuildings = new List<BuildingStatus>();
	private List<BuildingStatus> m_destroyedBuilding = new List<BuildingStatus>();

	//EndGame variables
	private float m_timeEndGameStart;

	//Syncvars
	[SyncVar]
	private float m_gameTimeLeft = 180;
	[SyncVar]
	private float m_waitingTimeLeft = 30;
	[SyncVar]
	private float m_porcentInAshes = 0;
	[SyncVar]
	private float m_timeLeftToLeaveGame = 10;
	[SyncVar]
	private GameObjectSyncList m_characterList = new GameObjectSyncList();
	[SyncVar]
	private EGameState m_state = EGameState.WaitingForPlayers;

	/// <summary>
	/// Time in seconds left 
	/// </summary>
	public int GameSecondsLeft
	{
		get
		{
			return Mathf.CeilToInt(m_gameTimeLeft);
		}
	}
	/// <summary>
	/// Time left to start the game
	/// </summary>
	public int WaitingTimeLeft
	{
		get
		{
			return Mathf.CeilToInt(m_waitingTimeLeft);
		}
	}
	/// <summary>
	/// Porcentage of the city destroyed
	/// </summary>
	public float PorcentBurned
	{
		get
		{
			return m_porcentInAshes;
		}
	}

	public int EndGameTimeLeft
	{
		get
		{
			return Mathf.CeilToInt(m_timeLeftToLeaveGame);
		}
	}

  public Character GetCharacter(int id)
  {
    foreach(GameObjectListObject lo in m_characterList)
    {
      if(lo.o.GetComponent<Character>().ControllingPlayerID == id)
      {
        return lo.o.GetComponent<Character>();
      }
    }
    return null;
  }

	/// <summary>
	/// Current state of the game
	/// </summary>
	public EGameState State
	{
		get
		{
			return m_state;
		}
		private set
		{
			m_state = value;
		}
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
 
  public ETeams WinningTeam{
    get { return m_porcentInAshes > 0.66f ? ETeams.CrazyPeople : ETeams.FireFighters; }
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

		//Suscribe to events to the network manager
		if(MainNetworkManager._instance != null)
		{
			MainNetworkManager._instance.ClientDisconected += OnDisconnect;
			MainNetworkManager._instance.ClientErrorHappend += OnError;
			MainNetworkManager._instance.ServerErrorHappend += OnError;
			MainNetworkManager._instance.ConnectionDroped += OnDrop;
		}
		//Generaty city if nececary
		if(ProceduralMapManager._instance != null)
		ProceduralMapManager._instance.GenerateCity();
	}

	public override void OnStartServer()
	{
		//Set the state
		State = EGameState.WaitingForPlayers;
		m_timeStartedWaiting = Time.time;
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
			case EGameState.WaitingForPlayers:
				{
					if (m_waitingTimeLeft <= 0)
					{
						List<int> playersIDConnected = new List<int>();
						//Spawn all players
						foreach (GameObjectListObject objectPlayer in m_characterList)
						{
							SpawnPlayer(objectPlayer.o.GetComponent<Character>());
							playersIDConnected.Add(objectPlayer.o.GetComponent<Character>().ControllingPlayerID);
						}
						List<NetworkPlayer> tempList = new List<NetworkPlayer>(MainNetworkManager._instance.PlayersConnected);
						//Disconnect the other players
						foreach(NetworkPlayer netP in tempList)
						{
							if(netP != null && MainNetworkManager._instance.PlayersConnected.Contains(netP) && !playersIDConnected.Contains(netP.Player_ID))
							{
								MainNetworkManager._instance.RemoveNetPlayer(netP);
							}
						}
						m_timeGameStarted = Time.time;
						State = EGameState.Playing;
					}
				}
				break;
			case EGameState.Playing:
				{
					//Check for truck spawn
					if (m_spawnedTruck == null)
					{
						if (Time.time - m_timeLastTruckDied > m_FiretruckRespainTime)
						{
							SpawnFiretruk();
						}
					}

					//Check if we finished the game
					if (m_gameTimeLeft <= 0 || m_porcentInAshes >= 0.60f)
					{
						foreach(GameObjectListObject o in m_characterList)
						{
							o.o.GetComponent<Character>().RpcDisableInput();
						}
						m_timeEndGameStart = Time.time;
						State = EGameState.GameEnded;
						GameUIHandler._instance.SetUpEndGameUI();
					}
				}
				break;
			case EGameState.GameEnded:
				{
					if(m_timeLeftToLeaveGame <= 0)
					{
						MainNetworkManager._instance.Disconect();
						SceneManager.LoadScene(0);
					}
				}
				break;
		}
	}

	[ServerCallback]
	private void FixedUpdate()
	{
		if (State == EGameState.WaitingForPlayers)
		{
			m_waitingTimeLeft = (float)m_maxWaitingTimeSeconds - (Time.time - m_timeStartedWaiting);
		}
		else if (State == EGameState.Playing)
		{
			m_gameTimeLeft = (float)m_maxGameLenghtSecond - (Time.time - m_timeGameStarted);
		}
		else if(State == EGameState.GameEnded)
		{
			m_timeLeftToLeaveGame = 10.0f - (Time.time - m_timeEndGameStart);
		}
	}

	[ServerCallback]
	private void LateUpdate()
	{
		
	}

	/// <summary>
	/// Registersa building with the manager
	/// </summary>
	/// <param name="bs">The building that wants to register to participate in the game</param>
	[Server]
	public void RegisterBuilding(BuildingStatus bs)
	{
		if (!m_AliveBuildings.Contains(bs) && !m_destroyedBuilding.Contains(bs))
		{
			m_AliveBuildings.Add(bs);
		}
	}

	/// <summary>
	/// A building claimed itself as destroyed
	/// </summary>
	/// <param name="bs">The building that is destroyed</param>
	[Server]
	public void BuildingDestroyed(BuildingStatus bs)
	{
		if(m_AliveBuildings.Contains(bs) && !bs.IsAlive)
		{
			m_AliveBuildings.Remove(bs);
			m_destroyedBuilding.Add(bs);
			m_porcentInAshes = (float)m_destroyedBuilding.Count / (float)m_AliveBuildings.Count; 
		}
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
		//Check if we are in the correct state
		if (State != EGameState.WaitingForPlayers)
			return;
		//Add the player to the list
		m_characterList.Add(new GameObjectListObject() { o = player });
		//Check if all players are where
		if(m_characterList.Count == MainNetworkManager._instance.PlayersConnected.Count)
		{
			m_timeStartedWaiting = Time.time;
			m_maxWaitingTimeSeconds = 5;
		}
	}

	/// <summary>
	/// Sets the reference for the local player
	/// </summary>
	/// <param name="localCharacteR">the client</param>
	[Client]
	public void LocalPlayerSettetUp(Character localCharacteR)
	{
		//Check if we are in the correct state
		if (State != EGameState.WaitingForPlayers)
			return;
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
		MainNetworkManager._instance.Restart();
	}

	private void OnDrop()
	{
		MainNetworkManager._instance.Restart();
	}

	private void OnError(NetworkConnection conn, int code)
	{
		MainNetworkManager._instance.Restart();
	}
	#endregion
}

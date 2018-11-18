using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


//May not be nececary
public enum EGameState  
{
  InGame, InMainMenu, LoadingGame, InLobby, OnPauseScreen
};

[RequireComponent(typeof(NetworkIdentity))]
public class GameManager : NetworkBehaviour
{
	[SerializeField]
	private GameObject m_fireTruckPrefab;

	SyncList<GameObject> d;

	public static GameManager _instance = null;
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
	}

	private void OnDestroy()
	{
		if (_instance = this)
			_instance = null;
	}

	[ServerCallback]
	private void Update()
	{
		
	}

	[ServerCallback]
	private void FixedUpdate()
	{
		
	}

	[ServerCallback]
	private void LateUpdate()
	{
		
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
	public void SpawnFiretruk()
	{
		GameObject ft = Instantiate(m_fireTruckPrefab);
		NetworkServer.Spawn(ft);
	}
}

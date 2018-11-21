using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpawnPoint : MonoBehaviour {

	[SerializeField]
	private ETeams m_spawnSide;
	[SerializeField]
	private bool m_isFireTruckSpawn = false;
	public bool IsTruckSpawn
	{
		get
		{
			return m_isFireTruckSpawn;
		}
	}

	private GameObject m_spawnPointUsedObject;
	private IList<GameObject> m_EntitiesInSpawn;
	private float m_timeSpawnAssigned;


	/// <summary>
	/// Gets if this spawn is usable or not
	/// </summary>
	public bool IsSpawnFree
	{
		get
		{
			//Can only use this spawn if we have no character waiting to be sapwned here, or if no other character is in it
			return m_spawnPointUsedObject == null && m_EntitiesInSpawn.Count == 0;
		}
	}

	/// <summary>
	/// Returns the team this spawns belongs to
	/// </summary>
	public ETeams SpawnSide
	{
		get
		{
			return m_spawnSide;
		}
	}

	private void Awake()
	{
		//Init values
		m_spawnPointUsedObject = null;
		m_EntitiesInSpawn = new List<GameObject>();
	}


	private void Update()
	{
		//Reset the spawn if a player takes more then 10 seconds to position itself on the spawn
		if(m_EntitiesInSpawn != null)
		{
			if(Time.time - m_timeSpawnAssigned > 10.0f)
			{
				ResetSpawn();
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		//Check if we hitted a character or vehicle 
		Character charHitted = collision.transform.GetComponent<Character>();
		Vehicle vehicle = collision.transform.GetComponent<Vehicle>();
		if (charHitted != null || vehicle != null)
		{
			//We hitted a character or vheicle so add it to the list
			m_EntitiesInSpawn.Add(collision.transform.gameObject);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		//Check if the character that soposued to be spawned here exited
		if (collision.gameObject == m_spawnPointUsedObject)
		{
			//Reset the flag
			m_spawnPointUsedObject = null;
		}

		//Remove the character from the character in spawn list
		m_EntitiesInSpawn.Remove(collision.gameObject);
	}

	/// <summary>
	/// Marks the spawner as used by a specific character
	/// </summary>
	/// <param name="c">The character that is using this spawn</param>
	public void UseSpawn(GameObject c)
	{
		//Set reference and time
		m_spawnPointUsedObject = c;
		m_timeSpawnAssigned = Time.time;
	}


	/// <summary>
	/// Reset the sapwn in case we dont want to use it
	/// </summary>
	public void ResetSpawn()
	{
		//Reset reference
		m_spawnPointUsedObject = null;
	}
}

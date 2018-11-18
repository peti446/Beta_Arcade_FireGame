using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpawnPoint : MonoBehaviour {

	[SerializeField]
	private ETeams m_spawnSide;

	private Character m_spawnPointUsedCharacter;
	private IList<Character> m_characterInSpawn;
	private float m_timeSpawnAssigned;


	/// <summary>
	/// Gets if this spawn is usable or not
	/// </summary>
	public bool IsSpawnFree
	{
		get
		{
			//Can only use this spawn if we have no character waiting to be sapwned here, or if no other character is in it
			return m_spawnPointUsedCharacter == null && m_characterInSpawn.Count == 0;
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
		m_spawnPointUsedCharacter = null;
		m_characterInSpawn = new List<Character>();
	}


	private void Update()
	{
		//Reset the spawn if a player takes more then 10 seconds to position itself on the spawn
		if(m_characterInSpawn != null)
		{
			if(Time.time - m_timeSpawnAssigned > 10.0f)
			{
				ResetSpawn();
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		//Check if we hitted a character 
		Character charHitted = collision.transform.GetComponent<Character>();
		if (charHitted != null)
		{
			//We hitted a character so add it to the list
			m_characterInSpawn.Add(charHitted);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		Character charHitted = collision.transform.GetComponent<Character>();
		//Check if we hitted the character we were waiting for
		if (charHitted == m_spawnPointUsedCharacter)
		{
			//Reset the flag
			m_spawnPointUsedCharacter = null;
		}

		//Remove the character from the character in spawn list
		m_characterInSpawn.Remove(charHitted);
	}

	/// <summary>
	/// Marks the spawner as used by a specific character
	/// </summary>
	/// <param name="c">The character that is using this spawn</param>
	public void UseSpawn(Character c)
	{
		//Set reference and time
		m_spawnPointUsedCharacter = c;
		m_timeSpawnAssigned = Time.time;
	}


	/// <summary>
	/// Reset the sapwn in case we dont want to use it
	/// </summary>
	public void ResetSpawn()
	{
		//Reset reference
		m_spawnPointUsedCharacter = null;
	}
}

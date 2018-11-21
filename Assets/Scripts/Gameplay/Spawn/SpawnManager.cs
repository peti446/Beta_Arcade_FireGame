using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

	private IList<SpawnPoint> m_FireStationSpawns;
	private IList<SpawnPoint> m_BurnlingsSpawns;
	private IList<SpawnPoint> m_TruckSpawns;

	public static SpawnManager _instance
    {
        get;
        private set;
    }

    private void Awake()
    {
		if (_instance != null)
		{
			Destroy(gameObject);
			return;
		}
        _instance = this;

		//Set the list
		m_FireStationSpawns = new List<SpawnPoint>();
		m_BurnlingsSpawns = new List<SpawnPoint>();
		m_TruckSpawns = new List<SpawnPoint>();

		//Add the spawns to the correct list
		SpawnPoint[] allSpawns = GameObject.FindObjectsOfType<SpawnPoint>();
		foreach(SpawnPoint sp in allSpawns)
		{
			switch(sp.SpawnSide)
			{
				case ETeams.CrazyPeople:
					m_BurnlingsSpawns.Add(sp);
					break;
				case ETeams.FireFighters:
					if(sp.IsTruckSpawn)
					{
						m_TruckSpawns.Add(sp);
					}
					else
					{
						m_FireStationSpawns.Add(sp);
					}
					break;
			}
		}
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    public SpawnPoint GetSpawnPoint(Character c)
    {
        switch (MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].Player_Team)
        {
            case ETeams.CrazyPeople:
                foreach(SpawnPoint sp in m_BurnlingsSpawns)
                {
					if(sp.IsSpawnFree)
					{
						sp.UseSpawn(c.gameObject);
						return sp;
					}
                }
                break;
            case ETeams.FireFighters:
                foreach (SpawnPoint sp in m_FireStationSpawns)
                {
					if (sp.IsSpawnFree)
					{
						sp.UseSpawn(c.gameObject);
						return sp;
					}
				}
                break;
        }

        return null;
    }

	public SpawnPoint GetFireTruckSpawnPoint(GameObject fireTruck)
	{
		foreach(SpawnPoint sp in m_TruckSpawns)
		{
			if(sp.IsSpawnFree)
			{
				sp.UseSpawn(fireTruck);
				return sp;
			}
		}

		return null;
	}
}

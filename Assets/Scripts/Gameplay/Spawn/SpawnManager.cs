using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

    [SerializeField]
    private GameObject[] m_FireStationSpawns;
    [SerializeField]
    private GameObject[] m_BurnlingsSpawns;

    public Transform GetSpawnPoint(ETeams team)
    {
        Transform t = gameObject.transform;
        switch (team)
        {
            case ETeams.CrazyPeople:
                foreach(GameObject sp in m_BurnlingsSpawns)
                {
                    Collider[] allCollions = Physics.OverlapBox(sp.transform.position, new Vector3(10,10,10));
                    bool hasCharacter = false;
                    foreach(Collider c in allCollions)
                    {
                        if(c.gameObject.GetComponentInChildren<Character>() != null)
                        {
                            hasCharacter = true;
                            break;
                        }
                    }
                    
                    if(!hasCharacter)
                    {
                        t = sp.transform;
                        break;
                    }
                }
                break;
            case ETeams.FireFighters:
                foreach (GameObject sp in m_BurnlingsSpawns)
                {
                    Collider[] allCollions = Physics.OverlapBox(sp.transform.position, new Vector3(10, 10, 10));
                    bool hasCharacter = false;
                    foreach (Collider c in allCollions)
                    {
                        if (c.gameObject.GetComponentInChildren<Character>() != null)
                        {
                            hasCharacter = true;
                            break;
                        }
                    }

                    if (!hasCharacter)
                    {
                        t = sp.transform;
                        break;
                    }
                }
                break;
        }

        return t;
    }
}

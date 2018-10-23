using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{

    public GameObject player;
    public GameObject fire_bar;
    public GameObject bullet_prefab;
    public GameObject water_prefab;
    public Transform bullet_spawn;
    private bool coll = false;

    public GameObject building;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonUp("Fire1"))
        {
            ShootWater();
        }
        if(coll == true)
        {
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Holding E");
                building.gameObject.GetComponent<BuildingStatus>().StartingFire();

            }
        }

    }

    void ShootWater()
    {
        Instantiate(water_prefab, bullet_spawn.position, bullet_spawn.rotation);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Building")
        {
            coll = true;
            Debug.Log("Collided");
            //col.gameObject.GetComponent<BuildingStatus>().FireStart();
        }
        else
        {
            coll = false;
        }
    }

}

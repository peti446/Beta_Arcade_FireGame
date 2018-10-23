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
        //If player presses F
        if(Input.GetKey(KeyCode.F))
        {
            //Start Fire on building
            building.gameObject.GetComponent<BuildingStatus>().StartingFire();
        }
        //If player presses E
        if(Input.GetKey(KeyCode.E))
        {
            //Extinguish fire on building
            building.gameObject.GetComponent<BuildingStatus>().Extinguish();
        }
    }
}

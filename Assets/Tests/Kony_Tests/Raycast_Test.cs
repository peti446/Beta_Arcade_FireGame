using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast_Test : MonoBehaviour {


  private RaycastHit hit;
  // Use this for initialization
  void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), 10))
    {
    }
	}
}

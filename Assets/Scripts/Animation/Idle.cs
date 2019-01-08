using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : MonoBehaviour {
    
    public Animator anim;
    public GameObject animHolder;


	// Use this for initialization
	void Start ()
    {
        //anim = gameObject.GetComponent<Animator>();
        //anim = animHolder.GetComponent<Animator>();
        
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Working");
            anim.Play("Idle");
        }
	}
}

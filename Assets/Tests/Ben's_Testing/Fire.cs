using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    float speed;
    // Use this for initialization
    void Start()
    {
        speed = 15;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * Time.deltaTime * speed;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            other.gameObject.GetComponent<BuildingStatus>().Hit();
            Debug.Log("Hit Building");
            Destroy(gameObject);
        }
    }
}

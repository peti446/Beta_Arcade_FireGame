using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleInputs : MonoBehaviour {
      
 
    private Vehicle m_vehicle;
  
    private float m_horizontalInput;
    private float m_verticalInput;
    

    private void Awake()
    {
        m_vehicle = GetComponent<Vehicle>();
    }

    // Update is called once per frame
    void Update()
    {
        m_horizontalInput = Input.GetAxis("Horizontal");
        m_verticalInput = Input.GetAxis("Vertical");

        m_vehicle.SetInputs(m_horizontalInput, m_verticalInput);


        if (Input.GetKey(KeyCode.F))
        {
            m_vehicle.ShootWater();
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            m_vehicle.ExitVehicle();
        }
        
    }

}

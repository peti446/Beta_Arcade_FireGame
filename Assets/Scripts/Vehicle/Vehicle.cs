using UnityEngine;
using UnityEngine.Networking;

public enum EVehicleStatus
{
    IDLE, AcceleratingFoward, AcceleratingBack, DesceleratingFoward, DesceleratingBack, Brake
}


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkIdentity))]
public class Vehicle : NetworkBehaviour {

    private Rigidbody m_vehicleRigibody;
    [SerializeField]
    private float m_vehicleMaxSpeed = 20.0f;
    [SerializeField]
    private float m_vehicleAcceleration = 5.0f;

    [SerializeField]
    private float m_maxVehicleTurn = 2.0f;
    [SerializeField]
    private float m_vehicleBrake = 3.0f;
    [SerializeField]
    private float m_vehicleReverseMaxSpeed = 5.0f;
    [SerializeField]
    private float m_vehicleReverseAcceleration = 0.1f;

    [SerializeField]
    private float m_vehicleUsageLife = 100f;


    [SyncVar]
    private int m_controllingPlayerID = -1;

    //Don't change this values on editor, just showed on editor for debug information
    private float m_vehicleSpeed = 0.0f;
    private float m_vehicleTurn = 0.0f;

       
    public EVehicleStatus State
    {
        get;
        private set;
    }

    private void Awake()
    {
        m_vehicleRigibody = GetComponent<Rigidbody>();
    }

    public void GainControl(Character c)
    {
        if (c.hasAuthority)
        {
            gameObject.GetComponent<VehicleInputs>().enabled = true;
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            camera.transform.SetParent(gameObject.transform, false);
        }
    }

    public void LoseControl(Character c)
    {
        if (c.hasAuthority)
            gameObject.GetComponent<VehicleInputs>().enabled = false;
    }

    private void FixedUpdate()
    {
        UpdateVehicleVelocity();
    }
    
    private void UpdateVehicleVelocity()
    {
        switch(State)
        {
            case EVehicleStatus.AcceleratingFoward:
                m_vehicleSpeed += m_vehicleAcceleration;
                m_vehicleSpeed = Mathf.Clamp(m_vehicleSpeed, 0, m_vehicleMaxSpeed);
                break;
            case EVehicleStatus.AcceleratingBack:
                m_vehicleSpeed -= m_vehicleBrake;
                m_vehicleSpeed = Mathf.Clamp(m_vehicleSpeed, 0, m_vehicleMaxSpeed);
                break;
            case EVehicleStatus.Brake:
                m_vehicleSpeed -= m_vehicleReverseAcceleration;
                m_vehicleSpeed = Mathf.Clamp(m_vehicleSpeed, m_vehicleReverseMaxSpeed * -1, 0);
                break;
            case EVehicleStatus.DesceleratingFoward:
                m_vehicleSpeed -= m_vehicleAcceleration;
                m_vehicleSpeed = Mathf.Clamp(m_vehicleSpeed, 0, m_vehicleMaxSpeed);
                break;
            case EVehicleStatus.DesceleratingBack:
                m_vehicleSpeed += m_vehicleReverseAcceleration;
                m_vehicleSpeed = Mathf.Clamp(m_vehicleSpeed, m_vehicleReverseMaxSpeed * -1, 0);
                break;

            default:
                break;
        }

        //Vehicle Turn
        transform.Rotate(0, Mathf.Clamp(m_vehicleSpeed * 0.1f, -m_maxVehicleTurn, m_maxVehicleTurn) * m_vehicleTurn, 0);
        Vector3 newMove = transform.forward * m_vehicleSpeed;
        newMove.y = m_vehicleRigibody.velocity.y;
        m_vehicleRigibody.velocity = newMove;

    }

    /// <summary>
    /// Move the vehicle depends on horizontal and vertical inputs
    /// </summary>
    /// <param name="horizontalInput">Side Move</param>
    /// <param name="verticalInput">Foward Move</param>
    public void SetInputs(float horizontalInput, float verticalInput)
    {        
        if (verticalInput > 0)
        {
            State = EVehicleStatus.AcceleratingFoward;    
        }
        else if (verticalInput < 0)
        {
            if (m_vehicleSpeed > 0)
            {              
                State = EVehicleStatus.AcceleratingBack;
            }
            else
            {                
                State = EVehicleStatus.Brake;
            }            
        }
        else
        {
            if (m_vehicleSpeed > 0)
            {
                State = EVehicleStatus.DesceleratingFoward;                
            }
            else
            {
                State = EVehicleStatus.DesceleratingBack;                
            }
        }
        
        //Vehicle Turn Input
        m_vehicleTurn = horizontalInput;


    }

    public void ShootWater()
    {
        
        m_vehicleUsageLife -= 1.0f * Time.deltaTime;

        if (m_vehicleUsageLife<=0)
        {
            //TODO kick out player
        }
    }
   
}

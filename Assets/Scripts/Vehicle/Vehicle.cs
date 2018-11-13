using UnityEngine;

public enum EVehicleStatus
{
    IDLE, AcceleratingFoward, AcceleratingBack, DesceleratingFoward, DesceleratingBack, Brake
}


[RequireComponent(typeof(Rigidbody))]

public class Vehicle : MonoBehaviour {

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

    //Don't change this values on editor, just showed on editor for debug information
    [SerializeField]
    private float m_vehicleSpeed = 0.0f;
    [SerializeField]
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
        m_vehicleRigibody.velocity = gameObject.transform.forward * m_vehicleSpeed;
        //m_vehicleRigibody.velocity += Physics.gravity;
    }


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
   
}

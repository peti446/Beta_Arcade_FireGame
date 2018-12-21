using UnityEngine;
using UnityEngine.Networking;

public enum EVehicleStatus
{
    IDLE, AcceleratingFoward, AcceleratingBack, DesceleratingFoward, DesceleratingBack, Brake
}


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Interact))]
public class Vehicle : NetworkBehaviour {
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
    [SerializeField]
    private GameObject m_cameraPivot;

    [SyncVar(hook = "OnControllingPlayerIDUpdated")]
    private int m_controllingPlayerID = -1;

    private Rigidbody m_vehicleRigibody;
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

    public override void OnStartServer()
    {
        base.OnStartServer();
        Interact interactComponent = GetComponent<Interact>();
        if (interactComponent != null)
        {
            interactComponent.ServerInteraction.AddListener(OnCharacterWantsToEnterVheicle);
        }
    }

    private void FixedUpdate()
    {
        UpdateVehicleVelocity();
    }

    private void UpdateVehicleVelocity()
    {
        switch (State)
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
        m_vehicleRigibody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

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

        if (m_vehicleUsageLife <= 0)
        {
            //TODO kick out player


        }
    }

    [Client]
    public void ExitVehicle()
    {
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        camera.transform.SetParent(null);
        CmdPlayerExit();
    }

    [Command]
    public void CmdPlayerExit()
    {
        NetworkPlayer p = MainNetworkManager._instance.PlayersConnected[m_controllingPlayerID];
        //Revoke autority
        GetComponent<NetworkIdentity>().RemoveClientAuthority(p.connectionToClient);
        //Make the caracter visislbe
        Character[] allCharacters = FindObjectsOfType<Character>();
        foreach(Character c in allCharacters)
        {
             if(c.ControllingPlayerID == m_controllingPlayerID)
             {
                //Update the position and set active
                c.TargetUpdatePos(p.connectionToClient, transform.position, transform.rotation);
                c.RpcSetCaracterActive(true);
             }
        }

        //Update id
        m_controllingPlayerID = -1;
        NetworkServer.Destroy(gameObject);
    }

    private void OnControllingPlayerIDUpdated(int newPlayerID)
    {
        //Set the new player controller
        m_controllingPlayerID = newPlayerID;

        //Check if it is the local player, if so enable the input, otherwise disable it
        NetworkPlayer p = MainNetworkManager._instance.LocalPlayer;
        Debug.Log(newPlayerID);
        Debug.Log(p);
        Debug.Log(p.Player_ID);
        if(p != null && p.Player_ID == newPlayerID)
        {
            GetComponent<VehicleInputs>().enabled = true;
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            camera.transform.SetParent(m_cameraPivot.transform, false);
            camera.transform.position = m_cameraPivot.transform.position;
            camera.transform.rotation = m_cameraPivot.transform.rotation;
        }
        else
        {
            GetComponent<VehicleInputs>().enabled = false;
        }
    }

    [Server]
    private void OnCharacterWantsToEnterVheicle(Character c)
    {
        //Get the players team
        if(ETeams.FireFighters == MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].Player_Team
            && GetComponent<NetworkIdentity>().clientAuthorityOwner == null)
        {
            //We are fire fighters so we can got up to the vheicle, give the player authorty
            GetComponent<NetworkIdentity>().AssignClientAuthority(
                MainNetworkManager._instance.PlayersConnected[c.ControllingPlayerID].connectionToClient
            );

            //Disable the character on all clients
            c.RpcSetCaracterActive(false);

            //Set the current controling player
            m_controllingPlayerID = c.ControllingPlayerID;

            Debug.Log("On character want to enter:" + m_controllingPlayerID);
        }
    }
   
}

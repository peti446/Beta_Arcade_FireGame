using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestPlaneSpawnVehicles : MonoBehaviour {

    public GameObject ESTOESUNAVARIABLE;

    public void Start()
    {
        StartCoroutine(CoroutineUtilities.DelaySeconds(() => { if (!MainNetworkManager.Is_Server) return; GameObject o = Instantiate(ESTOESUNAVARIABLE, new Vector3(0, 15, 0), Quaternion.identity); NetworkServer.Spawn(o); }, 5));
    }
}

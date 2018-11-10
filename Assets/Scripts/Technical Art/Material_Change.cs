using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Material_Change : MonoBehaviour {

    [Range(0,4)]
    public int CurrentMaterial;
    public List<Material> Materials;
    int tempint;


    private void OnDrawGizmos()
    {
        if (tempint != CurrentMaterial) ChangeMaterial();
    }

    void ChangeMaterial()
    {
        if (CurrentMaterial >= Materials.Count) { Debug.Log("You dont have that many textures bro"); CurrentMaterial = 0; return; }
        this.gameObject.GetComponent<Renderer>().material = Materials[CurrentMaterial];
        tempint = CurrentMaterial;
    }
}

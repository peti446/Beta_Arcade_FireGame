using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Material_Change : MonoBehaviour {

    [Range(0, 4)]
    public int CurrentMaterial;
    public bool isComplex = false;
    public List<Material> Materials;
    int tempint;


    private void OnDrawGizmos()
    {
        if (tempint != CurrentMaterial)
        {
            if (CurrentMaterial >= Materials.Count) { Debug.Log("You dont have that many textures bro"); CurrentMaterial = 0; }
            else { if (!isComplex) {ChangeMaterial(); } else ChangeComplexMaterials(); }
        }
    }

    void ChangeMaterial()
    {
        this.gameObject.GetComponent<Renderer>().material = Materials[CurrentMaterial];
        tempint = CurrentMaterial;
    }

    void ChangeComplexMaterials()
    {
        Renderer[] children;
        children = GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in children)
        {
            rend.material= Materials[CurrentMaterial];
            /*var mats = new Material[rend.sharedMaterials.Length];
            for (int j = 0; j < rend.sharedMaterials.Length; j++)
            {
                mats[j] = Materials[CurrentMaterial];

            }
            rend.sharedMaterials = mats;*/
        }
        tempint = CurrentMaterial;
    }
}



         

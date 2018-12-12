using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "MapBuildingsPrefabsList", order = 1)]
public class MapBuildings : ScriptableObject
{
	public GameObject[] Buildings1x1;
	public GameObject[] Buildings2x1;
	public GameObject[] Buildings1x2;
	public GameObject[] Buildings2x2;
	public GameObject FireStation;
	public GameObject Road;
}

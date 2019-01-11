using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "MapInfo", order = 1)]
public class MapInfo : ScriptableObject
{
	public string Name;
	public Sprite Mapsprite;
}

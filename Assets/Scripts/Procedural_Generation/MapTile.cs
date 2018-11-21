using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETileType
{
	Size1x1, Size2x1, Size1x2, Size2x2, Random
}

public class MapTile : MonoBehaviour {

	[HideInInspector]
	[SerializeField]
	protected int GridX;
	[HideInInspector]
	[SerializeField]
	protected int GridZ;

	protected readonly ETileType m_tileType = ETileType.Size1x1;
	public ETileType TileType
	{
		get
		{
			return m_tileType;
		}
	}
}

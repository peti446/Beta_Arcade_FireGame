using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETileType
{
	Size1x1, Size2x1, Size1x2, Size2x2, Random, Road
}

public class MapTile : MonoBehaviour {

	[HideInInspector]
	[SerializeField]
	protected int GridX;
	[HideInInspector]
	[SerializeField]
	protected int GridZ;
	[HideInInspector]
	[SerializeField]
	protected ETileType m_tileType = ETileType.Size1x1;
	public ETileType TileType
	{
		get
		{
			return m_tileType;
		}
	}

	private bool m_isSettetUp = false;

	public virtual void EditorEnabled()
	{
		if(!m_isSettetUp)
		{
			Setup();
			m_isSettetUp = true;
		}
	}

	protected virtual void Setup()
	{
		m_tileType = ETileType.Size1x1;
	}
}

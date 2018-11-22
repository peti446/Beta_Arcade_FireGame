using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETileType
{
	Size1x1, Size2x1, Size1x2, Size2x2, Random, Road, Firestation
}

public class MapTile : MonoBehaviour {

	[HideInInspector]
	[SerializeField]
	protected int m_gridX;
	[HideInInspector]
	[SerializeField]
	protected int m_gridZ;
	[HideInInspector]
	[SerializeField]
	protected ETileType m_tileType = ETileType.Size1x1;
	public ETileType TileType
	{
		get
		{
			return m_tileType;
		}
		set
		{
			m_tileType = value;
		}
	}
	public int GridX
	{
		get { return m_gridX; }
	}
	public int GridZ
	{
		get { return m_gridZ; }
	}

	//To controll the setup
	private bool m_isSettetUp = false;
	/// <summary>
	/// Called on the editor script when its enabled
	/// </summary>
	public void EditorEnabled()
	{
		//We only want to set up the class once
		if(!m_isSettetUp)
		{
			//Set the correct grid based on current position
			GetGridFromPosition(out m_gridX, out m_gridZ);
			//Set the correct scale
			gameObject.transform.localScale = GetTileSize();
			//Setup and make sure we dont do it again
			Setup();
			m_isSettetUp = true;
		}
	}

	/// <summary>
	/// Function called once during it lifetime in the editor, called when the editor is enables.
	/// </summary>
	protected virtual void Setup(){}

	/// <summary>
	/// Gets the scale of the current tile
	/// </summary>
	/// <returns><c>Vector3</c> the scale of the tile</returns>
	public Vector3 GetTileSize()
	{
		return GetTileSize(TileType);
	}

	/// <summary>
	/// Gets the grid position from the current trasnform
	/// </summary>
	/// <param name="x"><c>out</c> the x coordinate of the grid</param>
	/// <param name="z"><c>out</c> the z coordinate of the grid</param>
	public void GetGridFromPosition(out int x, out int z)
	{
		//Get the size and current pos of the tile
		Vector3 size = GetTileSize();
		Vector3 pos = transform.position;
		//Substract the nececary amount if the size is a mutiple of 2, wich will make the center be off the grid by 5 units
		pos -= new Vector3((((size.x - 1) % 2) * 5), 0, (((size.z - 1) % 2) * 5));
		//Divide by the cell size witch is 10 to get the current cell 
		x = Mathf.CeilToInt(pos.x * 0.1f);
		z = Mathf.CeilToInt(pos.z * 0.1f);
	}

	/// <summary>
	/// Get the transform in world based on the grid coordinate
	/// </summary>
	/// <param name="x">The X coordinate of the grid</param>
	/// <param name="z">The Z coordinate of the grid</param>
	/// <returns><c>Vector3</c> position in world space where the object should be</returns>
	public Vector3 ConvertGridToPosition(int x, int z)
	{
		//Return using the static formula
		return ConvertGridToPosition(x,z,TileType);
	}

	/// <summary>
	/// Checks if a tile can be placed on the given position
	/// </summary>
	/// <param name="pos"><c>Vector3</c> The position to wich the tile wants to be setted</param>
	/// <returns><c>True</c> if the tile has space to move there, <c>false</c> otherwise</returns>
	public bool CanMoveToGridPos(Vector3 pos)
	{
		//Raycast over a box with the pos at the center and the tile size as the tile size as the box definition
		RaycastHit[] hits = Physics.BoxCastAll(pos + (Vector3.up * 20.0f), GetTileSize()*4.5f, Vector3.down, Quaternion.identity, 20.0f, 1 << 30);
		//Go trought all object hitted and check if there is another map tile there 
		foreach (RaycastHit hit in hits)
		{
			MapTile mt = hit.transform.gameObject.GetComponent<MapTile>();
			if (mt != null && mt != this)
			{
				//There is another tile so we cannot move there
				return false;
			}
		}
		//No tile found so we can move
		return true;
	}

	/// <summary>
	/// Checks if a tile can be rezised without interfiering with oter tiles around it 
	/// </summary>
	/// <param name="newTile">The new tile type</param>
	/// <returns><c>True</c> if the tile has space to change size, <c>false</c> otherwise</returns>
	public bool CanChangeSizeTo(ETileType newTile)
	{
		//Raycast over a box with the grid converted to position using the new tile type at the center and the tile size as the tile size as the box definition
		RaycastHit[] hits = Physics.BoxCastAll(ConvertGridToPosition(m_gridX, m_gridZ, newTile) + (Vector3.up * 20.0f), GetTileSize(newTile)*4.5f, Vector3.down, Quaternion.identity, 20.0f, 1 << 30);
		//Go trought all object hitted and check if there is another map tile there 
		foreach (RaycastHit hit in hits)
		{
			MapTile mt = hit.transform.gameObject.GetComponent<MapTile>();
			if (mt != null && mt != this)
			{
				//There is another tile so we cannot change
				return false;
			}
		}
		//No tile so we can change
		return true;
	}

	/// <summary>
	/// Gets the scale of a specific tile
	/// </summary>
	/// <param name="tile">The tile we want to know the size of</param>
	/// <returns><c>Vector3</c> the scale of the tile</returns>
	public static Vector3 GetTileSize(ETileType tile)
	{
		Vector3 size = Vector3.one;
		switch (tile)
		{
			case ETileType.Size1x2:
				size = new Vector3(1, 1, 2);
				break;
			case ETileType.Size2x1:
				size = new Vector3(2, 1, 1);
				break;
			case ETileType.Firestation:
			case ETileType.Size2x2:
				size = new Vector3(2, 1, 2);
				break;
		}
		return size;
	}

	/// <summary>
	/// Converts a gid coordinate with a tile size to a world transform
	/// </summary>
	/// <param name="x">The grid x coordinate</param>
	/// <param name="z">The grid z coordinate</param>
	/// <param name="tile">The tile type we want to convert from grid coord to world space position</param>
	/// <returns>World space position of the tile</returns>
	public static Vector3 ConvertGridToPosition(int x, int z, ETileType tile)
	{
		Vector3 size = GetTileSize(tile);
		return new Vector3((x * 10) + (((size.x - 1) % 2) * 5), 0, (z * 10) + (((size.z - 1) % 2) * 5));
	}

	/// <summary>
	/// Checks if a tile can be placed on the given position
	/// </summary>
	/// <param name="x">Grid X position</param>
	/// <param name="z">Griz Z pos</param>
	/// <param name="tile">The type of the tile</param>
	/// <returns><c>True</c> if there is space to move a tile there, <c>false</c> otherwise</returns>
	public static bool CanMoveToGridPos(int x, int z, ETileType tile)
	{
		//Raycast over a box with the pos at the center and the tile size as the tile size as the box definition
		RaycastHit[] hits = Physics.BoxCastAll(ConvertGridToPosition(x, z, tile) + (Vector3.up * 20.0f), GetTileSize(tile) * 4.5f, Vector3.down, Quaternion.identity, 20.0f, 1 << 30);
		//Go trought all object hitted and check if there is another map tile there 
		foreach (RaycastHit hit in hits)
		{
			MapTile mt = hit.transform.gameObject.GetComponent<MapTile>();
			if (mt != null)
			{
				//There is another tile so we cannot move there
				return false;
			}
		}
		//No tile found so we can move
		return true;
	}
}

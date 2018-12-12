using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
	/// Get the current object that will be positioned onto the tile
	/// </summary>
	/// <returns>The building/road game object</returns>
	protected virtual GameObject GetTileObject(ProceduralMapManager proceduralManager)
	{
		//Current orientation for this tile
		Vector2 orientation =Vector2.zero;


		//Directions to search for roads
		Vector2[] dirs = { new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1) };
		//Search in all directions
		foreach (Vector2 base_dir in dirs)
		{
			//multiply dir by the size to get the tiles next to us
			Vector2 dir = new Vector3(base_dir.x * GetTileSize().x, base_dir.y * GetTileSize().y);
			//Get the position the position of the tile in the current direction
			int newX = m_gridX + (int)dir.x;
			int newZ = m_gridZ + (int)dir.y;
			//Get the tite type
			ETileType e;
			if (GetTileAtPos(newX, newZ, out e))
			{
				if (e == ETileType.Road)
				{
					//We got a road so set orientation to the base dir so we can rotate it later
					orientation = base_dir;
					break;
				}
			}
		}

		//If we do not have an orientation just return as the no road is nearby
		if(orientation == Vector2.zero)
		{
			return null;
		}

		//Based on the tile type instanciate the correct object
		GameObject o = null;
		switch (m_tileType)
		{
			case ETileType.Size1x1:
				if (proceduralManager.MapBuildingsScripteableObject.Buildings1x1.Length > 0)
				{
					o = Instantiate(proceduralManager.MapBuildingsScripteableObject.Buildings1x1[Random.Range(0, proceduralManager.MapBuildingsScripteableObject.Buildings1x1.Length)]);
					o.transform.rotation = Quaternion.FromToRotation(-o.transform.right, o.transform.TransformDirection(new Vector3(orientation.x, 0, orientation.y)));
				}
				break;
			case ETileType.Size1x2:
			case ETileType.Size2x1:
				if (proceduralManager.MapBuildingsScripteableObject.Buildings1x2.Length > 0)
				{
					o = Instantiate(proceduralManager.MapBuildingsScripteableObject.Buildings1x2[Random.Range(0, proceduralManager.MapBuildingsScripteableObject.Buildings1x2.Length)]);
					o.transform.rotation = Quaternion.FromToRotation(-o.transform.right, o.transform.TransformDirection(new Vector3(orientation.x, 0, orientation.y)));
				}
				break;
			case ETileType.Size2x2:
				if (proceduralManager.MapBuildingsScripteableObject.Buildings2x2.Length > 0)
				{
					o = Instantiate(proceduralManager.MapBuildingsScripteableObject.Buildings2x2[Random.Range(0, proceduralManager.MapBuildingsScripteableObject.Buildings2x2.Length)]);
					o.transform.rotation = Quaternion.FromToRotation(-o.transform.right, o.transform.TransformDirection(new Vector3(orientation.x, 0, orientation.y)));
				}
				break;
			case ETileType.Firestation:
				o = Instantiate(proceduralManager.MapBuildingsScripteableObject.FireStation);
				o.transform.rotation = Quaternion.FromToRotation(-o.transform.right, o.transform.TransformDirection(new Vector3(orientation.x, 0, orientation.y)));
				break;
		}
		return o;
	}

	/// <summary>
	/// Spawns a preview of the object
	/// </summary>
	public void SpawnPreviewBuilding()
	{
		ProceduralMapManager proceduralManager = ProceduralMapManager._instance;
		//If there is no manager return as we cannot spawn any object so just return
		if (proceduralManager == null)
		{
#if UNITY_EDITOR
			if (!EditorApplication.isPlaying)
			{
				proceduralManager = GameObject.FindObjectOfType<ProceduralMapManager>();
			}
			else
			{
#endif
			Debug.LogError("Cannot spawn buildins without a procedural manager");
			return;
#if UNITY_EDITOR
			}
#endif
		}
		//Get the tile object to place it in the world
		GameObject tileObject = GetTileObject(proceduralManager);
		if(tileObject != null)
		{
			//Mark the object as a preview
			tileObject.tag = "PreviewBuilding";
			//Set the position of the object to the tile one
			tileObject.transform.position = transform.position;
		}
	}
	/// <summary>
	/// Spawns the building and removes the tile
	/// </summary>
	public void SpawnBuilding()
	{
		ProceduralMapManager proceduralManager = ProceduralMapManager._instance;
		//If there is no manager return as we cannot spawn any object so just return
		if (proceduralManager == null)
		{
#if UNITY_EDITOR
			if (!EditorApplication.isPlaying)
			{
				proceduralManager = GameObject.FindObjectOfType<ProceduralMapManager>();
			}
			else
			{
#endif
				Debug.LogError("Cannot spawn buildins without a procedural manager");
				return;
#if UNITY_EDITOR
			}
#endif
		}
		//Get the object that should be placed in this world
		GameObject tileObject = GetTileObject(proceduralManager);
		if (tileObject != null)
		{
			//Set the position of the object to the tile one
			tileObject.transform.position = transform.position;

			//Destroy the tile
			Destroy(gameObject);
		}
	}

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

	/// <summary>
	/// Get the tile at a certan position
	/// </summary>
	/// <param name="x">X position</param>
	/// <param name="z">Z position</param>
	/// <param name="type">the tile type we found in the position, if there is any</param>
	/// <returns>true if there is a tile type, false if there is none</returns>
	public static bool GetTileAtPos(int x, int z, out ETileType type)
	{
		type = ETileType.Size1x1;
		//Raycast to the position
		RaycastHit[] hits = Physics.BoxCastAll(ConvertGridToPosition(x, z, ETileType.Size1x1) + (Vector3.up * 20.0f), GetTileSize(ETileType.Size1x1) * 4.5f, Vector3.down, Quaternion.identity, 20.0f, 1 << 30);
		//Go trought all object hitted and check if there is another map tile there 
		foreach (RaycastHit hit in hits)
		{
			MapTile mt = hit.transform.gameObject.GetComponent<MapTile>();
			if (mt != null)
			{
				//There is another tile return true and the tile type
				type = mt.TileType;
				return true;
			}
		}
		return false;
	}
}

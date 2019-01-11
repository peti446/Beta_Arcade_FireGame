using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProceduralMapManager : MonoBehaviour {
	[SerializeField]
	private MapBuildings m_mapBuildingsScripteableObject;
	/// <summary>
	/// The scripteable object with all the data for spawning the objects
	/// </summary>
	public MapBuildings MapBuildingsScripteableObject
	{
		get
		{
			return m_mapBuildingsScripteableObject;
		}
	}

	//Data abotut tiles, the list of all tiles in the world, and a count per tile type
	private List<MapTile> m_mapTiles = new List<MapTile>();
	private IDictionary<ETileType, int> m_tileCount = new Dictionary<ETileType, int>();

	/// <summary>
	/// Instance of the manager
	/// </summary>
	public static ProceduralMapManager _instance
	{
		get;
		private set;
	}

	//Instance handling
	private void Awake()
	{
		if (_instance != null)
			Destroy(gameObject);

		_instance = this;
	}

	private void OnDestroy()
	{
		if (_instance == this)
			_instance = null;
	}


	//Refresh the list when we start the game
	private void Start()
	{
		RefreshTileList();
	}

	/// <summary>
	/// Refreshes the internal list of objects and orders it based on x and z grid cord
	/// </summary>
	public void RefreshTileList()
	{
		//Clear the map
		m_tileCount.Clear();
		m_mapTiles.Clear();

		//Get all MapTiles in the world
		MapTile[] allTiles = GameObject.FindObjectsOfType<MapTile>();

		//Incrementt the count for each tile type
		foreach (MapTile t in allTiles)
		{
			if (!m_tileCount.ContainsKey(t.TileType))
			{
				m_tileCount[t.TileType] = 1;
			}
			else
			{
				m_tileCount[t.TileType] += 1;
			}
		}

		//Add the tile object to the list and sort it based on x and z pos
		m_mapTiles.AddRange(allTiles);
		m_mapTiles.Sort((a, b) =>
		{
			if (a.GridX > b.GridX)
			{
				return 1;
			}
			else if (a.GridX < b.GridX)
			{
				return -1;
			}
			else
			{
				if (a.GridZ > b.GridZ)
				{
					return 1;
				}
				else if (a.GridZ < b.GridZ)
				{
					return -1;
				}

				return 0;
			}
		});
	}

	/// <summary>
	/// Get the count of a tile typle
	/// </summary>
	/// <param name="tile">The tile type we want to get a count of</param>
	/// <returns>Tile count</returns>
	public int GetTileCount(ETileType tile)
	{
		return m_tileCount.ContainsKey(tile) ? m_tileCount[tile] : 0;
	}

	/// <summary>
	/// Deletes the peview city from the scene
	/// </summary>
	public void DeleteCityPreview()
	{
		//Get allobjects of type preview
		GameObject[] allPreviews = GameObject.FindGameObjectsWithTag("PreviewBuilding");
		//Delete all of them
		foreach (GameObject go in allPreviews)
		{
			//Diferent destroy types if we are in editor playing or editing, also for otimisation, if we are not in editor aka a build just remove the unecesary if statment
#if UNITY_EDITOR
			if (!EditorApplication.isPlaying)
				DestroyImmediate(go.gameObject);
			else
				Destroy(go.gameObject);
#else
				Destroy(go.gameObject);
#endif
		}
	}

	/// <summary>
	/// Generates a Preview city
	/// </summary>
	public void GenerateCityPreview()
	{
		//Refresl the list then spawn the preview
		RefreshTileList();
		foreach (MapTile mt in m_mapTiles)
		{
			mt.SpawnPreviewBuilding();
		}
	}

	/// <summary>
	/// Generates a static city
	/// </summary>
	public void GenerateCity()
	{
		//Refreshes the list and then spawn the buildings
		RefreshTileList();
		foreach (MapTile mt in m_mapTiles)
		{
			mt.SpawnBuilding();
		}
	}

}

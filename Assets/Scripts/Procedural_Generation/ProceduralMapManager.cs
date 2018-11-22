using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMapManager : MonoBehaviour {

	[SerializeField]
	public GameObject[] m_spawneableFireStations;
	[SerializeField]
	public GameObject[] m_spawneableRoadsBuildings;
	[SerializeField]
	public GameObject[] m_spawneable1x1Buildings;
	[SerializeField]
	public GameObject[] m_spawneable1x2Buildings;
	[SerializeField]
	public GameObject[] m_spawneable2x1Buildings;
	[SerializeField]
	public GameObject[] m_spawneadble2x2Buildings;
	private List<MapTile> m_mapTiles = new List<MapTile>();
	private IDictionary<ETileType, int> m_tileCount = new Dictionary<ETileType, int>();

	public static ProceduralMapManager _instance
	{
		get;
		private set;
	}

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

	public int GetTileCount(ETileType tile)
	{
		return m_tileCount.ContainsKey(tile) ? m_tileCount[tile] : 0;
	}

	public void DeleteCityPreview()
	{
		GameObject[] allPreviews = GameObject.FindGameObjectsWithTag("PreviewBuilding");
		foreach (GameObject go in allPreviews)
		{
			DestroyImmediate(go);
		}
	}

	public void GenerateCityPreview()
	{
		RefreshTileList();
		Debug.Log("Generating: " + m_mapTiles.Count);
		foreach (MapTile mt in m_mapTiles)
		{
			mt.SpawnPreviewBuilding();
		}
	}

	private void GenerateCity()
	{

	}

}

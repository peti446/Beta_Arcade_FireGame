using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMapManager : MonoBehaviour {

	private List<MapTile> m_mapTiles = new List<MapTile>();
	private IDictionary<ETileType, int> m_tileCount = new Dictionary<ETileType, int>();

	public void Start()
	{
		RefreshTileList();
	}

	public void RefreshTileList()
	{
		m_tileCount.Clear();
		m_mapTiles.Clear();

		//Get all MapTiles in the world and sort it
		MapTile[] allTiles = Component.FindObjectsOfType<MapTile>();
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

	}

	public void DeletePreviewCity()
	{

	}

	private void GenerateCity()
	{

	}

}

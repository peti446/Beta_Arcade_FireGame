using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapEditor_Window : EditorWindow {

	private ETileType m_newTile = ETileType.Size1x1;
	private int m_gridX, m_gridZ = 0;
	private IDictionary<ETileType, GameObject> m_prefabs = new Dictionary<ETileType, GameObject>();
	private ProceduralMapManager m_MapManager = null;

	//Directions to try to find a new path
	private readonly short[] dirX = { 1,0,-1,0,1,-1,1,-1 };
	private readonly short[] dirZ = { 0,1,0,-1,1,1,-1,-1 };

	[MenuItem("Custom tools/Map Tile Controller")]
	public static void CreateWindow()
	{
		GetWindow<MapEditor_Window>("Map Tile Controller");
	}

	private void PopulatePrefabsList()
	{
		m_prefabs.Clear();
		m_prefabs.Add(ETileType.Size1x1, AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProceduralTiles/1x1.prefab"));
		m_prefabs.Add(ETileType.Size1x2, AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProceduralTiles/1x2.prefab"));
		m_prefabs.Add(ETileType.Size2x1, AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProceduralTiles/2x1.prefab"));
		m_prefabs.Add(ETileType.Size2x2, AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProceduralTiles/2x2.prefab"));
		m_prefabs.Add(ETileType.Firestation, AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProceduralTiles/Firestation.prefab"));
		m_prefabs.Add(ETileType.Random, AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProceduralTiles/Random.prefab"));
		m_prefabs.Add(ETileType.Road, AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProceduralTiles/Road.prefab"));
	}

	private void OnGUI()
	{
		if (m_prefabs.Count == 0)
			PopulatePrefabsList();

		if(m_MapManager == null)
		{
			m_MapManager = GameObject.FindObjectOfType<ProceduralMapManager>();
			if(m_MapManager == null)
			{
				GUI.Label(new Rect(0, 0, 300, 20), "No ProceduralMapManager Detected in scene!", EditorStyles.boldLabel);
				GUI.Label(new Rect(5, 20, 300, 100), "You cannot use the ProceduralMapTool if the map is not flagged as procedural. Would you like to flag it?", EditorStyles.wordWrappedLabel);

				if(GUI.Button(new Rect(5,55,300,20), "Flag it!"))
				{
					Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ProceduralTiles/ProceduralMapManager.prefab"));
				}

				return;
			}
		}

		m_MapManager.RefreshTileList();

		GUI.Label(new Rect(0, 0, 150, 20), "Tile Spawn", EditorStyles.boldLabel);

		m_newTile = (ETileType)EditorGUI.EnumPopup(new Rect(15, 20, 300, 20), "Tile Type", m_newTile);
		m_gridX = EditorGUI.IntField(new Rect(15, 40, 300, 15), "Grid X Pos", m_gridX);
		m_gridZ = EditorGUI.IntField(new Rect(15, 60, 300, 15),"Grid Z Pos", m_gridZ);

		if(!MapTile.CanMoveToGridPos(m_gridX, m_gridZ, m_newTile))
		{
			IList<KeyValuePair<int, int>> m_cellsToView = new List<KeyValuePair<int, int>>();
			m_cellsToView.Add(new KeyValuePair<int, int>(m_gridX, m_gridZ));
			while (m_cellsToView.Count > 0)
			{
				int x = m_cellsToView[0].Key;
				int z = m_cellsToView[0].Value;
				m_cellsToView.RemoveAt(0);
				for (short i = 0; i < 8; i++)
				{
					if (MapTile.CanMoveToGridPos(x + dirX[i], z + dirZ[i], m_newTile))
					{
						m_gridX = x + dirX[i];
						m_gridZ = z + dirZ[i];
						m_cellsToView.Clear();
						break;
					} else
					{
						m_cellsToView.Add(new KeyValuePair<int, int>(x + dirX[i], z + dirZ[i]));
					}
				}
			}
		}

		GUI.enabled = m_newTile == ETileType.Firestation ? m_MapManager.GetTileCount(m_newTile) == 0 : true;
		//Buttons
		if (GUI.Button(new Rect(15, 80, 300, 15), "Spawn tile", EditorStyles.miniButton))
		{
			if (m_prefabs[m_newTile] != null)
			{
				GameObject o = Instantiate(m_prefabs[m_newTile], MapTile.ConvertGridToPosition(m_gridX, m_gridZ, m_newTile), Quaternion.identity);
				MapTile mt = o.GetComponent<MapTile>();
				mt.TileType = m_newTile;
				Selection.activeGameObject = o;
				SceneView.lastActiveSceneView.LookAt(o.gameObject.transform.position);
			}
		}

		GUI.enabled = true;
		GUI.Label(new Rect(0, 100, 150, 20), "City Generation", EditorStyles.boldLabel);


		//Buttons
		if (GUI.Button(new Rect(15, 120, 150, 20), "Generate Preview City"))
		{
			m_MapManager.DeleteCityPreview();
			m_MapManager.GenerateCityPreview();
		}

		GUI.enabled = GameObject.FindGameObjectWithTag("PreviewBuilding") != null;
		//Buttons
		if (GUI.Button(new Rect(170, 120, 150, 20), "Remove Preview"))
		{
			m_MapManager.DeleteCityPreview();
		}

		GUI.enabled = true;
		//Buttons
		if (GUI.Button(new Rect(15, 145, 150, 20), "Generate City"))
		{

		}

		GUI.enabled = true;

		GUI.Label(new Rect(0, 175, 150, 20), "Tile Count", EditorStyles.boldLabel);
		Rect r = new Rect(5, 195, 150, 20);
		int i2 = 1;
		foreach (ETileType e in Enum.GetValues(typeof(ETileType)))
		{
			GUI.Label(r, Enum.GetName(e.GetType(), e) + ": " + m_MapManager.GetTileCount(e));
			if (i2%2 == 0)
			{
				r.x = 5;
				r.y += 15;
			} else
			{
				r.x = 175;
			}
			i2++;
		}
	}
}

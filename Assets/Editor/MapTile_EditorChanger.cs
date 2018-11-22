using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapTile), true)]
public class MapTile_EditorChanger : Editor
{
	private MapTile m_mapTileScript;
	private SerializedProperty m_gridX;
	private SerializedProperty m_gridZ;

	public void OnEnable()
	{
		//Get initial values
		m_mapTileScript = target as MapTile;
		m_gridX = serializedObject.FindProperty("GridX");
		m_gridZ = serializedObject.FindProperty("GridZ");
		m_mapTileScript.GetComponent<Renderer>().material.shader = Shader.Find("Standard");
		m_mapTileScript.EditorEnabled();
	}

	public override void OnInspectorGUI()
	{
		//Update
		serializedObject.Update();

		//Get the tile info user input
		ETileType tileType = ((ETileType)EditorGUILayout.EnumPopup("Tile Type", m_mapTileScript.TileType));
		
		//Get the user input
		int x = EditorGUILayout.IntField("Grid X", m_gridX.intValue);
		int z = EditorGUILayout.IntField("Grid Z", m_gridZ.intValue);


		//Check if we did change tile, if we did also check if we can change to the given size
		if (m_mapTileScript.TileType != tileType && m_mapTileScript.CanChangeSizeTo(tileType))
		{
			//Update scripts
			if (tileType == ETileType.Random && m_mapTileScript.TileType != ETileType.Random)
			{
				GameObject o = m_mapTileScript.gameObject;
				Component[] Components = m_mapTileScript.gameObject.GetComponents<MapTile>();
				foreach (Component c in Components)
				{
					DestroyImmediate(c);
				}
				o.transform.position = new Vector3(m_gridX.intValue * 10, 0, m_gridZ.intValue * 10);
				o.AddComponent<RandomMapTile>();
				EditorGUIUtility.ExitGUI();
			}
			else if (tileType != ETileType.Random && m_mapTileScript.TileType == ETileType.Random)
			{
				GameObject o = m_mapTileScript.gameObject;
				Component[] Components = m_mapTileScript.gameObject.GetComponents<RandomMapTile>();
				foreach (Component c in Components)
				{
					DestroyImmediate(c);
				}
				o.transform.position = new Vector3(m_gridX.intValue * 10, 0, m_gridZ.intValue * 10);
				MapTile mt = o.AddComponent<MapTile>();
				mt.TileType = tileType;
				EditorGUIUtility.ExitGUI();
			}

			//Save the tile type to the object
			m_mapTileScript.TileType = tileType;
		}

		//Change the tile color based on the current tile type
		switch (m_mapTileScript.TileType)
		{
			case ETileType.Size1x1:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.white);
				break;
			case ETileType.Size1x2:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.blue);
				break;
			case ETileType.Size2x1:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.cyan);
				break;
			case ETileType.Size2x2:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.green);
				break;
			case ETileType.Random:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.gray);
				break;
			case ETileType.Road:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.black);
				break;
			case ETileType.Firestation:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.red);
				break;
		}

		//Update the poosition in the grid
		SetPosToGrid(x, z);

		//Draw the default inspector for subclasses
		DrawDefaultInspector();
	}
	private void OnSceneGUI()
	{
		//Get the current grid from the position
		int gridX = 0;
		int gridZ = 0;
		m_mapTileScript.GetGridFromPosition(out gridX, out gridZ);

		//Update the map to be in the correct location
		SetPosToGrid(gridX, gridZ);
	}

	private void SetPosToGrid(int x, int z)
	{
		//New position in the world
		Vector3 newPos = m_mapTileScript.ConvertGridToPosition(x, z);
		Vector3 size = m_mapTileScript.GetTileSize();

		//Only make a ray cast and update the values if we moved
		if (x != m_gridX.intValue || z != m_gridZ.intValue) {
			//Check if we can actually move to the given grid position
			if(!m_mapTileScript.CanMoveToGridPos(newPos))
			{
				//If the space is used set the pos back
				m_mapTileScript.transform.position = m_mapTileScript.ConvertGridToPosition(m_gridX.intValue, m_gridZ.intValue);
				m_mapTileScript.transform.rotation = Quaternion.identity;
				m_mapTileScript.transform.localScale = size;
				return;
			}

			//Update the value inside the script
			m_gridX.intValue = x;
			m_gridZ.intValue = z;

			//Update the object
			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
		}

		//Set to the correct position and rotation will always be the same
		m_mapTileScript.transform.position = newPos;
		m_mapTileScript.transform.rotation = Quaternion.identity;
		m_mapTileScript.transform.localScale = size;
	}
}

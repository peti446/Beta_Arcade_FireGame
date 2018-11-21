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
		serializedObject.Update();
		//Get the user input
		int x = EditorGUILayout.IntField("Grid X", m_gridX.intValue);
		int z = EditorGUILayout.IntField("Grid Z", m_gridZ.intValue);
		//Update the poosition in the grid
		SetPosToGrid(x, z);

		//Draw the tile type for debuging
		string label = "1x1";
		switch(m_mapTileScript.TileType)
		{
			case ETileType.Size1x1:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.white);
				break;
			case ETileType.Size1x2:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.blue);
				label = "1x2";
				break;
			case ETileType.Size2x1:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.cyan);
				label = "2x1";
				break;
			case ETileType.Size2x2:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.green);
				label = "2x2";
				break;
			case ETileType.Random:
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.gray);
				label = "Random";
				break;
			case ETileType.Road:
				label = "Road";
				m_mapTileScript.GetComponent<Renderer>().material.SetColor(Shader.PropertyToID("_Color"), Color.black);
				break;
		}
		EditorGUILayout.LabelField("Tile Type", label);

		DrawDefaultInspector();
	}
	private void OnSceneGUI()
	{
		//Get the current pos in the map
		Vector3 pos = m_mapTileScript.transform.position;
		int gridX = Mathf.CeilToInt(pos.x * 0.1f);
		int gridZ = Mathf.CeilToInt(pos.z * 0.1f);

		//Update the map to be in the correct location
		SetPosToGrid(gridX, gridZ);
	}

	private void SetPosToGrid(int x, int z)
	{
		//Only make a ray cast and update the values if we moved
		if (x != m_gridX.intValue || z != m_gridZ.intValue) {
			//Check if we can actually move to the given grid position
			RaycastHit[] hits = Physics.RaycastAll(new Vector3(x * 10, 10, z * 10), Vector3.down, 15.0f, 1 << 30);
			foreach (RaycastHit hit in hits)
			{
				MapTile mt = hit.transform.gameObject.GetComponent<MapTile>();
				if (mt != null && mt != m_mapTileScript)
				{
					//Somone is already in the given position so just roll back to the last given values
					m_mapTileScript.transform.position = new Vector3(m_gridX.intValue * 10, 0, m_gridZ.intValue * 10);
					return;
				}
			}

			//Update the value inside the script
			m_gridX.intValue = x;
			m_gridZ.intValue = z;

			//Update the object
			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
		}

		//Set to the correct position and rotation will always be the same
		m_mapTileScript.transform.position = new Vector3(x * 10, 0, z * 10);
		m_mapTileScript.transform.rotation = Quaternion.identity;
		//Set the correct size for the tile based on the tyle type
		Vector3 size = Vector3.one;
		switch(m_mapTileScript.TileType)
		{
			case ETileType.Size1x2:
				size = new Vector3(1, 1, 2);
				break;
			case ETileType.Size2x1:
				size = new Vector3(2, 1, 1);
				break;
			case ETileType.Size2x2:
				size = new Vector3(2, 1, 2);
				break;
		}
		m_mapTileScript.transform.localScale = size;
	}
}

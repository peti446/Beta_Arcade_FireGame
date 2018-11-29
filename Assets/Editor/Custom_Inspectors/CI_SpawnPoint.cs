using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnPoint))]
[CanEditMultipleObjects]
public class CI_SpawnPoint : Editor
{
	private SerializedProperty m_isFireTruckSpawnSP;
	private SerializedProperty m_TeamSpawn;

	private void OnEnable()
	{
		m_isFireTruckSpawnSP = serializedObject.FindProperty("m_isFireTruckSpawn");
		m_TeamSpawn = serializedObject.FindProperty("m_spawnSide");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		m_TeamSpawn.enumValueIndex = (int)((ETeams)EditorGUILayout.EnumPopup("Team Spawn Type", (ETeams)m_TeamSpawn.enumValueIndex));

		if((ETeams)m_TeamSpawn.enumValueIndex == ETeams.FireFighters)
		{
			m_isFireTruckSpawnSP.boolValue = EditorGUILayout.Toggle("Is Truck spawn", m_isFireTruckSpawnSP.boolValue);
		}

		serializedObject.ApplyModifiedProperties();
	}

}

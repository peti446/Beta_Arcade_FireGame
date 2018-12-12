using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoadObject : MonoBehaviour
{
	public enum RoadObjectType
	{
		ClosedL,
		OpenL,
		ClosedT,
		ClosedEnd,
		OneSided,
		DoubleSided,
		Corner,
		Cross,
		Center
	}

	public enum RoadObjectOrientation : ushort
	{
		North = 0,
		East = 1,
		South = 2,
		West = 3
	}

	private static readonly IDictionary<RoadObjectType, GameObject> m_roadsGameObjects = new Dictionary<RoadObjectType, GameObject>();

	private void Awake()
	{
		m_roadsGameObjects.Clear();
		//Get all road types from the road kit
		foreach (Transform r in transform)
		{
			SubRoadObject subRoadObject = r.GetComponent<SubRoadObject>();
			if(subRoadObject != null)
			{
				//If the road type already exists destroy it otherwise keep it
				if (!m_roadsGameObjects.ContainsKey(subRoadObject.RoadObjectType))
				{
					m_roadsGameObjects.Add(subRoadObject.RoadObjectType, r.gameObject);
				}
			}
		}
	}

	public void SetRoadType(RoadObjectType roadType, RoadObjectOrientation targetOrientation)
	{

		GameObject roadGameObject = null;

#if UNITY_EDITOR
		//As the editor dont calls the awake we need to initialise it by ourselves
		if (!EditorApplication.isPlaying)
		{
			m_roadsGameObjects.Clear();
			//Get all road types from the road kit
			foreach (Transform r in transform)
			{
				SubRoadObject subRoadObject = r.GetComponent<SubRoadObject>();
				if (subRoadObject != null)
				{
					//If the road type already exists destroy it otherwise keep it
					if (!m_roadsGameObjects.ContainsKey(subRoadObject.RoadObjectType))
					{
						m_roadsGameObjects.Add(subRoadObject.RoadObjectType, r.gameObject);
					}
					else
					{
						DestroyImmediate(r.gameObject);
					}
				}

			}
		}
#endif

		//Delete all besides the selected type
		foreach (KeyValuePair<RoadObjectType, GameObject> roadMapObject in m_roadsGameObjects)
		{
			if (roadMapObject.Key != roadType)
			{
				//Diferent destroy types if we are in editor playing or editing, also for otimisation, if we are not in editor aka a build just remove the unecesary if statment
#if UNITY_EDITOR
				if (!EditorApplication.isPlaying)
					DestroyImmediate(roadMapObject.Value);
				else
					Destroy(roadMapObject.Value);
#else
				Destroy(roadObject.Value);
#endif
			}
			else
			{
				roadGameObject = roadMapObject.Value;
			}
		}

		//Now set the rotation of the object
		if(roadGameObject != null)
		{
			//Set relative transform to 0
			roadGameObject.transform.position = Vector3.zero;
			roadGameObject.transform.localPosition = Vector3.zero;
			//Cross and center dont need rotation
			if (roadType == RoadObjectType.Center || roadType == RoadObjectType.Cross)
			{
				return;
			}
			//Rotate
			Debug.Log(transform.name + " " + roadType.ToString() + " " + targetOrientation.ToString());
			transform.rotation *= Quaternion.AngleAxis((ushort)targetOrientation * 90.0f, transform.up);
		}
	}
}

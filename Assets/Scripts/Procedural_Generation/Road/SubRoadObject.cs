using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubRoadObject : MonoBehaviour
{
	[SerializeField]
	private RoadObject.RoadObjectType m_roadObjectType;

	public RoadObject.RoadObjectType RoadObjectType
	{
		get
		{
			return m_roadObjectType;
		}
	}
}

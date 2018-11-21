using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMapTile : MapTile
{
	[SerializeField]
	[Range(0.0f,100.0f)]
	private float m_chance1x1 = 25.0f;
	[SerializeField]
	[Range(0.0f, 100.0f)]
	private float m_chance1x2 = 25.0f;
	[SerializeField]
	[Range(0.0f, 100.0f)]
	private float m_chance2x1 = 25.0f;
	[SerializeField]
	[Range(0.0f, 100.0f)]
	private float m_chance2x2 = 25.0f;

	protected override void Setup()
	{
		m_tileType = ETileType.Random;
	}
}

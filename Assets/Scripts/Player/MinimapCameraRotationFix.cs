using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraRotationFix : MonoBehaviour
{
	private void LateUpdate()
	{
		//Make sure the rotation is always 90 no matter what
		gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
	}
}

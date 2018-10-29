using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour {

	CameraController c;
	
	void Start()
	{
		c = CameraController.Instance;
	}

	void LateUpdate () {
		transform.LookAt(c.GetCurrentCam());
	}
}

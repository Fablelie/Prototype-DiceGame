using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateParticleEffect : MonoBehaviour{

	public static void CreateFx(GameObject fx, Vector3 position)
	{
		var obj = Instantiate(fx);
		obj.transform.position = position;
	}

}

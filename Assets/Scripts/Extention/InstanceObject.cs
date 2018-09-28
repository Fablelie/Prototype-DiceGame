using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceObject<T> : MonoBehaviour where T : MonoBehaviour{

	private static T _instance;

	public static T Instance => _instance;

	protected void Awake() {
		if(_instance == null)
			_instance = GetComponent<T>();
	}
}

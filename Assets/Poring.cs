using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poring : MonoBehaviour {
	public Node node;
	public Animator animator;

	void Start () {
		animator = GetComponent<Animator>();
	}

	void HeadTo(Vector3 position) {
		transform.LookAt(position, transform.up);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCursor : MonoBehaviour {
	private Animator animator;
	static public MagicCursor Instance;
	void Start () {
		Instance = this;
		animator = GetComponent<Animator>();
	}
	
	public void MoveTo(Node node) {
		transform.parent = node.transform;
		transform.localPosition = new Vector3(0, 0.8f, 0);
		
		//animator.playbackTime = 0;
		animator.Play("magicCursorFocus");
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCursor : InstanceObject<MagicCursor> {
	private Animator animator;
    

    new void Awake () 
	{
		base.Awake();
		animator = GetComponent<Animator>();
	}
	
	public void MoveTo(Node node) {
        gameObject.SetActive(true);
		transform.parent = node.transform;
		transform.localPosition = new Vector3(0, 0.8f, 0);
		
		//animator.playbackTime = 0;
		animator.Play("magicCursorFocus");

		Invoke("SetDiable", 0.5f);
	}

	private void SetDiable()
	{
		gameObject.SetActive(false);
	}
}

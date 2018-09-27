using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapTitle : MonoBehaviour {

	public Text title1;
	public Text title2;
	public static MapTitle Instance;
	private Animator animator;
	void Awake() {
		Instance = this;
		animator = GetComponent<Animator>();
	}

	public void SetTitle (string title1, string title2) {
		this.title1.text = title1;
		this.title2.text = title2;
		animator.Play("ShowTitleFadeIn");
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {

	public float startTime = 3;
	public AudioClip[] sounds;
	private AudioSource audio;
	// Use this for initialization
	void Start () {
		audio = GetComponent<AudioSource>();
		audio.time = startTime;
		audio.Play();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

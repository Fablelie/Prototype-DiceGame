using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {

	public float startTime = 3;
	public AudioClip[] sounds;
	private AudioSource m_audio;
	// Use this for initialization
	void Start () {
		m_audio = GetComponent<AudioSource>();
		m_audio.time = startTime;
		m_audio.Play();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

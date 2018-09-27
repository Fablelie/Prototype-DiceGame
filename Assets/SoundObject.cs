using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundObject : MonoBehaviour {
	public bool loop = false;
	private bool isFadeIn = false;
	private bool isFadeOut = false;
	private bool onFadeOutDestroy = false;
	private float volume = 1.0f;
	public float MinVolume = 1.0f;
	public float MaxVolume = 1.0f;
	private float fadeSpeed = 0.05f;
	public Transform target;

	void FixedUpdate () {
		if (target != null) transform.position = target.position;
		if (loop == false)
		if (!GetComponent<AudioSource>().isPlaying && GetComponent<AudioSource>().clip != null) {
			Destroy (gameObject);
		}

		if (isFadeOut) {
			volume -= fadeSpeed;
			GetComponent<AudioSource>().volume = volume;
			if(volume <= MinVolume) {
				if(onFadeOutDestroy) Destroy (gameObject);
				if (volume <= 0) GetComponent<AudioSource>().Pause();
				isFadeOut = false;
			}
		}

		if (isFadeIn) {
			volume += fadeSpeed;
			GetComponent<AudioSource>().volume = volume;
			if(volume >= MaxVolume) {
				isFadeIn = false;
			}
		}
	}

	public void FadeOut(float speed = 0.05f, bool destroy = false, float min=0) {
		fadeSpeed = speed;
		isFadeOut = true;
		onFadeOutDestroy = destroy;
		MinVolume = min;
	}

	public void FadeIn(float speed = 0.05f, float max=1) {
		fadeSpeed = speed;
		isFadeIn = true;
		GetComponent<AudioSource>().UnPause();
		onFadeOutDestroy = false;
		MaxVolume = max;
	}

	public void ChangeVolume(float vol, float speed=0.05f) {
		if (vol > volume) FadeIn(speed, vol);
		if (vol < volume) FadeOut(speed, false, vol);
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour {
	public static GameObject SoundObject;

	void Start() {
		
	}

	public static SoundObject PlayClip(AudioClip clip, float volume = 1, bool loop = false, Transform t = null) {
		if (clip == null) return null;
		if (SFX.SoundObject == null) SFX.SoundObject = Resources.Load("SoundObject") as GameObject;print( SFX.SoundObject );
        if (t == null) t = Camera.main.transform;

        GameObject obj = Instantiate(SFX.SoundObject, t.position, t.rotation);
        if (t != Camera.main.transform) obj.GetComponent<AudioSource>().spatialBlend = 1f;
        obj.GetComponent<SoundObject>().target = t;
		obj.GetComponent<SoundObject>().loop = loop;
		obj.GetComponent<SoundObject>().MaxVolume = volume;
        obj.GetComponent<AudioSource>().volume = volume;
        obj.GetComponent<AudioSource>().clip = clip;
		obj.GetComponent<AudioSource>().loop = loop;
		obj.GetComponent<AudioSource>().Play();

		return obj.GetComponent<SoundObject>();
	}
}

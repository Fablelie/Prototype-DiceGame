using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraType {

	Default,
	TopDown,
}

public class CameraController : MonoBehaviour {
	public Camera camera;
	public Animator animator;
	public CameraType type;
	static public Transform target;
	public Vector3 offset;

	void Awake () {
		animator = GetComponent<Animator>();
		camera = GetComponent<Camera>();

		camera.enabled = false;
	}

	void Start () {
		// RaycastHit hit;
		// Ray ray = GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
		// if (Physics.Raycast(ray, out hit))
        // //if (Physics.Raycast(transform.position, Vector3.forward, out hit, Mathf.Infinity))
        // {
        //     offset = hit.point;
		// 	//offset.y = 0;
        //     Debug.Log("Did Hit");
        // }else Debug.Log("Not Hit");

		switch (type) {
			case CameraType.Default:
				camera.enabled = true;
			break;
			case CameraType.TopDown:
				camera.enabled = false;
			break;
		}    
	}

	public void SetTarget(Poring poring) {
		target = poring.transform;
	}

	public void Show(CameraType newType) {
		camera.enabled = false;
		if (this.type != newType) return;
		
		camera.enabled = true;
		Focus();
	}
	
	// Update is called once per frame
	void Update () {
		switch (type) {
			case CameraType.Default:
				if (target == null) return;
				transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, transform.position.y, target.position.z) - offset, Time.deltaTime);
			break;
			case CameraType.TopDown:
				if (Input.GetKeyDown(KeyCode.Z)) Focus();

				const float speed = 8;
				transform.Translate(Input.GetAxis("Horizontal") * speed * Time.deltaTime, Input.GetAxis("Vertical") * speed * Time.deltaTime, 0);
				//GetComponent<Klak.Motion.BrownianMotion>().UpdateTransform();
				//print(new Vector3(Input.GetAxis("Vertical") * speed * Time.deltaTime, 0, Input.GetAxis("Horizontal") * speed * Time.deltaTime));
			break;
		}  
	}

	void Focus() {
		switch (this.type) {
			case CameraType.TopDown:
				animator.Play("focus");
			break;
		}
	}
}

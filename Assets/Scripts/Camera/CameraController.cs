using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraType {

	Default,
	TopDown,
}

[System.Serializable]
public struct CameraData
{
	public Camera Camera;
	public CameraType Type;
	public Animator Animator;
}

public class CameraController : InstanceObject<CameraController> {

	public CameraData[] CameraDatas;
	public Transform Target;
	public CameraType CurrentType;

	[SerializeField] private float m_speed;

	public const float BridEyeViewSpeed = 8;

	public void SetTarget(Poring poring) {
		Target = poring.transform;
		
	}

	public void Show(CameraType type) 
	{	
		CurrentType = type;
		foreach (var data in CameraDatas)
		{
			data.Camera.enabled = (data.Type == type);
			
			if(type == CameraType.TopDown && data.Type == CameraType.TopDown)
				Focus(data.Animator);
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		switch (CurrentType) {
			case CameraType.Default:
				if (Target == null) return;
				// Move the rig towards target position.
				transform.position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime * m_speed);

			break;
			case CameraType.TopDown:
				// if (Input.GetKeyDown(KeyCode.Z)) Focus();
#if UNITY_ANDROID || UNITY_IOS
				CameraMove();
#endif

#if UNITY_EDITOR
				transform.Translate(Input.GetAxis("Horizontal") * BridEyeViewSpeed * Time.deltaTime, 0, Input.GetAxis("Vertical") * BridEyeViewSpeed * Time.deltaTime);
#endif
				//GetComponent<Klak.Motion.BrownianMotion>().UpdateTransform();
				//print(new Vector3(Input.GetAxis("Vertical") * speed * Time.deltaTime, 0, Input.GetAxis("Horizontal") * speed * Time.deltaTime));
			break;
		}  
	}

	Vector2 StartPosition;
	bool isTouch = false;
	private void CameraMove()
	{
		Debug.Log(Input.touchCount);
		Debug.Log(isTouch);

		if(Input.touchCount > 0 && !isTouch)
		{
			isTouch = true;
			StartPosition = Input.GetTouch(0).position;
		}
		else if(Input.touchCount <= 0)
		{
			isTouch = false;
		}

		if(isTouch)
		{
			var moveTo = (StartPosition - Input.GetTouch(0).position).normalized;
			transform.Translate((-moveTo.x * Time.deltaTime) * 2, 0, (-moveTo.y * Time.deltaTime) * 2);
		}
	}

	Vector2 GetWorldPosition()
    {
        return CameraDatas[1].Camera.ScreenToWorldPoint(Input.GetTouch(0).position);
    }
 

	private void Focus(Animator ani) 
	{
		ani.Play("focus");
	}
}

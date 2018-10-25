using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputDPad : MonoBehaviour {
	[SerializeField] private GameObject basePosPrefab;
	[SerializeField] private GameObject currentPosPrefab;

	[SerializeField] private GraphicRaycaster m_Raycaster;
	[SerializeField] private EventSystem m_EventSystem;
	[SerializeField] private PointerEventData m_PointerEventData;

	public void OnClickBtn()
	{
		Debug.LogError("OnClickBtn");
	}

	Vector3 basePos;
	Vector3 currentPos;
	bool isDPadDrawer = false;
	void LateUpdate()
	{
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			m_PointerEventData = new PointerEventData(m_EventSystem);
			m_PointerEventData.position = touch.position;

			List<RaycastResult> result = new List<RaycastResult>();

			m_Raycaster.Raycast(m_PointerEventData, result);
			switch (touch.phase)
			{
				case TouchPhase.Began:
					if(result[0].gameObject.layer == 8)
					{
						isDPadDrawer = true;
						basePos = touch.position;

						basePosPrefab.transform.position = basePos;
						currentPosPrefab.transform.position = basePos;
						basePosPrefab.SetActive(true);
						currentPosPrefab.SetActive(true);
					}
				break;
				case TouchPhase.Stationary:
				case TouchPhase.Moved:
					if(isDPadDrawer)
					{
						currentPosPrefab.transform.position = touch.position;
					}
				break;
				case TouchPhase.Ended:
					if(isDPadDrawer)
					{
						isDPadDrawer = false;
						basePosPrefab.SetActive(false);
						currentPosPrefab.SetActive(false);
					}
				break;
			}
		}
	}
}

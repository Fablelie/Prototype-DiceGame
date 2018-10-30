﻿using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum DiceType
{
	Move,
	Deffensive,
	Offensive,
}

public class Roll : MonoBehaviour {

	public DiceType Type;
	public GameObject image;
	public GameObject text;
	public GameObject arrow;

	public bool isRolling = false;
	public float rollSpeed = 0;
	private int radiusStep = 0;
	private List<GameObject> _texts = new List<GameObject>();
	private int _poringIndex;
	private int _resultIndex;
	
	public void SetRoll(List<FaceDice> valueList, int poringIndex, int resultIndex) {
		if (isRolling) return;
		_texts.Clear();
		gameObject.SetActive(true);
		_poringIndex = poringIndex;
		_resultIndex = resultIndex;
		radiusStep = 360/valueList.Count;
		for (int i = 0; i < valueList.Count; i++) {
			GameObject ob = Instantiate(text);
			ob.GetComponent<Text>().text = (int)valueList[i]+ "";
			ob.transform.SetParent(transform);
			ob.SetActive(true);
			// Vector3.ClampMagnitude(v, radiusStep*i);
			// Vector3.Magnitude();

			// Quaternion Rot = Quaternion.AngleAxis(-radiusStep * i, Vector3.forward);
         	// Vector3 Dir = Rot * Vector3.forward;
  			ob.GetComponent<RectTransform>().anchoredPosition = Quaternion.AngleAxis((radiusStep * i) - radiusStep, Vector3.forward) * Vector3.right * 100;
			// ob.GetComponent<RectTransform>().anchoredPosition = Dir;
			// print(Dir);
			// Vector3.
			_texts.Add(ob);
		}

		arrow.transform.Rotate(new Vector3(0, 0, Random.Range(0, 360)));
		rollSpeed = 32;
		isRolling = true;
	}

	void Start () {
		// SetRoll(6);
	}
	
	// Update is called once per frame
	void Update () {
		if (isRolling) {
			rollSpeed -= rollSpeed / 30;

			if (rollSpeed < 1) { 
				if (_resultIndex == RadiusToNumber() - 1)
				{
					rollSpeed = 0;
					Invoke ("OnRollEnd", 0.5f);
					isRolling = false;
				}
				else rollSpeed += 1;
			}

			arrow.transform.Rotate(new Vector3(0, 0, rollSpeed));
			//print(arrow.transform.eulerAngles.z);
			foreach (GameObject text in _texts) text.GetComponent<Text>().fontSize = 24;
			_texts[RadiusToNumber()-1].GetComponent<Text>().fontSize = 36;
		}

		// if (Input.GetKeyDown(KeyCode.R)) {
		// 	SetRoll(6);
		// }
	}


	int RadiusToNumber() {
		int i = Mathf.RoundToInt((arrow.transform.eulerAngles.z+(radiusStep/2)) / radiusStep);
		return i;
	}

	void OnRollEnd() {
		foreach (GameObject text in _texts) Destroy(text.gameObject);
		gameObject.SetActive(false);
		int index = RadiusToNumber()-1;
		//print("NUMBER IS " + number);
		if (_poringIndex == PhotonNetwork.LocalPlayer.GetPlayerNumber())
		{
			PrototypeGameMode.Instance.PhotonNetworkRaiseEvent(EventCode.RollEnd, new object[] { index, (int)Type, _poringIndex });
		}
		
		// PrototypeGameMode.Instance.OnRollEnd(number, Type);
		// PrototypeGameMode.Instance.photonView.RPC("OnRollEnd", RpcTarget.All, number, Type);
	}
}

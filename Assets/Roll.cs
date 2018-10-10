using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
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
	private List<FaceDice> _diceFacelist = new List<FaceDice>();
	private Poring _poring;
	
	public void SetRoll(List<FaceDice> valueList, Poring poring = null) {
		if (isRolling) return;
		_texts.Clear();
		gameObject.SetActive(true);
		_diceFacelist = valueList;
		_poring = poring;
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
				rollSpeed = 0;
				Invoke ("OnRollEnd", 0.5f);
				isRolling = false;
			}

			arrow.transform.Rotate(new Vector3(0, 0, rollSpeed));
			//print(arrow.transform.eulerAngles.z);
			foreach (GameObject text in _texts) text.GetComponent<Text>().fontSize = 24;
			_texts[RadiusToNumber()-1].GetComponent<Text>().fontSize = 48;
		}

		// if (Input.GetKeyDown(KeyCode.R)) {
		// 	SetRoll(6);
		// }
	}


	int RadiusToNumber() {
		return Mathf.RoundToInt((arrow.transform.eulerAngles.z+(radiusStep/2)) / radiusStep);
	}

	void OnRollEnd() {
		foreach (GameObject text in _texts) Destroy(text.gameObject);
		gameObject.SetActive(false);
		int index = RadiusToNumber()-1;
		//print("NUMBER IS " + number);
		object[] content = new object[] { number, (int)Type };
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All, };
		SendOptions sendOptions = new SendOptions{ Reliability = true};

		PhotonNetwork.RaiseEvent((byte)EventCode.RollEnd, content, raiseEventOptions, sendOptions);
		// PrototypeGameMode.Instance.OnRollEnd(number, Type);
		// PrototypeGameMode.Instance.photonView.RPC("OnRollEnd", RpcTarget.All, number, Type);
	}
}

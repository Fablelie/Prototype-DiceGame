using System.Collections;
using System.Collections.Generic;
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
	private List<GameObject> texts = new List<GameObject>();
	
	public void SetRoll(int maxNumber) {
		if (isRolling) return;
		texts.Clear();
		image.SetActive(true);
		radiusStep = 360/maxNumber;
		for (int i = 0; i < maxNumber; i++) {
			GameObject ob = Instantiate(text);
			ob.GetComponent<Text>().text = i + 1 + "";
			ob.transform.parent = transform;
			ob.SetActive(true);
			// Vector3.ClampMagnitude(v, radiusStep*i);
			// Vector3.Magnitude();

			// Quaternion Rot = Quaternion.AngleAxis(-radiusStep * i, Vector3.forward);
         	// Vector3 Dir = Rot * Vector3.forward;
  			ob.GetComponent<RectTransform>().anchoredPosition = Quaternion.AngleAxis((radiusStep * i) - radiusStep, Vector3.forward) * Vector3.right * 100;
			// ob.GetComponent<RectTransform>().anchoredPosition = Dir;
			// print(Dir);
			// Vector3.
			texts.Add(ob);
		}

		arrow.transform.Rotate(new Vector3(0, 0, Random.RandomRange(0, 360)));
		rollSpeed = 32;
		isRolling = true;
	}

	void Start () {
		// SetRoll(6);
	}
	
	// Update is called once per frame
	void Update () {
		if (isRolling) {
			rollSpeed -= rollSpeed / 48;

			if (rollSpeed < 1) { 
				rollSpeed = 0;
				Invoke ("OnRollEnd", 1);
				isRolling = false;
			}

			arrow.transform.Rotate(new Vector3(0, 0, rollSpeed));
			//print(arrow.transform.eulerAngles.z);
			foreach (GameObject text in texts) text.GetComponent<Text>().fontSize = 48;
			texts[RadiusToNumber()-1].GetComponent<Text>().fontSize = 64;
		}

		if (Input.GetKeyDown(KeyCode.R)) {
			SetRoll(6);
		}
	}


	int RadiusToNumber() {
		return Mathf.RoundToInt((arrow.transform.eulerAngles.z+(radiusStep/2)) / radiusStep);
	}

	void OnRollEnd() {
		foreach (GameObject text in texts) Destroy(text.gameObject);
		image.SetActive(false);
		int number = RadiusToNumber();
		//print("NUMBER IS " + number);
		GameMode.Instance.OnRollEnd(number, Type);

	}
}

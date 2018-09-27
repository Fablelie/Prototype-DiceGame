using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoringUI : MonoBehaviour {

	public Text Name;
	public Text Gold;
	public Scrollbar HP;

	public void Set(string name, float gold, float hp) {
		this.Name.text = name;
		this.Gold.text = "<color=\"#fff\">"+ Mathf.RoundToInt(gold) +" </color><b>G</b>";
		this.HP.value = hp / 100;
	}

}

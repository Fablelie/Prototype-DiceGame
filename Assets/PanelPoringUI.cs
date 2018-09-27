using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelPoringUI : MonoBehaviour {

	public GameObject Template;
	public List<PoringUI> PoringUI = new List<PoringUI>();

	public void Add(string name, float gold, float hp) {
		GameObject p = Instantiate(Template);
		p.transform.parent = transform;
		p.GetComponent<PoringUI>().Set(name, gold, hp);
	}

}

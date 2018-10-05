using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoringTPS : MonoBehaviour {

	// private Poring poring;
	private int m_moveStep = 0;
	private bool m_canMove = false;
	void Start () {
		// poring = GetComponent<Poring>();
	}

	void FixedUpdate() {
		m_moveStep++;
		if (m_moveStep > 60) m_canMove = true;
	}
	
	void Update () {
		
		if (!m_canMove) return;
		if (Input.GetKey(KeyCode.W)) {
			
		}
	}
}

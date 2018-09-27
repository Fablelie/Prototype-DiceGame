using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	public GameMode gameMode;
	public Map map;
	void Start() {
		gameMode.StartGameMode();
		MapTitle.Instance.SetTitle(map.Name, gameMode.Name);
	}
}

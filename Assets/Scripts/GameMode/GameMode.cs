using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameMode : MonoBehaviour {
	[Header("General Settings")]
    public string Name;
	public int MaxPlayer = 4;
	public int Turn;
	public bool isPause;


	//public List<Poring> porings = new List<Poring>();

	public abstract void StartGameMode();
	public abstract void UpdateGameMode();
	public abstract void OnRollEnd(int number);
	static public GameMode Instance;

	void Start() {
		Instance = this;
	}

	void Update() {
		UpdateGameMode();
	}

	void EndTurn() {

	}
	
}

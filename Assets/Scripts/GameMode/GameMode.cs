using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameMode : InstanceObject<GameMode>
{
	[Header("General Settings")]
    public string Name;
	public int MaxPlayer = 4;
	public int Turn;
	public bool isPause;

	public abstract void StartGameMode();
	public abstract void UpdateGameMode();
	public abstract void OnRollEnd(int number, DiceType type, Poring poring = null);

	void Update() {
		UpdateGameMode();
	}

	void EndTurn() {

	}
	
}

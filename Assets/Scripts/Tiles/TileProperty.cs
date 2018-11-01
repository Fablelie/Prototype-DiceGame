using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileProperty : ScriptableObject {
    public TileType Type;
    public int WeightStep;
	public Material Material;

	public virtual void OnEnter(Poring poring, Node node)
	{

	}

	public virtual void OnFinish(Poring poring, Node node)
	{

	}

	public virtual void OnExit(Poring poring, Node node)
	{

	}

	public virtual void OnEndRound()
	{

	}

}

public enum TileType
{
	Normal,
	Forest,
	Sand,
	Sanctuary,
	Posion,
	WarpIn,
	WarpOut
}
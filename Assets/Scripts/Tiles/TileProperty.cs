using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileProperty : ScriptableObject {
    public TileType Type;
    public int WeightStep;
	public Material Material;

}

public enum TileType
{
	Normal,
	Forest,
	Sand
}
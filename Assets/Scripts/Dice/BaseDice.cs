using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDice : ScriptableObject 
{
	public string DiceName;
	public List<FaceDice> FaceDiceList;

	public virtual int GetDiceFace(int index)
	{
		return (int)FaceDiceList[index];
	}
}

public enum FaceDice
{
	Miss = -1,
	Zero = 0,
	One = 1,
	Two = 2,
	Three = 3,
	Four = 4,
	Five = 5,
	Six = 6,
	Special = 7,
	SuperSpecial = 8,
	Max,
}
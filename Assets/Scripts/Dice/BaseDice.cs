using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDice : ScriptableObject 
{
	public string DiceName;
	public List<FaceDice> FaceDiceList;

	public virtual FaceDice GetDiceFace(int index)
	{
		return FaceDiceList[index];
	}

	public virtual int GetNumberFromDiceFace(FaceDice e)
	{
		if ((int)e >= (int)FaceDice.Miss && (int)e <= (int)FaceDice.Six)
		{
			return (int)e;
		}
		else
		{
			return 0;
		}
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
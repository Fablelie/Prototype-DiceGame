﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Sanctuary", menuName = "Node/Property/Sanctuary")]
public class TileSanctuary : TileProperty 
{
	private Dictionary<Poring, int> members = new Dictionary<Poring, int>();

	public override void OnEnter(Poring poring)
	{
		poring.Property.PermanentPoint += poring.Property.CurrentPoint;
	}

	public override void OnFinish(Poring poring)
	{
		if(members.ContainsKey(poring))
		{
			if(members[poring] < 0)
			{
				Debug.Log("You can Heal this time!!");
				ActivateHeal(poring);
			}
			else
			{
				Debug.LogFormat("Now you're dirty!! wait {0} turn.", members[poring]);
			}
		}
		else
		{
			Debug.Log("Heal first time");
			members.Add(poring, 0);
			ActivateHeal(poring);
		}
	}

	public override void OnEndRound()
	{
		for (int i = 0; i < members.Count; i++)
		{
			members[members.ElementAt(i).Key] -= 1;
		}
	}

	private void ActivateHeal(Poring poring)
	{
		poring.Property.CurrentHp = poring.Property.CurrentMaxHp;
		members[poring] = 3;
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDCell : MonoBehaviour 
{
	[SerializeField] private Text KillCount;
	[SerializeField] private Text CurrentPoint;
	[SerializeField] private Text PermanentPoint;
	[SerializeField] private Text HP;
	[SerializeField] private Text PAtk;
	[SerializeField] private Text MAtk;
	[SerializeField] private Text OffensiveDiceResult;
	[SerializeField] private Text DeffensiveDiceResult;

	private Poring poring;

	public void Init(Poring poring)
	{
		this.poring = poring;
	}

	private void FixedUpdate() 
	{
		if (poring != null)
		{
			KillCount.text = $"Kill : {poring.WinCondition}";
			CurrentPoint.text = $"Current Point : {poring.Property.CurrentPoint}";
			PermanentPoint.text = $"Permanent Point : {poring.Property.PermanentPoint}";
			HP.text = $"HP : {poring.Property.CurrentHp} / {poring.Property.CurrentMaxHp} (+{poring.Property.GrowupHp * poring.Property.CurrentPoint})";
			PAtk.text = $"P.Atk : {poring.Property.CurrentPAtk} (+{poring.Property.GrowupPAtk * poring.Property.CurrentPoint})";
			MAtk.text = $"M.Atk : {poring.Property.CurrentMAtk} (+{poring.Property.GrowupMAtk * poring.Property.CurrentPoint})";
			
			int offenResult = (poring.OffensiveResultList.Count > 0) ? poring.OffensiveResultList[0] * 10 : 0;
			int DefResult = (poring.DeffensiveResultList.Count > 0) ? poring.DeffensiveResultList[0] * 10 : 0;
			OffensiveDiceResult.text = $"{offenResult}%";
			DeffensiveDiceResult.text = $"{DefResult}%";
		}	
	}
}

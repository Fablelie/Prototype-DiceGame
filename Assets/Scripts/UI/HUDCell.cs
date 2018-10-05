using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

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
		var property = this.poring.Property;

		this.poring.ObserveEveryValueChanged(p => p.WinCondition, FrameCountType.FixedUpdate).Subscribe((i) => 
		{
			KillCount.text = $"Kill : {i}";
			// print("Update Kill");
		});

		this.poring.ObserveEveryValueChanged(p => p.Property.CurrentPoint, FrameCountType.FixedUpdate).Subscribe(i => 
		{
			
			CurrentPoint.text = $"Current Point : {i}";
			HP.text = $"HP : {property.CurrentHp} / {property.CurrentMaxHp} (+{property.GrowupHp * i})";
			PAtk.text = $"P.Atk : {property.CurrentPAtk} (+{property.GrowupPAtk * i})";
			MAtk.text = $"P.Atk : {property.CurrentMAtk} (+{property.GrowupMAtk * i})";
			// print("UPdate Point");
		});

		this.poring.ObserveEveryValueChanged(p => p.Property.PermanentPoint, FrameCountType.FixedUpdate).Subscribe(i => PermanentPoint.text = $"Permanent Point : {i}");
		this.poring.ObserveEveryValueChanged(p => p.Property.CurrentHp, FrameCountType.FixedUpdate).Subscribe(i => HP.text = $"HP : {i} / {property.CurrentMaxHp} (+{property.GrowupHp * property.CurrentPoint})");
		this.poring.ObserveEveryValueChanged(p => p.Property.CurrentPAtk, FrameCountType.FixedUpdate).Subscribe(i => PAtk.text = $"P.Atk : {i} (+{property.GrowupPAtk * property.CurrentPoint})");
		this.poring.ObserveEveryValueChanged(p => p.Property.CurrentMAtk, FrameCountType.FixedUpdate).Subscribe(i => MAtk.text = $"P.Atk : {i} (+{property.GrowupMAtk * property.CurrentPoint})");

		this.poring.ObserveEveryValueChanged(p => p.OffensiveResult, FrameCountType.FixedUpdate).Subscribe(i => 
		{
			// print("Update Dice offen");
			float offenResult = (i > 0) ? i * 10 : 0;
			float atkResult = property.CurrentPAtk;
			float adaptive = (float)(atkResult / 100) * offenResult;
			atkResult += adaptive;
			atkResult = Mathf.Ceil(atkResult);
			OffensiveDiceResult.text = $"{offenResult}% {atkResult}(+{adaptive})";
		});

		this.poring.ObserveEveryValueChanged(p => p.DeffensiveResult, FrameCountType.FixedUpdate).Subscribe(i => 
		{
			// print("Update Dice def");
			float DefResult = (i > 0) ? i * 10 : 0;
			DeffensiveDiceResult.text = $"{DefResult}%";
		});
	}

	// private void FixedUpdate() 
	// {
	// 	if (poring != null)
	// 	{
	// 		// KillCount.text = $"Kill : {poring.WinCondition}";
	// 		// CurrentPoint.text = $"Current Point : {poring.Property.CurrentPoint}";
	// 		// PermanentPoint.text = $"Permanent Point : {poring.Property.PermanentPoint}";
	// 		// HP.text = $"HP : {poring.Property.CurrentHp} / {poring.Property.CurrentMaxHp} (+{poring.Property.GrowupHp * poring.Property.CurrentPoint})";
	// 		// PAtk.text = $"P.Atk : {poring.Property.CurrentPAtk} (+{poring.Property.GrowupPAtk * poring.Property.CurrentPoint})";
	// 		// MAtk.text = $"M.Atk : {poring.Property.CurrentMAtk} (+{poring.Property.GrowupMAtk * poring.Property.CurrentPoint})";
			
	// 		float offenResult = (poring.OffensiveResultList.Count > 0) ? poring.OffensiveResultList[0] * 10 : 0;
	// 		float DefResult = (poring.DeffensiveResultList.Count > 0) ? poring.DeffensiveResultList[0] * 10 : 0;
	// 		float atkResult = poring.Property.CurrentPAtk;// + (poring.Property.GrowupPAtk * poring.Property.CurrentPoint);
	// 		float adaptive = (atkResult / 100) * offenResult;
	// 		// Debug.LogFormat($"{adaptive} = ({atkResult} / 100) * {offenResult}");
	// 		atkResult = atkResult + adaptive;
	// 		OffensiveDiceResult.text = $"{offenResult}% {atkResult}(+{adaptive})";
	// 		DeffensiveDiceResult.text = $"{DefResult}%";
	// 	}	
	// }
}

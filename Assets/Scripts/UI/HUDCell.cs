using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

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

	[SerializeField] private Roll _moveRoll;
	[SerializeField] private Roll _offensiveRoll;
	[SerializeField] private Roll _deffensiveRoll;
	
	public Image BG;

	private Poring poring;

	public void Init(Poring poring)
	{
		this.poring = poring;
		var property = this.poring.Property;

		// if (PrototypeGameMode.Instance.GetPoringIndexByPoring(poring) == PlayerNumberingExtensions.GetPlayerNumber(PhotonNetwork.LocalPlayer))
		// {
			poring.MoveRoll = _moveRoll;
			poring.OffensiveRoll = _offensiveRoll;
			poring.DeffensiveRoll = _deffensiveRoll;
		// }

		this.poring.ObserveEveryValueChanged(p => p.WinCondition, FrameCountType.FixedUpdate).Subscribe((i) => 
		{
			KillCount.text = $"Kill : {i}";
			// print("Update Kill");
		});

		this.poring.ObserveEveryValueChanged(p => p.Property.CurrentPoint, FrameCountType.FixedUpdate).Subscribe(i => 
		{
			
			CurrentPoint.text = $"Exp. {i} / {property.CurrentRequestExp} Lv. {property.CurrentLevel}";
		});

		// this.poring.ObserveEveryValueChanged(p => p.Property.PermanentPoint, FrameCountType.FixedUpdate).Subscribe(i => PermanentPoint.text = $"Permanent Point : {i}");
		this.poring.ObserveEveryValueChanged(p => p.Property.CurrentMaxHp, FrameCountType.FixedUpdate).Subscribe(i => HP.text = $"HP : {property.CurrentHp} / {i}");
		this.poring.ObserveEveryValueChanged(p => p.Property.CurrentHp, FrameCountType.FixedUpdate).Subscribe(i => HP.text = $"HP : {i} / {property.CurrentMaxHp}");
		this.poring.ObserveEveryValueChanged(p => p.Property.CurrentPAtk, FrameCountType.FixedUpdate).Subscribe(i => PAtk.text = $"P.Atk : {i}");
		this.poring.ObserveEveryValueChanged(p => p.Property.CurrentMAtk, FrameCountType.FixedUpdate).Subscribe(i => MAtk.text = $"P.Atk : {i}");

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
}

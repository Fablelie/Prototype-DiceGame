using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillDescriptionPanel : MonoBehaviour {

	public Text Description;

	[SerializeField] private Animation ani;

	public void SetDetail(SkillDetail detail)
	{
		string msg = "";
		msg += $"Damage : <color=#FF0000>{detail.Damage}</color> \n";

		string colorHex = (detail.DamageType == DamageType.PAtk) ? "FF0000" : "0000FF";
		msg += $"Damage Type : <color=#{colorHex}>{detail.DamageType}</color> \n";

		msg += $"Target Type : {detail.TargetTypestr}\n";
		msg += $"MoveToTarget : {detail.MoveToTarget} \n";
		msg += $"AOE : ({detail.AOE}*{detail.AOE}) \n";
		msg += $"Range : {detail.Range} \n";
		msg += $"CD : {detail.CD} \n";
		msg += $"Description : \n {detail.Description} \n";

		Description.text = msg;
		gameObject.SetActive(true);
		ani.Play("SkillDescriptionOnEnable", PlayMode.StopAll);
	}

	public void ClosePanel()
	{
		ani.Play("SkillDescriptionOnDisable", PlayMode.StopAll);
	}
}

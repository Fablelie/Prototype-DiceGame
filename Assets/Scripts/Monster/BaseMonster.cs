using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseMonster : MonoBehaviour 
{
	[Header("Data from base.")]
	public MonsterData Data;
	[Header("Prefab")]
	public Animator animator;
	public Text Name;
	public void Init(BaseMonsterScriptableObject baseProperty, Poring poring)
	{
		Data.Name = baseProperty.Data.Name;
		Data.Attack = baseProperty.Data.Attack;
		Data.MaxHp = baseProperty.Data.MaxHp;
		Data.CurrentHp = baseProperty.Data.CurrentHp;
		Data.DropExp = baseProperty.Data.DropExp;
		Data.DropCurrency = baseProperty.Data.DropCurrency;
		Data.BuffEffects = baseProperty.Data.BuffEffects;

		Name.text = Data.Name;

		transform.SetParent(poring.Node.transform);
		transform.localPosition = Vector3.zero;
		poring.Node.AddMonster(this);

		animator.Play("Spawn");
	}
}

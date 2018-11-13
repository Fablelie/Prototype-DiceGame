using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseMonster", menuName = "Monster/BaseMonster")]
public class BaseMonsterScriptableObject : ScriptableObject 
{
	public GameObject Prefab;
	public MonsterData Data;
}

[System.Serializable]
public class MonsterData
{
	public string Name;
	public int Attack;
	public int MaxHp;
	public int CurrentHp;
	public int DropExp;
	public int DropCurrency;
	public List<BuffEffect> BuffEffects;
}

[System.Serializable]
public class BuffEffect
{
	public BuffType Type;
	public int Duration;
	public int EffectValue;
}

public enum BuffType
{
	Atk,
	Def,
	RestoreHp,
}

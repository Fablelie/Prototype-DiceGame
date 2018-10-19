using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
	Self = 0,
	All = 1,
	Another,
	Tile,
	ActiveOnAttack,
	ActiveOnDefense,
}

public enum SkillMode
{
	Activate,
	Passive,
}

public enum DamageType
{
	PAtk,
	MAtk,
}

[Flags]
public enum SkillStatus
{
	
	Stun = 2,
	Posion = 4,
	Burn = 8,
	Bleed = 16,
	Sleep = 32,
	Ambursh = 64,
}


public class BaseSkill : ScriptableObject 
{
	public string AnimationStateName;
	public GameObject PrefabEffect;
	public Sprite SkillIcon;
	[Range(0, 10)] public int MinRangeValue;
	[Range(0, 10)] public int MaxRangeValue;

	public SkillMode SkillMode;
	public DamageType DamageType;
	public float DamageMultiple;

	public TargetType TargetType;
	[Tooltip("0 is not AOE skill will skin this value.")] [Range(0, 4)] public int AOEValue;
	public bool IsIgnoreSelf;
	public int SkillDuration;
	public int TurnCD;
	public bool MoveToTarget;

	[SerializeField] [EnumFlags] public SkillStatus SkillStatus;

	public virtual void OnActivate(Poring poring, Poring targetPoring = null, Node targetNode = null, List<Node> nodeList = null)
	{

	}

	public virtual void OnAttack()
	{

	}

	public virtual void OnDefense()
	{

	}

	public virtual void OnEndTurn()
	{

	}

	public virtual void OnStartTurn()
	{

	}

	public virtual void OnReceiveStatus(int skillStatusResult)
	{
		// TODO 
	}
}

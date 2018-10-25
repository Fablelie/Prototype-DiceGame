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

public enum AttackTypeResult
{
	None,
	Double,
	PowerUp,
}

public struct OnAttackSkillResult
{	
	public AttackTypeResult Type;
	public DamageType DamageType;
	public int StatusResult;
	public float DamageResult;
	public GameObject EffectOnSelf;
	public GameObject EffectOnTarget;

	public OnAttackSkillResult(AttackTypeResult type, DamageType damageType, int statusResult, float damageResult, GameObject effectOnSelf = null, GameObject effectOnTarget = null)
	{
		Type         = type;
		DamageType   = damageType;
		StatusResult = statusResult;
		DamageResult = damageResult;
		EffectOnSelf = effectOnSelf;
		EffectOnTarget = effectOnTarget;
	}
}

public enum DefenseTypeResult
{
	None,
	Counter,
	Dodge,
}

public struct OnDefenseSkillResult
{
	public DefenseTypeResult Type;
	public DamageType DamageType;
	public int StatusResult;
	public float DamageResult;

	public GameObject EffectOnSelf;
	public GameObject EffectOnTarget;

	public OnDefenseSkillResult(DefenseTypeResult type, DamageType damageType, int statusResult, float damageResult, GameObject effectOnSelf = null, GameObject effectOnTarget = null)
	{
		Type         = type;
		DamageType   = damageType;
		StatusResult = statusResult;
		DamageResult = damageResult;
		EffectOnSelf = effectOnSelf;
		EffectOnTarget = effectOnTarget;
	}
}

public class BaseSkill : ScriptableObject 
{
	public string AnimationStateName;
	public GameObject EffectOnHit;
	public GameObject EffectOnSelf;
	public GameObject EffectOnTarget;
	public Sprite SkillIcon;
	[Range(0, 10)] public int MinRangeValue;
	[Range(0, 10)] public int MaxRangeValue;

	public SkillMode SkillMode;
	public DamageType DamageType;
	public float DamageMultiple;

	public TargetType TargetType;
	[Tooltip("0 is not AOE skill will skip this value.")] [Range(0, 4)] public int AOEValue;
	public bool IsIgnoreSelf;
	public int SkillDuration;
	public int TurnCD;
	public bool MoveToTarget;
	public int CurrentCD = 0;

	[SerializeField] 
	 #if UNITY_EDITOR
	[EnumFlags] 
	#endif
	public SkillStatus SkillStatus;

	public virtual void Init(BaseSkill baseSkill)
	{
		this.name          = baseSkill.name;
		AnimationStateName = baseSkill.AnimationStateName;
		SkillIcon          = baseSkill.SkillIcon;
		MinRangeValue      = baseSkill.MinRangeValue;
		MaxRangeValue      = baseSkill.MaxRangeValue;
		SkillMode          = baseSkill.SkillMode;
		DamageType         = baseSkill.DamageType;
		DamageMultiple     = baseSkill.DamageMultiple;
		TargetType         = baseSkill.TargetType;
		AOEValue           = baseSkill.AOEValue;
		IsIgnoreSelf       = baseSkill.IsIgnoreSelf;
		SkillDuration      = baseSkill.SkillDuration;
		TurnCD             = baseSkill.TurnCD;
		MoveToTarget       = baseSkill.MoveToTarget;
		CurrentCD          = 0;

		EffectOnHit = baseSkill.EffectOnHit;
		EffectOnSelf = baseSkill.EffectOnSelf;
		EffectOnTarget = baseSkill.EffectOnTarget;
	}

	public virtual void OnActivate(Poring poring, Poring targetPoring = null, Node targetNode = null, List<Node> nodeList = null){}
	public virtual OnAttackSkillResult OnAttack(Poring poring, FaceDice faceDice){ return new OnAttackSkillResult(AttackTypeResult.None, DamageType.PAtk, 0, 0); }
	public virtual OnDefenseSkillResult OnDefense(Poring attacker, Poring poring, FaceDice faceDice){ return new OnDefenseSkillResult(DefenseTypeResult.None, DamageType.PAtk, 0, 0); }
	public virtual void OnEndTurn(){}
	public virtual void OnStartTurn(){}
	public virtual void OnReceiveStatus(int skillStatusResult){}
}

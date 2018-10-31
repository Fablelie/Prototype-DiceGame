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
	Stun = 1 << 0,
	Posion = 1 << 1,
	Burn = 1 << 2,
	Bleed = 1 << 3,
	Sleep = 1 << 4,
	Ambursh = 1 << 5,
	Root = 1 << 6,
	Freeze = 1 << 7,
}

[System.Serializable]
public class EffectReceiver
{
	public int OwnerId;
	[Tooltip("-1 is calculate from current atk, damage type and damage multiple")] public float Damage;
	public int EffectDuration;
	[SerializeField] 
	 #if UNITY_EDITOR
	[EnumFlags] 
	#endif
	public SkillStatus Status;
	public GameObject Particle;

	public EffectReceiver(EffectReceiver baseObj)
	{
		OwnerId        = baseObj.OwnerId;
		EffectDuration = baseObj.EffectDuration;
		Status         = baseObj.Status;
		Particle       = baseObj.Particle;
		Damage		   = baseObj.Damage;
	}

}

public enum AttackTypeResult
{
	None,
	Double,
	PowerUp,
	Enchant,
}

public struct OnAttackSkillResult
{	
	public AttackTypeResult Type;
	public DamageType DamageType;
	public List<EffectReceiver> EffectStatusResults;
	public float DamageResult;
	public GameObject EffectOnSelf;
	public GameObject EffectOnTarget;

	public OnAttackSkillResult(AttackTypeResult type, DamageType damageType, float damageResult, List<EffectReceiver> statusResult = null, GameObject effectOnSelf = null, GameObject effectOnTarget = null)
	{
		if(statusResult == null) statusResult = new List<EffectReceiver>();
		Type         = type;
		DamageType   = damageType;
		EffectStatusResults = statusResult;
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
	Enchant
}

public struct OnDefenseSkillResult
{
	public DefenseTypeResult Type;
	public DamageType DamageType;
	public List<EffectReceiver> EffectStatusResults;
	public float DamageResult;

	public GameObject EffectOnSelf;
	public GameObject EffectOnTarget;

	public OnDefenseSkillResult(DefenseTypeResult type, DamageType damageType, float damageResult, List<EffectReceiver> statusResult = null, GameObject effectOnSelf = null, GameObject effectOnTarget = null)
	{
		if(statusResult == null) statusResult = new List<EffectReceiver>();
		Type         = type;
		DamageType   = damageType;
		EffectStatusResults = statusResult;
		DamageResult = damageResult;
		EffectOnSelf = effectOnSelf;
		EffectOnTarget = effectOnTarget;
	}
}

[Serializable]
public struct StrongerWithStatus
{
	[SerializeField] 
	 #if UNITY_EDITOR
	[EnumFlags] 
	#endif
	public SkillStatus Status;

	public int DamageMultiple;
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
	public List<EffectReceiver> EffectsReceiver = new List<EffectReceiver>();
	public List<StrongerWithStatus> StrongerList = new List<StrongerWithStatus>();

	public string Description;

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
		Description 	   = baseSkill.Description;

		EffectsReceiver = baseSkill.EffectsReceiver;

		EffectOnHit = baseSkill.EffectOnHit;
		EffectOnSelf = baseSkill.EffectOnSelf;
		EffectOnTarget = baseSkill.EffectOnTarget;
	}

	public virtual void OnActivate(Poring poring, Poring targetPoring = null, Node targetNode = null, List<Node> nodeList = null){}
	public virtual OnAttackSkillResult OnAttack(Poring poring, FaceDice faceDice){ return new OnAttackSkillResult(AttackTypeResult.None, DamageType.PAtk, 0); }
	public virtual OnDefenseSkillResult OnDefense(Poring attacker, Poring poring, FaceDice faceDice){ return new OnDefenseSkillResult(DefenseTypeResult.None, DamageType.PAtk, 0); }
	public virtual void OnEndTurn(){}
	public virtual void OnStartTurn(){}
	public virtual void OnReceiveStatus(int skillStatusResult){}

	/// <summary>
	/// Use every time before sent result of skill.
	/// </summary>
	public void SetEffectOwnerIdAndDamage(Poring poring)
	{
		EffectsReceiver.ForEach(fx =>
        {
			fx.OwnerId = PrototypeGameMode.Instance.GetPoringIndexByPoring(poring);
			if (fx.Damage == -1) fx.Damage = DamageMultiple * ((DamageType == DamageType.PAtk) ? poring.Property.CurrentPAtk : poring.Property.CurrentMAtk);
		});
	}
}

public class ExtensionStatus
{
	public static bool CheckHasStatus(int input, int condition)
	{
		bool result = true;
		result &= ((input & condition) != 0);
		return result;
	}
}

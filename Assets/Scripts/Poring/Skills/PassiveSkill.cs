using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveSKill", menuName = "Poring/Skills/PassiveSkill")]
public class PassiveSkill : BaseSkill 
{
	public AttackTypeResult AttackType;
	public DefenseTypeResult DefenseType;

	public FaceDice ActiveOnType;

	public override void Init(BaseSkill baseSkill)
	{
		base.Init(baseSkill);
		var skill = (PassiveSkill)baseSkill;
		AttackType   = skill.AttackType;
		DefenseType  = skill.DefenseType;
		ActiveOnType = skill.ActiveOnType;
	}

	public override OnAttackSkillResult OnAttack(Poring poring, FaceDice faceDice)
	{
		
		if (faceDice == ActiveOnType && CurrentCD <= 0)
		{
			
			CurrentCD = TurnCD;
			float damage = 0;
			switch (AttackType)
			{
				case AttackTypeResult.None:
					damage = 0;
				break;
				case AttackTypeResult.Double:
					damage = (DamageType == DamageType.PAtk) ? poring.Property.CurrentPAtk : poring.Property.CurrentMAtk;
				break;
				case AttackTypeResult.PowerUp:
					damage = (DamageType == DamageType.PAtk) ? poring.Property.CurrentPAtk : poring.Property.CurrentMAtk;
					damage *= 2;
				break;
				case AttackTypeResult.Enchant:
					damage = (DamageType == DamageType.PAtk) ? poring.Property.CurrentPAtk : poring.Property.CurrentMAtk;
				break;
			}
			return new OnAttackSkillResult()
			{
				Type = AttackType,
				DamageType = DamageType,
				EffectStatusResults = SetEffectOwnerIdAndDamage(poring),
				DamageResult = damage,
				EffectOnSelf = EffectOnSelf,
				EffectOnTarget = EffectOnHit,
			};
		}
		else 
		{
			return new OnAttackSkillResult()
			{
				Type = AttackTypeResult.None,
				DamageType = DamageType,
				EffectStatusResults = new List<EffectReceiver>(),
				DamageResult =0
			};
		}
	}

	public override OnDefenseSkillResult OnDefense(Poring attacker, Poring poring, FaceDice faceDice)
	{
		
		if (faceDice == ActiveOnType && CurrentCD <= 0)
		{
			// Debug.LogError($"{poring.name} : {this.name}!!");
			float damage = 0;
			switch (DefenseType)
			{
				case DefenseTypeResult.None:
					damage = 0;
				break;
				case DefenseTypeResult.Counter:
					damage = (DamageType == DamageType.PAtk) ? poring.Property.CurrentPAtk : poring.Property.CurrentMAtk;
					if (attacker.Node != poring.Node)
					{
						DefenseType = DefenseTypeResult.None;
						damage = 0;
					}
				break;
				case DefenseTypeResult.Dodge:
					damage = 0;
				break;
			}

			return new OnDefenseSkillResult()
			{
				Type = DefenseType,
				DamageType = DamageType,
				EffectStatusResults = SetEffectOwnerIdAndDamage(poring),
				DamageResult = damage,
				EffectOnSelf = EffectOnSelf,
				EffectOnTarget = EffectOnHit,
			};
		}
		else 
		{
			return new OnDefenseSkillResult()
			{
				Type = DefenseTypeResult.None,
				DamageType = DamageType,
				EffectStatusResults = new List<EffectReceiver>(),
				DamageResult = 0
			};
		}
	}
}

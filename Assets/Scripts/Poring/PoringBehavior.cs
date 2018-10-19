using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoringBehavior : MonoBehaviour 
{
	public Poring Poring;
	private PrototypeGameMode m_gameMode;
	private float m_moveSpeed = 1;
	private Vector3 m_rightForward = new Vector3(1, 0, 1);
	private Vector3 m_targetPosition;
	private bool m_isMove = false;
	private WaitForSeconds waitSecond = new WaitForSeconds(0.7f);

	private void Awake() 
	{
		m_gameMode = PrototypeGameMode.Instance;
	}

	private void TurnFaceTo(Vector3 pos)
	{
		transform.LookAt(pos);
	}

	private IEnumerator MoveStep()
	{
		m_isMove = true;
		while (m_isMove)
		{
			
			float step = m_moveSpeed * m_moveSpeed * Time.deltaTime;
			// Debug.Log("MoveStep >>>>>>>>>>>>>>>>> " + step);
			transform.position = Vector3.MoveTowards(transform.position, m_targetPosition, step);
			yield return new WaitForEndOfFrame();
		}
	}

	public void OnSpawn()
	{	
		
	}
    
	public void SetupJumpToNodeTarget(List<Node> nodeList, Action callbackForSkillMove = null)
	{
		StartCoroutine(JumpTo(nodeList, callbackForSkillMove));
	}

    private WaitForSeconds wait = new WaitForSeconds(0.1f);
    private IEnumerator JumpTo(List<Node> nodeList, Action callbackForSkillMove = null)
	{
        foreach (var node in nodeList)
		{
            yield return wait;
            Poring.PrevNode = Poring.Node;
            Poring.Node.RemovePoring(Poring);
			m_targetPosition = node.transform.position;
			TurnFaceTo(m_targetPosition);
			Poring.Animator.Play("jump");
			m_isMove = true;

			yield return new WaitUntil(() => !m_isMove);
            
            node.AddPoring(Poring);
		}

        yield return wait;

		if (callbackForSkillMove != null)
			callbackForSkillMove();
		else
			FinishMove();
	}

	

	public void FinishMove()
	{
		Poring.Node.PoringKeepValueOnTile(Poring);
		m_gameMode.CurrentGameState = eStateGameMode.Encounter;
	}

	public bool hasAttack = false;

    public void AttackTarget()
    {
		Poring.OffensiveResultList.Clear();
		Poring.Target.DeffensiveResultList.Clear();
		Poring.OffensiveRoll.SetRoll(Poring.Property.OffensiveDices[0].FaceDiceList, m_gameMode.GetPoringIndexByPoring(Poring));
		Poring.Target.DeffensiveRoll.SetRoll(Poring.Target.Property.DeffensiveDices[0].FaceDiceList, m_gameMode.GetPoringIndexByPoring(Poring.Target));
		StartCoroutine(WaitForDiceResult());
    }

	private IEnumerator WaitForDiceResult()
	{
		yield return new WaitUntil(() => Poring.OffensiveResultList.Count > 0 && Poring.Target.DeffensiveResultList.Count > 0);
		TurnFaceTo(Poring.Target.transform.position);
		hasAttack = true;
		var attackerDiceResult = CalculateAtackerDiceResult(Poring);
		var defenderDiceResult = CalculateDefenderDiceResult(Poring, Poring.Target);

		switch (defenderDiceResult.Type)
		{
			case DefenseTypeResult.None:
			break;
			case DefenseTypeResult.Counter:
				Poring.Target.Animator.Play("Skill");
				yield return waitSecond;

				Poring.Property.CurrentHp -= defenderDiceResult.DamageResult;
				if (Poring.Property.CurrentHp > 0)
				{
					Poring.Animator.Play("take_damage");
					yield return waitSecond;

					if(!Poring.Target.Behavior.hasAttack && Poring.Node == Poring.Target.Node)
					{
						Poring.Target.Target = Poring;
						Poring.Target.Behavior.AttackTarget();
						yield break;
					}
					else
					{
						Poring.Target.Behavior.hasAttack = hasAttack = false;
						Poring.Target.Target = Poring.Target = null;

						m_gameMode.CurrentGameState = eStateGameMode.EndTurn;
						yield break;
					}
				}
				else
				{
					Poring.Animator.Play("die");
					yield return waitSecond;
					Poring.Target.Property.CurrentPoint += Poring.Property.CurrentPoint / 2;
					Poring.Target.WinCondition += 1;

					Poring.Behavior.Respawn();
					Poring.Target.Behavior.hasAttack = hasAttack = false;
					Poring.Target.Target = Poring.Target = null;

					m_gameMode.CurrentGameState = eStateGameMode.EndTurn;
					yield break;
				}
			case DefenseTypeResult.Dodge:
			break;
		}

		switch (attackerDiceResult.Type)
		{
			case AttackTypeResult.None:
			break;
			case AttackTypeResult.Double:
			break;
			case AttackTypeResult.PowerUp:
			break;
		}

		float damageResult = AdaptiveDamageCalculate(Poring);
		damageResult = AdaptiveDefenseCalculate(damageResult, Poring.Target);
		float hpResult = Poring.Target.Property.CurrentHp;

		hpResult -= damageResult;

		Poring.Animator.Play("Skill");
		yield return waitSecond;
		Poring.Target.Animator.Play((damageResult == 0) ? "Dodge" : (hpResult <= 0) ? "die" : "take_damage");
		yield return waitSecond;

		Poring.Target.Property.CurrentHp = hpResult;
		if (hpResult > 0) // alive
		{
			if(!Poring.Target.Behavior.hasAttack && Poring.Node == Poring.Target.Node)
			{
				Poring.Target.Target = Poring;
				Poring.Target.Behavior.AttackTarget();
			}
			else
			{
                Poring.Target.Behavior.hasAttack = hasAttack = false;
                Poring.Target.Target = Poring.Target = null;

                m_gameMode.CurrentGameState = eStateGameMode.EndTurn;
			}
		}
		else // die
		{
			Poring.Property.CurrentPoint += Poring.Target.Property.CurrentPoint / 2;
			Poring.WinCondition += 1;

            Poring.Target.Behavior.Respawn();
            Poring.Target.Behavior.hasAttack = hasAttack = false;
            Poring.Target.Target = Poring.Target = null;

            m_gameMode.CurrentGameState = eStateGameMode.EndTurn;
		}
	}
	#region Calculate

	private OnAttackSkillResult CalculateAtackerDiceResult(Poring poring)
	{
		OnAttackSkillResult result = new OnAttackSkillResult(AttackTypeResult.None, DamageType.PAtk, 0, 0);
		FaceDice faceDice = poring.Property.OffensiveDices[0].GetDiceFace(poring.OffensiveResultList[0]);
		poring.Property.SkillList.ForEach(skill =>
		{
			var subResult = skill.OnAttack(poring, faceDice);
			if(subResult.Type != AttackTypeResult.None)
			{
				result = subResult;
			}
		});

		return result;
	}

	private OnDefenseSkillResult CalculateDefenderDiceResult(Poring attacker, Poring poring)
	{
		OnDefenseSkillResult result = new OnDefenseSkillResult(DefenseTypeResult.None, DamageType.PAtk, 0, 0);
		FaceDice faceDice = poring.Property.DeffensiveDices[0].GetDiceFace(poring.DeffensiveResultList[0]);
		poring.Property.SkillList.ForEach(skill =>
		{
			var subResult = skill.OnDefense(attacker, poring, faceDice);
			if(subResult.Type != DefenseTypeResult.None)
			{
				result = subResult;
			}
		});

		return result;
	}

	private float AdaptiveDamageCalculate(Poring poring)
	{
		int result = 0;
		for(int i = 0; i < poring.Property.OffensiveDices.Count; i++)
		{
			var offDice = poring.Property.OffensiveDices[i];
			int diceResult = offDice.GetNumberFromDiceFace(offDice.GetDiceFace(poring.OffensiveResultList[i]));

			if(diceResult == (int)FaceDice.Miss) return 0;

			result = result + diceResult;
		}
		float damage = poring.Property.CurrentPAtk;
		damage = damage + (damage / 100) * (result*10);

		return Mathf.Ceil(damage);
	}

	private float AdaptiveDefenseCalculate(float damage, Poring poring)
	{
		int result = 0;
		for(int i = 0; i < poring.Property.DeffensiveDices.Count; i++)
		{
			var defDice = poring.Property.DeffensiveDices[i];
			int diceResult = defDice.GetNumberFromDiceFace(defDice.GetDiceFace(poring.DeffensiveResultList[i]));

			result = result + diceResult;
		}
		damage = damage - (damage / 100) * (result * 10);
		return Mathf.Ceil(damage);
	}

	#endregion

    public void Respawn()
    {
        Poring.Property.CurrentPoint = Poring.Property.CurrentPoint / 2;
        
        gameObject.transform.position = m_gameMode.StartNode.transform.position;
        Poring.Animator.Play("Warp_down");

        Poring.PrevNode = null;
        Poring.Node.RemovePoring(Poring);
        m_gameMode.StartNode.AddPoring(Poring);

		Poring.Property.CurrentHp = Poring.Property.CurrentMaxHp;
    }

    #region Callback from animation event

	public void CallbackDamageActive()
	{
		
	}

    // Call from animation event.
    public void CallbackStartMove()
    {
        // Debug.LogError("CallbackStartMove");
        m_moveSpeed = Vector3.Distance(Vector3.Scale(transform.localPosition, m_rightForward), Vector3.Scale(m_targetPosition, m_rightForward));
        m_moveSpeed *= 1f; // moveSpeed with animation "Jump"

        Poring.Animator.speed = m_moveSpeed;

        StartCoroutine(MoveStep());
    }

    // Call from animation event.
    public void CallbackTriggerBlockAnimation()
    {

    }

    // Call from animation event.
    public void CallbackEndMove()
    {
        m_isMove = false;
        transform.position = m_targetPosition;
        Poring.Animator.speed = 1;
    }

    #endregion
}

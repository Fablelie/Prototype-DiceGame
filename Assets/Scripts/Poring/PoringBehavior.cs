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

	private void Awake() 
	{
		m_gameMode = (PrototypeGameMode)GameMode.Instance;
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
		// MainCameraFollowTarget.I.ChangeTarget(transform);
		// m_omActor.GetActorScript<PoringActorCharacter>().ActorControl.SetSampleAnimation((uint)eAnimationStatePoring.Warp_down);
		// m_omActor.GetAnimator?.Play("Warp_down");
	}
    
	public void SetupJumpToNodeTarget(List<Node> nodeList)
	{
		StartCoroutine(JumpTo(nodeList));
	}

    private WaitForSeconds wait = new WaitForSeconds(0.1f);
    private IEnumerator JumpTo(List<Node> nodeList)
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
		FinishMove();
	}

	

	public void FinishMove()
	{
		m_gameMode.CurrentGameState = eStateGameMode.Encounter;
	}

	public bool hasAttack = false;

    public void AttackTarget()
    {
		hasAttack = true;
		Poring.Animator.Play("Ultimate");
    }
	#region Calculate

	private int AdaptiveDamageCalculate()
	{
		int adaptiveDamage = 0;
		for(int i = 0; i < Poring.Property.OffensiveDices.Count; i++)
		{
			adaptiveDamage += Poring.Property.OffensiveDices[i].GetDiceFace(Poring.OffensiveResultList[i]);
			Poring.OffensiveResultList[i] = Random.Range(0, 5);
		}
		Debug.LogFormat("AdaptiveDamage >>>>>>>>>>>>>>>> {0}", adaptiveDamage);
		return adaptiveDamage * 10;
	}

	private int AdaptiveDefenseCalculate()
	{
		int adaptiveDefense = 0;
		for(int i = 0; i < Poring.Target.Property.DeffensiveDices.Count; i++)
		{
			adaptiveDefense += Poring.Target.Property.DeffensiveDices[i].GetDiceFace(Poring.Target.DeffensiveResultList[i]);
			Poring.Target.DeffensiveResultList[i] = Random.Range(0, 5);
		}
		Debug.LogFormat("adaptiveDefense >>>>>>>>>>>>>>>> {0}", adaptiveDefense);
		return adaptiveDefense * 10;
	}

	#endregion

    public void Respawn()
    {
        Poring.Property.CurrentPoint = Poring.Property.CurrentPoint / 2;
        Poring.Property.CurrentHp = Poring.Property.BaseHp;

        gameObject.transform.position = m_gameMode.StartNode.transform.position;
        Poring.Animator.Play("Warp_down");
    }

    #region Callback from animation event

	public void CallbackDamageActive()
	{
		int damageResult = Poring.Property.CurrentPAtk;
		int hpResult = Poring.Target.Property.CurrentHp;

		damageResult = damageResult + (damageResult / 100) * AdaptiveDamageCalculate(); 
		Debug.LogFormat("Damage + AdaptiveDamage >>>>>>>>>>>>>>> {0}", damageResult);
		damageResult = damageResult - (damageResult / 100) * AdaptiveDefenseCalculate();
		Debug.LogFormat("Damage - adaptiveDefense >>>>>>>>>>>>>>> {0}", damageResult);

		hpResult -= damageResult;
		Debug.Log("Current hp : " + Poring.Target.Property.CurrentHp);
		Debug.Log("HP : " + hpResult);

		if(hpResult > 0) // alive
		{
			Poring.Target.Animator.Play("take_damage");
			Poring.Target.Property.CurrentHp = hpResult;
			
			if(!Poring.Target.Behavior.hasAttack)
			{
				Poring.Target.Target = Poring;
				Poring.Target.Behavior.AttackTarget();
			}
			else
			{
				Poring.Target.Target = null;
				Poring.Target = null;
				m_gameMode.CurrentGameState = eStateGameMode.EndTurn;
			}
		}
		else // die
		{
            Poring.Target.Animator.Play("die");
			Poring.Property.CurrentPoint += Poring.Target.Property.CurrentPoint / 2;
			Poring.WinCondition += 1;
            Poring.Target.Target = null;
            Poring.Target = null;
            m_gameMode.CurrentGameState = eStateGameMode.EndTurn;
		}
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

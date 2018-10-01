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

    private Node m_targetNode;

	public void SetupJumpToNodeTarget(List<Node> nodeList, Node targetNode)
	{
        //nodeList.RemoveAt(0);
        m_targetNode = targetNode;
		StartCoroutine(JumpTo(nodeList));
	}

    private WaitForSeconds wait = new WaitForSeconds(1);

    private IEnumerator JumpTo(List<Node> nodeList)
	{
		foreach(var node in nodeList)
		{
            Poring.PrevNode = Poring.Node;
            Poring.Node.RemovePoring(Poring);
			m_targetPosition = node.transform.position;
			TurnFaceTo(m_targetPosition);
			Poring.Animator.Play("jump");
			m_isMove = true;

			yield return new WaitUntil(() => !m_isMove);
            
            node.AddPoring(Poring);
            if (node == m_targetNode)
            {
                break;
            }
		}

        yield return wait;
		FinishMove();
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

	public void FinishMove()
	{
		m_gameMode.CurrentGameState = eStateGameMode.Encounter;
	}
}

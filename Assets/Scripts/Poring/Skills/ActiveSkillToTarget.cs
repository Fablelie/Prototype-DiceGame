using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkill", menuName = "Poring/Skills/ActiveSkill")]
public class ActiveSkillToTarget : BaseSkill 
{
    private PrototypeGameMode gameMode;
    private eStateGameMode nextState;

    public override void OnActivate(Poring poring, Poring targetPoring = null, Node targetNode = null, List<Node> nodeList = null)
    {
        GameObject target = (targetPoring != null) ? targetPoring.gameObject : targetNode.gameObject;
        gameMode = (PrototypeGameMode)PrototypeGameMode.Instance;

        poring.transform.LookAt(target.transform.position);
        CurrentCD = TurnCD;

        if (nodeList != null)
        {
            // Move first!!
            nextState = eStateGameMode.EndTurn;
            poring.Behavior.SetupJumpToNodeTarget(nodeList, () => SkillEffectActivate(poring, targetPoring, targetNode));
        }
        else
        {
            nextState = gameMode.CurrentGameState;
            SkillEffectActivate(poring, targetPoring, targetNode);
        }
    }

    private void SkillEffectActivate(Poring poring, Poring targetPoring = null, Node targetNode = null)
    {
        float damageResult = (DamageType == DamageType.PAtk) ? poring.Property.CurrentPAtk : poring.Property.CurrentMAtk;
        damageResult *= DamageMultiple;

        PrototypeGameMode.Instance.StartCoroutine(WaitForAnimation(poring, damageResult, targetPoring, targetNode));
    }

    private IEnumerator WaitForAnimation(Poring poring, float damage, Poring targetPoring = null, Node targetNode = null)
    {
        
        if (targetPoring != null)
        {
            // bash
            poring.transform.LookAt(targetPoring.gameObject.transform);
            poring.Animator.Play(AnimationStateName);
            yield return new WaitForSeconds(2f);
            
            if (PrefabEffect != null)
                GameObject.Instantiate(PrefabEffect, targetPoring.transform.position, Quaternion.identity);

            AddDamageToTarget(poring, targetPoring, damage);
        }
        else if (targetNode != null)
        {
            // thunder bolt
            poring.transform.LookAt(targetNode.gameObject.transform);
            poring.Animator.Play(AnimationStateName);
            yield return new WaitForSeconds(2f);
            AOESkillActivate(poring, targetNode, AOEValue, damage);
        }
        else
        {
            // Magnum Break
            poring.Animator.Play(AnimationStateName);
            yield return new WaitForSeconds(2f);
            if (PrefabEffect != null)
                GameObject.Instantiate(PrefabEffect, targetPoring.transform.position, Quaternion.identity);
            AOESkillActivate(poring, targetNode, AOEValue, damage);
        }

        if (!MoveToTarget)
            TurnActiveUIController.Instance.SetActivePanel(true);
        gameMode.CurrentGameState = nextState;
        
    }

    private void AOESkillActivate(Poring poring, Node currentNode, int value, float damage, Node prevNode = null)
    {
        if (prevNode == null)
        {
            AddDamageToTargetNode(poring, damage, currentNode);
        }

        if (value > 0)
        {
            value--;
            currentNode.NeighborList.ForEach(neighbor =>
            {
                AddDamageToTargetNode(poring, damage, neighbor.Node);
                AOESkillActivate(poring, neighbor.Node, value, damage, currentNode);    
            });
        }
    }

    private void AddDamageToTargetNode(Poring poring, float damage, Node node)
    {
        node.porings.ForEach(target =>
        {
            if(target == poring && IsIgnoreSelf)
            {

            }
            else
                AddDamageToTarget(poring, target, damage);
        });
    }

    private void AddDamageToTarget(Poring poring, Poring targetPoring, float damage)
    {
        targetPoring.Animator.Play("take_damage");
        targetPoring.Property.CurrentHp -= damage;

        targetPoring.OnReceiveStatus((int)SkillStatus);

        CheckTargetAlive(poring, targetPoring);
    }

    private void CheckTargetAlive(Poring poring, Poring targetPoring)
    {
        if (targetPoring.Property.CurrentHp <= 0)
        {
            poring.WinCondition += 1;
            targetPoring.Behavior.Respawn();
        }
    }
}

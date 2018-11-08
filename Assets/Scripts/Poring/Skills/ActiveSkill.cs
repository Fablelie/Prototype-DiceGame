using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkill", menuName = "Poring/Skills/ActiveSkill")]
public class ActiveSkill : BaseSkill 
{
    private PrototypeGameMode gameMode;
    private eStateGameMode nextState;

    private List<Poring> targetOnRoute = new List<Poring>();

    public override void OnActivate(Poring poring, Poring targetPoring = null, Node targetNode = null, List<Node> nodeList = null)
    {
        gameMode = (PrototypeGameMode)PrototypeGameMode.Instance;
        
        if(targetPoring != null || targetNode != null)
        {
            GameObject target = (targetPoring != null) ? targetPoring.gameObject : targetNode.gameObject;
            poring.transform.LookAt(target.transform.position);
        }

        CurrentCD = TurnCD;
        targetOnRoute.Clear();

        if (nodeList != null)
        {
            // Move first!!
            nextState = eStateGameMode.EndTurn;
            if(IsAppendDamageOnRoute)
            {
                nodeList.ForEach(n => 
                {
                    n.porings.ForEach(p =>
                    {
                        if(p != targetPoring || p != poring) 
                            targetOnRoute.Add(p);
                    });
                });
            }

            poring.Behavior.SetupJumpToNodeTarget(nodeList, () => SkillEffectActivate(poring, targetPoring, targetNode));
        }
        else
        {
            nextState = (poring.Property.UltimateSkill.name == name) ? eStateGameMode.EndTurn : gameMode.CurrentGameState;
            SkillEffectActivate(poring, targetPoring, targetNode);
        }
    }

    private void SkillEffectActivate(Poring poring, Poring targetPoring = null, Node targetNode = null)
    {
        float damageResult = (DamageType == DamageType.PAtk) ? poring.Property.CurrentPAtk : poring.Property.CurrentMAtk;
        damageResult *= poring.GetBlessingBuff();
        EffectReceiver maximizePower = poring.GetStatus(SkillStatus.MaximizePower);
        damageResult *= (maximizePower != null) ? maximizePower.Damage : 1;
        damageResult *= DamageMultiple;

        if(poring.CheckHasStatus(SkillStatus.Blind) && targetNode == null)
            damageResult = 0;

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

            targetOnRoute.ForEach( p =>
            {
                if(p != targetPoring)
                {
                    if (EffectOnSelf != null)
                        InstantiateParticleEffect.CreateFx(EffectOnSelf, p.transform.position);

                    AddDamageToTarget(poring, p, damage);    
                }
            });
            
            if (EffectOnSelf != null)
                InstantiateParticleEffect.CreateFx(EffectOnSelf, targetPoring.transform.position);

            AddDamageToTarget(poring, targetPoring, damage);
        }
        else if (targetNode != null)
        {
            // thunder bolt
            poring.transform.LookAt(targetNode.gameObject.transform);
            poring.Animator.Play(AnimationStateName);
            if (EffectOnSelf != null)
                InstantiateParticleEffect.CreateFx(EffectOnSelf, poring.transform.position);
            yield return new WaitForSeconds(2f);

            if (EffectOnTarget != null)
            {   
                InstantiateParticleEffect.CreateFx(EffectOnTarget, targetNode.transform.position);
            }

            AOESkillActivate(poring, targetNode, AOEValue, damage);
        }
        else
        {
            // Magnum Break
            poring.Animator.Play(AnimationStateName);
            yield return new WaitForSeconds(2.5f);
            if (EffectOnSelf != null)
                InstantiateParticleEffect.CreateFx(EffectOnSelf, poring.transform.position);
            AOESkillActivate(poring, poring.Node, AOEValue, damage);
        }

        if (!MoveToTarget && gameMode.IsMineTurn())
            TurnActiveUIController.Instance.SetActivePanel(true, poring.Node.TileProperty.Type);
        else
            TurnActiveUIController.Instance.NotMyTurn();
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
            foreach (var neighbor in currentNode.NeighborList)
            {
                if (neighbor.Node == prevNode) continue;
                if (neighbor.Node.TileProperty.Type != TileType.Sanctuary)
                    AddDamageToTargetNode(poring, damage, neighbor.Node);
                AOESkillActivate(poring, neighbor.Node, value, damage, currentNode);    
            }
        }
    }

    private void AddDamageToTargetNode(Poring poring, float damage, Node node)
    {
        if(IsSelfOnly)
        {
            poring.OnReceiverEffect(SetEffectOwnerIdAndDamage(poring));
            return;
        }

        for (int i = 0; i < node.porings.Count; i++)
        {
            var target = node.porings[i];
            if(target == poring && IsIgnoreSelf)
            {

            }
            else
                AddDamageToTarget(poring, target, damage);
        }
    }

    private void AddDamageToTarget(Poring poring, Poring targetPoring, float damage)
    {
        if(EffectOnHit != null)
        {
            InstantiateParticleEffect.CreateFx(EffectOnHit, targetPoring.transform.localPosition);
        }

        foreach (var s in StrongerList)
        {
            if (targetPoring.CheckHasStatus(s.Status))
            {
                damage *= s.DamageMultiple;
            }
        }
        
        bool isAlive = targetPoring.TakeDamage(poring, damage);
        if(isAlive)
        {
            isAlive = targetPoring.OnReceiverEffect(SetEffectOwnerIdAndDamage(poring));
        }

        if(!isAlive)
            targetPoring.Behavior.Respawn();
    }
}

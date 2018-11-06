using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GreedSkill", menuName = "Poring/Skills/GreedSkill")]
public class GreedSkill : BaseSkill 
{
    private PrototypeGameMode gameMode;
    public override void OnActivate(Poring poring, Poring targetPoring = null, Node targetNode = null, List<Node> nodeList = null)
    {
        gameMode = (PrototypeGameMode)PrototypeGameMode.Instance;

        if(targetPoring != null || targetNode != null)
        {
            GameObject target = (targetPoring != null) ? targetPoring.gameObject : targetNode.gameObject;
            poring.transform.LookAt(target.transform.position);
        }

        SkillEffectActivate(poring, targetPoring, targetNode);
    }

    private void SkillEffectActivate(Poring poring, Poring targetPoring = null, Node targetNode = null)
    {
        PrototypeGameMode.Instance.StartCoroutine(WaitForAnimation(poring, targetPoring, targetNode));
    }

    private IEnumerator WaitForAnimation(Poring poring, Poring targetPoring = null, Node targetNode = null)
    {
        // Magnum Break
        poring.Animator.Play(AnimationStateName);
        yield return new WaitForSeconds(2);
        if (EffectOnSelf != null)
            InstantiateParticleEffect.CreateFx(EffectOnSelf, poring.transform.position);
        AOESkillActivate(poring, poring.Node, AOEValue);
        

        if (!MoveToTarget && gameMode.IsMineTurn())
            TurnActiveUIController.Instance.SetActivePanel(true, poring.Node.TileProperty.Type);
        else
            TurnActiveUIController.Instance.NotMyTurn();
    }

    private void AOESkillActivate(Poring poring, Node currentNode, int value, Node prevNode = null)
    {
        if (value > 0)
        {
            value--;
            foreach (var neighbor in currentNode.NeighborList)
            {
                if (neighbor.Node == prevNode) continue;
                if (neighbor.Node.TileProperty.Type != TileType.Sanctuary)
                    LootNodeTarget(poring, neighbor.Node);
                AOESkillActivate(poring, neighbor.Node, value, currentNode);    
            }
        }
    }

    private void LootNodeTarget(Poring poring, Node node)
    {
        node.PoringKeepValueOnTile(poring, true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreaterSkill", menuName = "Poring/Skills/CreaterSkill")]
public class CreaterSkill : BaseSkill 
{
    public BaseEffectOnTile BaseEffect;

	public override void OnActivate(Poring poring, Poring targetPoring = null, Node targetNode = null, List<Node> nodeList = null)
    {
        if(targetNode == null) targetNode = poring.Node;

        PrototypeGameMode.Instance.StartCoroutine(WaitForAnimation(poring, targetNode));
    }

    private IEnumerator WaitForAnimation(Poring poring, Node targetNode)
    {
        poring.Animator.Play(AnimationStateName);
        InstantiateParticleEffect.CreateFx(EffectOnSelf, poring.transform.position);
        yield return new WaitForSeconds(0.5f);
        
        var effect = Instantiate(BaseEffect);
        effect.transform.SetParent(targetNode.transform);
        effect.transform.position = Vector3.zero;
        targetNode.effectsOnTile.Add(effect.GetComponent<BaseEffectOnTile>());

        if (!MoveToTarget && PrototypeGameMode.Instance.IsMineTurn())
            TurnActiveUIController.Instance.SetActivePanel(true);
        else
            TurnActiveUIController.Instance.NotMyTurn();
    }
}

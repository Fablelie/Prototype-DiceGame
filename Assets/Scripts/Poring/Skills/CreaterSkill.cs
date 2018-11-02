using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreaterSkill", menuName = "Poring/Skills/CreaterSkill")]
public class CreaterSkill : BaseSkill 
{
    public BaseEffectOnTile BaseEffect;
    public bool isDestroyOnTrigger = true;

    public override void Init(BaseSkill baseSkill)
	{
		base.Init(baseSkill);
		var skill = (CreaterSkill)baseSkill;
		BaseEffect         = skill.BaseEffect;
        isDestroyOnTrigger = skill.isDestroyOnTrigger;
	}

	public override void OnActivate(Poring poring, Poring targetPoring = null, Node targetNode = null, List<Node> nodeList = null)
    {
        if(targetNode == null) targetNode = poring.Node;

        PrototypeGameMode.Instance.StartCoroutine(WaitForAnimation(poring, targetNode));
    }
    private Poring p;
    private IEnumerator WaitForAnimation(Poring poring, Node targetNode)
    {
        poring.Animator.Play(AnimationStateName);
        p = poring;
        yield return new WaitForSeconds(0.5f);
        InstantiateParticleEffect.CreateFx(EffectOnSelf, poring.transform.position);
        CurrentCD = TurnCD;
        
        CheckAOEToCreate(targetNode, AOEValue);

        if (PrototypeGameMode.Instance.IsMineTurn())
            TurnActiveUIController.Instance.SetActivePanel(true, poring.Node.TileProperty.Type);
        else
            TurnActiveUIController.Instance.NotMyTurn();
    }

    private void CheckAOEToCreate(Node node, int value, Node prevNode = null)
    {
        if (prevNode == null)
        {
            CreateEffect(node);
        }

        if (value > 0)
        {
            value--;
            foreach (var neighbor in node.NeighborList)
            {
                if (neighbor.Node == prevNode) continue;
                if (neighbor.Node.TileProperty.Type != TileType.Sanctuary)
                    CreateEffect(neighbor.Node);
                CheckAOEToCreate(neighbor.Node, value, node);    
            }
        }
    }

    private void CreateEffect(Node node)
    {
        var effect = Instantiate(BaseEffect);
        effect.LifeDuration = SkillDuration;
        effect.DestroyOnTrigger = isDestroyOnTrigger;
        effect.IsIgnoreSelf = IsIgnoreSelf;

        effect.EffectsDetail.AddRange(SetEffectOwnerIdAndDamage(p));

        effect.transform.SetParent(node.transform);
        effect.transform.localPosition = Vector3.zero;
        node.effectsOnTile.Add(effect);
    }
}

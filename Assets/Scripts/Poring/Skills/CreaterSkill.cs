using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreaterSkill", menuName = "Poring/Skills/CreaterSkill")]
public class CreaterSkill : BaseSkill 
{
    public List<EffectReceiver> effectsReceiver;
    public BaseEffectOnTile BaseEffect;
    public bool isDestroyOnTrigger = true;

    public override void Init(BaseSkill baseSkill)
	{
		base.Init(baseSkill);
		var skill = (CreaterSkill)baseSkill;
		effectsReceiver    = skill.effectsReceiver;
		BaseEffect         = skill.BaseEffect;
        isDestroyOnTrigger = skill.isDestroyOnTrigger;
	}

	public override void OnActivate(Poring poring, Poring targetPoring = null, Node targetNode = null, List<Node> nodeList = null)
    {
        if(targetNode == null) targetNode = poring.Node;

        PrototypeGameMode.Instance.StartCoroutine(WaitForAnimation(poring, targetNode));
    }

    private IEnumerator WaitForAnimation(Poring poring, Node targetNode)
    {
        poring.Animator.Play(AnimationStateName);
        yield return new WaitForSeconds(0.5f);
        InstantiateParticleEffect.CreateFx(EffectOnSelf, poring.transform.position);
        CurrentCD = TurnCD;
        
        // create & set property
        var effect = Instantiate(BaseEffect);
        effect.LifeDuration = SkillDuration;
        effect.DestroyOnTrigger = isDestroyOnTrigger;
        effect.IsIgnoreSelf = IsIgnoreSelf;

        effectsReceiver.ForEach(fx =>
        {
            fx.OwnerId = PrototypeGameMode.Instance.GetPoringIndexByPoring(poring);
            if (fx.Damage == -1) fx.Damage = DamageMultiple * ((DamageType == DamageType.PAtk) ? poring.Property.CurrentPAtk : poring.Property.CurrentMAtk);
        });

        effect.EffectsDetail.AddRange(effectsReceiver);

        effect.transform.SetParent(targetNode.transform);
        effect.transform.localPosition = Vector3.zero;
        targetNode.effectsOnTile.Add(effect);

        if (PrototypeGameMode.Instance.IsMineTurn())
            TurnActiveUIController.Instance.SetActivePanel(true);
        else
            TurnActiveUIController.Instance.NotMyTurn();
    }
}

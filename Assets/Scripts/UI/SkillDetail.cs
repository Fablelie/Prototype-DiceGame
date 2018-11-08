using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillDetail : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public int Damage;
	public DamageType DamageType;
    public string TargetTypestr;
    public bool MoveToTarget;
    public int AOE;
    public string Range;
    public int CD;
    public string Description;

	public SkillDescriptionPanel descriptPanel;

    public void SetDetail(BaseSkill baseSkill, Poring poring)
    {
        float d = (baseSkill.DamageType == DamageType.PAtk) ? poring.Property.CurrentPAtk : poring.Property.CurrentMAtk;
        d *= poring.GetBlessingBuff();
        EffectReceiver maximizePower = poring.GetStatus(SkillStatus.MaximizePower);
        d *= (maximizePower != null) ? maximizePower.Damage : 1;
            
        Damage = (int)((baseSkill.SkillMode == SkillMode.Activate) ? (baseSkill.DamageType == DamageType.PAtk) ? baseSkill.DamageMultiple * poring.Property.CurrentPAtk : baseSkill.DamageMultiple * poring.Property.CurrentMAtk : (baseSkill.DamageType == DamageType.PAtk) ? poring.Property.CurrentPAtk : poring.Property.CurrentMAtk);
        DamageType = baseSkill.DamageType;
        TargetTypestr = (baseSkill.TargetType == TargetType.Another) ? "Poring" : baseSkill.TargetType.ToString();
        MoveToTarget = baseSkill.MoveToTarget;
        AOE = baseSkill.AOEValue;
        Range = $"{baseSkill.MinRangeValue} - {baseSkill.MaxRangeValue}";
        CD = baseSkill.TurnCD;
        Description = baseSkill.Description;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
		descriptPanel.SetDetail(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
		descriptPanel.ClosePanel();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        descriptPanel.ClosePanel();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoringProperty", menuName = "Poring/Property")]
public class PoringProperty : ScriptableObject {
	public GameObject Prefab;
	[Header("Base")]
	public int BaseHp;
	[HideInInspector]public int CurrentHp;
	public int BasePAtk;
	[HideInInspector]public int CurrentPAtk;
	public int BaseMAtk;
	[HideInInspector]public int CurrentMAtk;

	[Header("Max")]
    public int MaxHp;
    public int MaxPAtk;
    public int MaxMAtk;

    [Header("Per one point")]

	// use for mod with value and multiply with current value,
	[Tooltip("use for mod with value and multiply with current value")] public int GrowupHp;
	[Tooltip("use for mod with value and multiply with current value")] public int GrowupPAtk;
	[Tooltip("use for mod with value and multiply with current value")] public int GrowupMAtk;

	private int m_currentPorint = 0; 
	public int CurrentPoint {
		get {
			return m_currentPorint;
		}

		set {
			m_currentPorint = (value > MaxPoint && MaxPoint != -1) ? MaxPoint : value;
			BaseHp = (m_currentPorint <= 0)? BaseHp : ((m_currentPorint * GrowupHp) >= MaxHp)? MaxHp : BaseHp + (m_currentPorint * GrowupHp);
			CurrentPAtk = (m_currentPorint <= 0)? CurrentPAtk : ((m_currentPorint * GrowupPAtk) >= MaxPAtk)? MaxPAtk : BasePAtk + (m_currentPorint * GrowupPAtk);
			CurrentMAtk = (m_currentPorint <= 0)? CurrentMAtk : ((m_currentPorint * GrowupMAtk) >= MaxMAtk)? MaxMAtk : BaseMAtk + (m_currentPorint * GrowupMAtk);
		}
	}

	[Header("Max Point")]
	[Tooltip("set to -1 is infinity")] public int MaxPoint;

	[Header("Starter Dice")]
	
	public List<MoveDice> MoveDices;
	public List<DeffensiveDice> DeffensiveDices;
	public List<OffensiveDice> OffensiveDices;

    public PoringProperty(PoringProperty baseProperty)
    {
        BaseHp = baseProperty.BaseHp;
        CurrentHp = baseProperty.BaseHp;
        BasePAtk = baseProperty.BasePAtk;
        CurrentPAtk = baseProperty.BasePAtk;
        BaseMAtk = baseProperty.BaseMAtk;
        CurrentMAtk = baseProperty.BaseMAtk;
        GrowupHp = baseProperty.GrowupHp;
        GrowupPAtk = baseProperty.GrowupPAtk;
        GrowupMAtk = baseProperty.GrowupMAtk;
        MaxHp = baseProperty.MaxHp;
        MaxMAtk = baseProperty.MaxMAtk;
        MaxPAtk = baseProperty.MaxPAtk;
        MaxPoint = baseProperty.MaxPoint;
        MoveDices = baseProperty.MoveDices;
        DeffensiveDices = baseProperty.DeffensiveDices;
        OffensiveDices = baseProperty.OffensiveDices;
    }

	//public PoringProperty CopyTo(PoringProperty p)
	//{
 //       PoringProperty pro = new PoringProperty()
 //       {
 //           BaseHp = this.BaseHp,

 //       };
	//	p.BaseHp          = BaseHp;
	//	p.CurrentHp       = BaseHp;
	//	p.BasePAtk        = BasePAtk;
	//	p.CurrentPAtk     = BasePAtk;
	//	p.BaseMAtk        = BaseMAtk;
	//	p.CurrentMAtk     = BaseMAtk;
	//	p.GrowupHp        = GrowupHp;
	//	p.GrowupPAtk      = GrowupPAtk;
	//	p.GrowupMAtk      = GrowupMAtk;
	//	p.MaxHp           = MaxHp;
	//	p.MaxMAtk         = MaxMAtk;
	//	p.MaxPAtk         = MaxPAtk;
	//	p.MaxPoint        = MaxPoint;
	//	p.MoveDices       = MoveDices;
	//	p.DeffensiveDices = DeffensiveDices;
	//	p.OffensiveDices  = OffensiveDices;
	//	return p;
	//}

}

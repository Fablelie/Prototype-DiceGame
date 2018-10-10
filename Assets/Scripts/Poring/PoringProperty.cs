using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoringProperty", menuName = "Poring/Property")]
public class PoringProperty : ScriptableObject {
	public GameObject Prefab;
	[Header("Base")]
	public int BaseHp;
	[HideInInspector] public int CurrentMaxHp;
	[HideInInspector]public float CurrentHp;
	public int BasePAtk;
	[HideInInspector]public float CurrentPAtk;
	public int BaseMAtk;
	[HideInInspector]public float CurrentMAtk;

	[Header("Max")]
    public int MaxHp;
    public int MaxPAtk;
    public int MaxMAtk;

    [Header("Per one point")]

	// use for mod with value and multiply with current value,
	[Tooltip("use for mod with value and multiply with current Lv")] public int GrowupHp;
	[Tooltip("use for mod with value and multiply with current Lv")] public int GrowupPAtk;
	[Tooltip("use for mod with value and multiply with current Lv")] public int GrowupMAtk;

	private int m_currentPoint = 0; 
	public int CurrentPoint {
		get {
			return m_currentPoint;
		}

		set {
			m_currentPoint = (value > MaxPoint && MaxPoint != -1) ? MaxPoint : value;
			// UpdateProperty();
		}
	}

	private int m_permanentPoint = 0;
	public int PermanentPoint 
	{
		get 
		{
			return m_permanentPoint;
		}

		set
		{
			m_permanentPoint = value;
			m_currentPoint = 0;
			UpdateProperty();
		}
	}


	[Header("Level")]

	public int CurrentRequestExp = 2;
	public int MaxLevel = 10;
	public int StartRequestExp = 2;
	public int MultiplyPerLevel = 2;
	private int m_currentLevel = 1;
	public int CurrentLevel
	{
		get
		{
			return m_currentLevel;
		}

		set
		{
			m_currentLevel = value;
			UpdateProperty();
		}
	}

	[Header("Max Point")]
	[Tooltip("set to -1 is infinity")] public int MaxPoint;

	[Header("Starter Dice")]
	
	public List<MoveDice> MoveDices;
	public List<DeffensiveDice> DeffensiveDices;
	public List<OffensiveDice> OffensiveDices;

    public void Init(PoringProperty baseProperty)
    {
        BaseHp          = baseProperty.BaseHp;
		CurrentMaxHp    = baseProperty.BaseHp;
        CurrentHp       = baseProperty.BaseHp;
        BasePAtk        = baseProperty.BasePAtk;
        CurrentPAtk     = baseProperty.BasePAtk;
        BaseMAtk        = baseProperty.BaseMAtk;
        CurrentMAtk     = baseProperty.BaseMAtk;
        GrowupHp        = baseProperty.GrowupHp;
        GrowupPAtk      = baseProperty.GrowupPAtk;
        GrowupMAtk      = baseProperty.GrowupMAtk;
        MaxHp           = baseProperty.MaxHp;
        MaxMAtk         = baseProperty.MaxMAtk;
        MaxPAtk         = baseProperty.MaxPAtk;
        MaxPoint        = baseProperty.MaxPoint;
        MoveDices       = baseProperty.MoveDices;
        DeffensiveDices = baseProperty.DeffensiveDices;
        OffensiveDices  = baseProperty.OffensiveDices;

		CurrentLevel          = 1;
		CurrentRequestExp     = baseProperty.StartRequestExp;
		MaxLevel              = baseProperty.MaxLevel;
		StartRequestExp       = baseProperty.StartRequestExp;
		MultiplyPerLevel      = baseProperty.MultiplyPerLevel;
    }

	private void UpdateProperty()
	{
		CurrentMaxHp = BaseHp + (m_currentLevel * GrowupHp);
		CurrentPAtk  = BasePAtk + (m_currentLevel * GrowupPAtk);
		CurrentMAtk  = BaseMAtk + (m_currentLevel * GrowupMAtk);
	}
}

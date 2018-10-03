using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoringProperty", menuName = "Poring/Property")]
public class PoringProperty : ScriptableObject {
	public GameObject Prefab;
	[Header("Base")]
	public int BaseHp;
	[HideInInspector] public int CurrentMaxHp;
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

	private int m_currentPoint = 0; 
	public int CurrentPoint {
		get {
			return m_currentPoint;
		}

		set {
			m_currentPoint = (value > MaxPoint && MaxPoint != -1) ? MaxPoint : value;
			UpdateProperty();
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

	[Header("Max Point")]
	[Tooltip("set to -1 is infinity")] public int MaxPoint;

	[Header("Starter Dice")]
	
	public List<MoveDice> MoveDices;
	public List<DeffensiveDice> DeffensiveDices;
	public List<OffensiveDice> OffensiveDices;

    public PoringProperty(PoringProperty baseProperty)
    {
        BaseHp = baseProperty.BaseHp;
		CurrentMaxHp = baseProperty.BaseHp;
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

	private void UpdateProperty()
	{
		int result = m_currentPoint + m_permanentPoint;

		CurrentMaxHp = (result <= 0)? CurrentMaxHp: ((result * GrowupHp) + BaseHp >= MaxHp)		? MaxHp  : BaseHp + (result * GrowupHp);
		CurrentPAtk  = (result <= 0)? CurrentPAtk : ((result * GrowupPAtk) + BasePAtk >= MaxPAtk)	? MaxPAtk: BasePAtk + (result * GrowupPAtk);
		CurrentMAtk  = (result <= 0)? CurrentMAtk : ((result * GrowupMAtk) + BaseMAtk >= MaxMAtk)	? MaxMAtk: BaseMAtk + (result * GrowupMAtk);

		Debug.LogFormat("Result : {0}", result);
		Debug.LogFormat("Current Point : {0}", m_currentPoint);
		Debug.LogFormat("Permanent Point : {0}", m_permanentPoint);
		Debug.LogFormat("Current HP : {0}", CurrentMaxHp);
		Debug.LogFormat("Current PAtk : {0}", CurrentPAtk);
		Debug.LogFormat("Current MAtk : {0}", CurrentMAtk);
	}
}

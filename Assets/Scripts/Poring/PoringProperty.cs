﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoringProperty", menuName = "Poring/Property")]
public class PoringProperty : ScriptableObject {
	public GameObject Prefab;
	[Header("Base")]
	public int BaseHp;
	[HideInInspector] public int CurrentMaxHp;
	private float m_currentHp;
	[HideInInspector]public float CurrentHp {
		get {
			return m_currentHp;
			}
		set {
			m_currentHp = value;
			if (m_currentHp > CurrentMaxHp) m_currentHp = CurrentMaxHp;
			}
	}
	public int BasePAtk;
	[HideInInspector]public float CurrentPAtk;
	public int BaseMAtk;
	[HideInInspector]public float CurrentMAtk;
	public int AttackRange;	

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

	public List<BaseSkill> SkillList = new List<BaseSkill>();
	public BaseSkill UltimateSkill;

	[Header("NormalAttackEffect!!")]
	public GameObject NormalAttackEffect;

	public Sprite JobImage;

	[HideInInspector] public int UltimatePoint = 0;

    public void Init(PoringProperty baseProperty)
    {
        BaseHp      	= baseProperty.BaseHp;
		CurrentMaxHp    = baseProperty.BaseHp;
        CurrentHp   	= baseProperty.BaseHp;
        BasePAtk    	= baseProperty.BasePAtk;
        CurrentPAtk 	= baseProperty.BasePAtk;
        BaseMAtk    	= baseProperty.BaseMAtk;
        CurrentMAtk 	= baseProperty.BaseMAtk;
		AttackRange     = baseProperty.AttackRange;

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

		// normal skill
		baseProperty.SkillList.ForEach(baseSkill => 
		{
			Type type = baseSkill.GetType();
			
			var skill = ScriptableObject.CreateInstance(type.ToString()) as BaseSkill;
			skill.Init(baseSkill);
			SkillList.Add(skill);
		});

		// Ultimate skill
		Type t = baseProperty.UltimateSkill.GetType();
		var s = ScriptableObject.CreateInstance(t.ToString()) as BaseSkill;
		s.Init(baseProperty.UltimateSkill);
		UltimateSkill = s;

		NormalAttackEffect = baseProperty.NormalAttackEffect;
		JobImage = baseProperty.JobImage;
    }

	private void UpdateProperty()
	{
		CurrentMaxHp = BaseHp + ((m_currentLevel-1) * GrowupHp);
		CurrentPAtk  = BasePAtk + ((m_currentLevel-1) * GrowupPAtk);
		CurrentMAtk  = BaseMAtk + ((m_currentLevel-1) * GrowupMAtk);

		CurrentMaxHp = (CurrentMaxHp <= MaxHp) ? CurrentMaxHp : MaxHp;
		CurrentPAtk = (CurrentPAtk <= MaxPAtk) ? CurrentPAtk : MaxPAtk;
		CurrentMAtk = (CurrentMAtk <= MaxMAtk) ? CurrentMAtk : MaxMAtk;
	}
}

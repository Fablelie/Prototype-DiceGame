using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoringProperty", menuName = "Poring/Property")]
public class PoringProperty : ScriptableObject {
	public GameObject Prefab;
	public int BaseHp;
	[HideInInspector]public int CurrentHp;
	public int BasePAtk;
	public int BaseMAtk;

	// use for mod with value and multiply with current value,
	[Tooltip("use for mod with value and multiply with current value")] public int GrowupHp;
	[Tooltip("use for mod with value and multiply with current value")] public int GrowupPAtk;
	[Tooltip("use for mod with value and multiply with current value")] public int GrowupMAtk;

	public PoringProperty CopyTo(PoringProperty p)
	{
		p.BaseHp     = BaseHp;
		p.CurrentHp  = BaseHp;
		p.BasePAtk   = BasePAtk;
		p.BaseMAtk   = BaseMAtk;
		p.GrowupHp   = GrowupHp;
		p.GrowupPAtk = GrowupPAtk;
		p.GrowupMAtk = GrowupMAtk;
		return p;
	}

}

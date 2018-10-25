using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEffectOnTile : MonoBehaviour {
	public EffectReceiver EffectDetail;
	public GameObject EffectOnTrigger;

	public EffectReceiver OnEnter()
	{
		InstantiateParticleEffect.CreateFx(EffectOnTrigger, transform.position);
		return EffectDetail;
	}

	public void CountDownLifeDuration(Node node)
	{
		EffectDetail.LifeDuration--;
		if(EffectDetail.LifeDuration <= 0)
		{
			node.effectsOnTile.Remove(this);
			Destroy(this);
		}
	}

}

[System.Serializable]
public struct EffectReceiver
{
	public int OwnerId;
	public float Damage;
	public int LifeDuration;
	public int EffectDuration;
	public bool DestroyOnTrigger;
	public SkillStatus Status; 
}

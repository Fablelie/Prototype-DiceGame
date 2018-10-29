using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEffectOnTile : MonoBehaviour {
	public List<EffectReceiver> EffectsDetail;
	
	[HideInInspector] public bool DestroyOnTrigger;
	[HideInInspector] public int LifeDuration;
	[HideInInspector] public bool IsIgnoreSelf;
	public GameObject EffectOnTrigger;


	public List<EffectReceiver> OnEnter(Node node)
	{
		InstantiateParticleEffect.CreateFx(EffectOnTrigger, transform.position);
		if(DestroyOnTrigger) 
		{
			node.effectsOnTile.Remove(this);
			Destroy(gameObject);
		}
		return EffectsDetail;
	}

	public void CountDownLifeDuration(Node node)
	{
		LifeDuration--;
		if(LifeDuration <= 0)
		{
			node.effectsOnTile.Remove(this);
			Destroy(gameObject);
		}
	}

}



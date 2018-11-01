using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Posion", menuName = "Node/Property/Posion")]
public class TilePosion : TileProperty 
{
    [SerializeField] private GameObject particle;
    [SerializeField] private int tileDamage = 2;
    public override void OnEnter(Poring poring, Node node)
    {
        var e = new EffectReceiver(){
            OwnerId = -1,
            Damage = tileDamage,
            EffectDuration = 1,
            Status = SkillStatus.Posion
        };
        
        InstantiateParticleEffect.CreateFx(particle, poring.transform.position);
        poring.OnReceiverEffect(new List<EffectReceiver>(){e});
    }
}

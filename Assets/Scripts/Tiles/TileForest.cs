using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Forest", menuName = "Node/Property/Forest")]
public class TileForest : TileProperty
{
    [SerializeField] private GameObject particle;
    public override void OnFinish(Poring poring, Node node)
    {
        var e = new EffectReceiver(){
            EffectDuration = 1,
            Status = SkillStatus.Ambursh
        };
        
        InstantiateParticleEffect.CreateFx(particle, poring.transform.position);
        poring.OnReceiverEffect(new List<EffectReceiver>(){e});
    }
}

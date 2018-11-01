using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WarpOut", menuName = "Node/Property/WarpOut")]
public class TileWarpOut : TileProperty 
{
    public void WarpReceivePoring(Poring poring, Node node)
    {
        if(PrototypeGameMode.Instance.CheckAnotherPoringInTargetNode(node))
        {
            poring.Target = node.porings.Find(p => p != poring);

            poring.AttackResultIndex = Random.Range(0,5);
            poring.DefendResultIndex = Random.Range(0,5);

            poring.Target.AttackResultIndex = Random.Range(0,5);
            poring.Target.DefendResultIndex = Random.Range(0,5);
        }
    }
}

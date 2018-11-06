using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WarpIn", menuName = "Node/Property/WarpIn")]
public class TileWarpIn : TileProperty 
{
    public override void OnFinish(Poring poring, Node node)
    {
        PrototypeGameMode gameMode = PrototypeGameMode.Instance;

        List<Node> nodes = new List<Node>();
        
        foreach (var n in gameMode.Nodes)
        {
            if(n != node && n.TileProperty.Type == TileType.WarpOut)
                nodes.Add(n);
        }

        WarpTo(poring, nodes[0]);
        
    }

    private void WarpTo(Poring poring, Node node)
    {
        TileWarpOut o = (TileWarpOut)node.TileProperty;

        poring.Node.RemovePoring(poring);
        node.AddPoring(poring);
        poring.transform.position = node.transform.position;
        poring.Animator.Play("Warp_down");
        o.WarpReceivePoring(poring, node);
    }
}

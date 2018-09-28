using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeGameMode : GameMode
{

    public Node[] Nodes;
    [SerializeField] private Camera[] cameras;

    public override void OnRollEnd(int number)
    {
        throw new System.NotImplementedException();
    }

    public override void StartGameMode()
    {
        Turn = 0;
    }

    public override void UpdateGameMode()
    {
        throw new System.NotImplementedException();
    }
}

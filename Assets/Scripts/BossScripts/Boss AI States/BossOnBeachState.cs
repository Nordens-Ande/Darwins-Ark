using Assets.Scripts.Environment;
using System;
using UnityEngine;

public class BossOnBeachState : BossAIState
{
    TileManager tileManager = TileManager.Instance;
    public BossOnBeachState(Boss boss) : base(boss)
    {
        container = new BossOnBeachContainer();
    }

    public override bool OnEnterState()
    {
        TaskCompleted = false;
        bool foundPath = boss.MovementScript.SetPathFromBeachToGrass();
        if(foundPath)
        {
            CanBeInterupted = false;
            return true;
        }
        return false;
    }

    public override void Tick()
    {
        CanBeInterupted = false;
        (bool reachedTargetTile, bool reachedNextTile) = boss.MovementScript.BossMove();
        if (reachedTargetTile)
        {
            CanBeInterupted = true;
            TaskCompleted = true;
        }
    }

    public override void OnExitState()
    {

    }
}

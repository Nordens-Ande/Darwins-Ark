using Assets.Scripts.Environment;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BossTerraMoveState : BossAIState
{
    private TileManager tileManager;

    public override void Initialize(Boss boss)
    {
        tileManager = TileManager.Instance;    
    }

    public override bool OnEnterState()
    {
        canBeInterupted = false;
        taskCompleted = false;

        BossTerraManager bossTerraManager = boss.BossTypeManager as BossTerraManager;
        Tile tile = bossTerraManager.CurrentAttackTile;
        List<Tile> path = new List<Tile>();

        path = boss.MovementScript.PathFinder.GetPathAStar(tileManager.GetTile(boss.transform.position), tile, boss.MovementScript.IsBossTraversableBeach, boss.MovementScript.MaxHeightDifference, 0);
        if (path == null)
        {
            return false;
        }

        return true;
    }

    public override void Tick()
    {
        (bool reachedTarget, bool reachedNext) = boss.MovementScript.BossMove();
        if(reachedNext)
        {
            canBeInterupted = true;
        }
        if(reachedTarget)
        {
            Debug.LogWarning("reached target in BossTerraMoveState, this shouldnt happen, tell Milton if you see this"); // something aint right if this pops up, new state should have been entered before this, check evaluation of states
            taskCompleted = true;
        }
    }

    public override void OnExitState()
    {
        
    }
}

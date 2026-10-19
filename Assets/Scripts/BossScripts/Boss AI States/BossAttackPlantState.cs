using UnityEngine;
using Assets.Scripts.Environment;
using System;


public class BossAttackPlantState : BossAIState
{
    Tile plantTile;
    bool atPlantTile;

    float attackTimer;

    public BossAttackPlantState(Boss boss) : base(boss)
    {
        container = new BossAttackPlantContainer();
        atPlantTile = false;
        attackTimer = 0;
    }

    public override bool OnEnterState()
    {
        CanBeInterupted = false;
        TaskCompleted = false;

        if (boss.ClosestPlantTilePath == null) return false;
        plantTile = boss.ClosestPlantTilePath[boss.ClosestPlantTilePath.Count - 1];
        if (plantTile == null) return false;

        boss.MovementScript.SetPath(plantTile, boss.MovementScript.IsBossTraversable);
        atPlantTile = false;
        attackTimer = 0;
        return true;
    }

    public override void Tick()
    {
        CanBeInterupted = false;

        if (!atPlantTile)
        {
            (bool reachedTargetTile, bool reachedNextTile) = boss.MovementScript.BossMove();
            
            if(reachedNextTile && !reachedTargetTile)
            {
                CanBeInterupted = true;
            }
            atPlantTile = reachedTargetTile;
        }
        else
        {
            if (attackTimer >= boss.AttackTime)
            {
                if(plantTile.CurrentPlant != null)
                {
                    plantTile.CurrentPlant.DamagePlant(100f);
                }
                TaskCompleted = true;
                CanBeInterupted = true;
                //OnEnterState();
            }
            attackTimer += Time.deltaTime;
        }
    }

    public override void OnExitState()
    {
        
    }
}

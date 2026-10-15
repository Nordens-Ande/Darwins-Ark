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
        CanBeInterupted = true;
        plantTile = boss.FindClosestPlantTile();
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
            atPlantTile = reachedTargetTile;
            if(reachedNextTile && !reachedTargetTile)
            {
                CanBeInterupted = true;
            }
        }
        else
        {
            if (attackTimer >= boss.AttackTime)
            {
                plantTile.ClearPlant();
                OnEnterState();
            }
            attackTimer += Time.deltaTime;
        }
    }

    public override void OnExitState()
    {
        
    }
}

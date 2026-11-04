using UnityEngine;
using Assets.Scripts.Environment;
using System;

[Serializable]
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

    public override void Initialize(Boss boss)
    {
        this.boss = boss;
        container = new BossAttackPlantContainer();
    }

    public override bool OnEnterState()
    {
        CanBeInterupted = false;
        TaskCompleted = false;

        if (boss.ClosestAttackTilePath == null) return false;
        plantTile = boss.ClosestAttackTilePath[boss.ClosestAttackTilePath.Count - 1];
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
            }
            attackTimer += Time.deltaTime;
        }
    }

    public override void OnExitState()
    {
        
    }
}

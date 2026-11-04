using Unity.VisualScripting;
using UnityEngine;
using Assets.Scripts.Environment;

public class BossAttackRiverState : BossAIState
{
    Tile riverTile;
    bool atRiverTile;

    float attackTimer;

    public BossAttackRiverState(Boss boss) : base(boss)
    {
        this.boss = boss;
        container = new BossAttackRiverContainer();
    }

    public override void Initialize(Boss boss)
    {
        this.boss = boss;
        container = new BossAttackRiverContainer();
    }

    public override bool OnEnterState()
    {
        CanBeInterupted = false;
        TaskCompleted = false;

        if (boss.ClosestAttackTilePath == null) return false;
        riverTile = boss.ClosestAttackTilePath[boss.ClosestAttackTilePath.Count - 1];
        if (riverTile == null) return false;

        boss.MovementScript.SetPath(riverTile, boss.MovementScript.IsBossTraversable);
        atRiverTile = false;
        attackTimer = 0;
        return true;
    }

    public override void Tick()
    {
        CanBeInterupted = false;

        if (!atRiverTile)
        {
            (bool reachedTargetTile, bool reachedNextTile) = boss.MovementScript.BossMove();

            if (reachedNextTile && !reachedTargetTile)
            {
                CanBeInterupted = true;
            }
            atRiverTile = reachedTargetTile;
        }
        else
        {
            if (attackTimer >= boss.AttackTime)
            {
                //hi mr Elliot, I need support to change tile types and change mesh?
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

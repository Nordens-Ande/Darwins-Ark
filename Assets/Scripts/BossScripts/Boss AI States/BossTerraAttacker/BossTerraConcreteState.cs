using Assets.Scripts.Environment;
using UnityEngine;

public class BossTerraConcreteState : BossAIState
{
    private BossTerraManager bossTerraManager;
    private TileManager tileManager;

    public override void Initialize(Boss boss)
    {
        this.boss = boss;
        bossTerraManager = gameObject.GetComponent<BossTerraManager>();
        container = new BossConcreteContainer();
        tileManager = TileManager.Instance;
    }

    public override bool OnEnterState()
    {
        canBeInterupted = false;
        taskCompleted = false;

        return true;
    }

    public override void Tick()
    {
        (bool reachedTarget, bool reachedNext) = boss.MovementScript.BossMove();
        if (reachedTarget)
        {
            //add timer
            //lay concrete
                //tile type
                //tile visual
            //if("finished")
            //{
            //    canBeInterupted = true;
            //    taskCompleted = true;
            //}
        }
    }

    public override void OnExitState()
    {
       
    }
}

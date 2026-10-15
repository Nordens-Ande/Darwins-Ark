using System;
using UnityEngine;

public interface IBossAIState
{
    public bool CanBeInterupted
    { get; set; }

    public bool OnEnterState();
    public void Tick();
    public void OnExitState();
}


public abstract class BossAIState : IBossAIState
{
    protected Boss boss;
    protected Container<Boss> container;
    bool canBeInterupted;

    public bool CanBeInterupted
    {
        get { return canBeInterupted; }
        set { canBeInterupted = value; }
    }

    public BossAIState(Boss boss)
    {
        this.boss = boss;
        canBeInterupted = true;
    }

    public abstract bool OnEnterState();

    public abstract void Tick();

    public abstract void OnExitState();
    
    public float Evaluate()
    {
        return container.Evaluate(boss);
    }
}

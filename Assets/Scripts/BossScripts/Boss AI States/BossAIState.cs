using System;
using UnityEngine;

public interface IBossAIState
{
    public bool CanBeInterupted
    { get; set; }

    public void Initialize(Boss boss);

    public bool OnEnterState();
    public void Tick();
    public void OnExitState();
}

[Serializable]
public abstract class BossAIState : IBossAIState
{
    [NonSerialized]
    protected Boss boss;
    protected Container<Boss> container;
    protected bool canBeInterupted;
    protected bool taskCompleted;

    public bool CanBeInterupted
    {
        get { return canBeInterupted; }
        set { canBeInterupted = value; }
    }

    public bool TaskCompleted
    {
        get { return taskCompleted; }
        set { taskCompleted = value; }
    }

    public BossAIState(Boss boss)
    {
        this.boss = boss;
    }

    public abstract void Initialize(Boss boss);

    public abstract bool OnEnterState();

    public abstract void Tick();

    public abstract void OnExitState();
    
    public float Evaluate()
    {
        return container.Evaluate(boss);
    }
}

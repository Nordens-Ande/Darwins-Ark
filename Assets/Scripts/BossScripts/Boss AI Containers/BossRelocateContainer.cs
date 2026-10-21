using UnityEngine;

public class BossRelocateContainer : Container<Boss>
{
    public override float Evaluate(Boss boss)
    {
        if (boss.DebugScores)
            Debug.Log("Boss relocate score: 0.5");
        return 0.5f;
    }
}

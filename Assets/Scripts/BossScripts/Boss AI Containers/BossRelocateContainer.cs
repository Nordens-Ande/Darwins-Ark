using UnityEngine;

public class BossRelocateContainer : Container<Boss>
{
    public override float Evaluate(Boss boss)
    {
        return 0.5f;
    }
}

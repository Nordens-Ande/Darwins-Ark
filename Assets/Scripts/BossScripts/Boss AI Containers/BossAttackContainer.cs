using UnityEngine;

public class BossAttackContainer : Container<Boss>
{
    public override float Evaluate(Boss boss)
    {
        //distance to target tile (plant, river tile etc)
        return 0f;
    }
}

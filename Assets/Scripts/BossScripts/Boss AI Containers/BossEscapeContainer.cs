using UnityEngine;

public class BossEscapeContainer : Container<Boss>
{
    public override float Evaluate(Boss boss)
    {
        //distance to closest target (plant, river tile or other tile that boss will attack)
        //health
        //distance to closest animal, hmm
        return 0f;
    }
}

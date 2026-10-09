using Assets.Scripts.Environment;
using UnityEngine;

public class BossSeekContainer : Container<Boss>
{
    public override float Evaluate(Boss boss)
    {
        if(boss.CurrentTile.Type == TileType.Beach)
        {
            return 1f;
        }
        else
        {
            return 0f;
        }
    }
}

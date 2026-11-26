using Assets.Scripts.Environment;
using UnityEngine;

public class BossConcreteContainer : Container<Boss>
{
    private TileManager tileManager = TileManager.Instance;
    public override float Evaluate(Boss boss)
    {
        if (tileManager == null)
        {
            Debug.LogWarning("tileManager null in bossConcreteContainer");
            return 0f;
        }

        BossTerraManager manager = boss.BossTypeManager as BossTerraManager;
        if (manager == null)
        {
            Debug.LogWarning("cast failed in BossConcreteContainer");
            return 0f;
        }

        if (tileManager.GetTile(boss.transform.position) == boss.ClosestAttackTilePath[^2] && manager.AttackTileIsLevel) return 1f;
        else if (tileManager.GetTile(boss.transform.position) == boss.ClosestAttackTilePath[^1]) return 1f;
        return 0f;
    }
}

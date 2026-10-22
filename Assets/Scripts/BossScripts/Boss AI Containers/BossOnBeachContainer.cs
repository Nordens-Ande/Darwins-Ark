using Assets.Scripts.Environment;
using UnityEngine;

public class BossOnBeachContainer : Container<Boss>
{
    TileManager tileManager = TileManager.Instance;

    public override float Evaluate(Boss boss)
    {
        if (tileManager == null) return 0f;

        Tile tile = tileManager.GetTile(boss.transform.position);
        if (tile == null) return 0f;

        if (tile.Type == TileType.Beach)
        {
            if (boss.DebugScores)
                Debug.Log("Boss on beach score: 1");
            return 1f;
        }
        else
        {
            if (boss.DebugScores)
                Debug.Log("Boss on beach score: 0");
            return 0f;
        }
    }
}

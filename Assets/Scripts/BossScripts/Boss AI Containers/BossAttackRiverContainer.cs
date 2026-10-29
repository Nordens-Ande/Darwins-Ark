using UnityEngine;
using Assets.Scripts.Environment;

public class BossAttackRiverContainer : Container<Boss>
{
    public override float Evaluate(Boss boss)
    {
        if (boss.ClosestAttackTilePath == null) return 0f;//no river tiles that are reachable
        Tile riverTile = boss.ClosestAttackTilePath[boss.ClosestAttackTilePath.Count - 1];
        if (riverTile == null) return 0f;

        float distance = Vector3.Distance(boss.transform.position, riverTile.position);
        if (distance >= boss.SightDistanceToTiles) return 0f; // too far

        if (boss.DebugScores)
            Debug.Log("Boss attack river score: " + (1 - distance / boss.SightDistanceToTiles));

        return 1 - distance / boss.SightDistanceToTiles; //should be 1 - distance / boss bla bla
        
    }
}

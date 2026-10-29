using UnityEngine;
using Assets.Scripts.Environment;

public class BossAttackPlantContainer : Container<Boss>
{
    public override float Evaluate(Boss boss)
    {
        if (boss.ClosestAttackTilePath == null) return 0f;//no plants that are reachable
        Tile plantTile = boss.ClosestAttackTilePath[boss.ClosestAttackTilePath.Count - 1];
        if (plantTile == null) return 0f; 
        
        float distance = Vector3.Distance(boss.transform.position, plantTile.position);
        if(distance >= boss.SightDistanceToTiles) return 0f; // too far

        if(boss.DebugScores)
            Debug.Log("Boss attack plant score: " + (1 - distance / boss.SightDistanceToTiles));

        return 1 - distance / boss.SightDistanceToTiles; //should be 1 - distance / boss bla bla
    }
}

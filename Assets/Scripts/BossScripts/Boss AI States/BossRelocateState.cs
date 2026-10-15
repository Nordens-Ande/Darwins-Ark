using System.Collections.Generic;
using Assets.Scripts.Environment;
using UnityEngine;


public class BossRelocateState : BossAIState
{
    TileManager tileManager = TileManager.Instance;

    float minRelocateDistance = 10;
    float maxRelocateDistance = 20;

    public BossRelocateState(Boss boss) : base(boss)
    {
        container = new BossRelocateContainer();
    }

    public override bool OnEnterState()
    {
        TaskCompleted = false;
        CanBeInterupted = false;

        List<Tile> path = FindTileToRelocate();
        if (path != null)
        {
            boss.MovementScript.CurrentPath = path;
            //CanBeInterupted = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    List<Tile> FindTileToRelocate()
    {
        List<Tile> path = new List<Tile>();

        List<Tile> possibleTiles = new List<Tile>();
        foreach(Chunk chunk in tileManager.chunks)
        {
            foreach(Tile tile in chunk.Tiles)
            {
                if (tile.Type != TileType.Grass && tile.Type != TileType.Concrete && tile.Type != TileType.River) continue;
                float distance = Vector3.Distance(tile.position, boss.transform.position);
                if (distance < minRelocateDistance && distance > maxRelocateDistance) continue;
                possibleTiles.Add(tile);
            }
        }
       
        for (int i = possibleTiles.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (possibleTiles[i], possibleTiles[j]) =
                (possibleTiles[j], possibleTiles[i]);
        }

        Tile startTile = tileManager.GetTile(boss.transform.position);
        if (startTile == null) return null;

        foreach(Tile tile in possibleTiles)
        {
            path = boss.MovementScript.PathFinder.GetPathAStar(tileManager.GetTile(boss.transform.position), tile, boss.MovementScript.IsBossTraversable, boss.MovementScript.MaxHeightDifference, 0);
            if (path != null)
            {
                return path;
            }
        }
        return null;
    }

    public override void Tick()
    {
        CanBeInterupted = false;
        (bool reachedTargetTile, bool reachedNextTile) = boss.MovementScript.BossMove();
        if(reachedNextTile)
        {
            CanBeInterupted = true;
        }
        if(reachedTargetTile)
        {
            TaskCompleted = true;
            CanBeInterupted = true;
        }
    }

    public override void OnExitState()
    {

    }
}

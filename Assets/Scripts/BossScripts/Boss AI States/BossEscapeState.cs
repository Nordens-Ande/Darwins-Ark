using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Environment;
using System;
using System.Net;

[Serializable]
public class BossEscapeState : BossAIState
{
    private TileManager tileManager;
    private const int escapeDistance = 5;
    private const int searchRange = 5;

    private int tilesEscaped;

    public BossEscapeState(Boss boss) : base(boss)
    {
        container = new BossEscapeContainer();
    }

    public override void Initialize(Boss boss)
    {
        this.boss = boss;
        container = new BossEscapeContainer();
        tileManager = TileManager.Instance;
    }

    public override bool OnEnterState()
    {
        Debug.Log("tried to enter escape");
        TaskCompleted = false;

        tilesEscaped = 0;

        List<AnimalAI> animals = boss.FindClosestAnimals(3);

        if (animals.Count <= 0) return false;

        Vector3 averageDir = Vector3.zero;
        for(int i = 0;  i < animals.Count; i++)
        {
            Vector3 direction = animals[i].transform.position - boss.transform.position;
            float weight = 1f / direction.magnitude;
            averageDir += direction.normalized * weight;
        }
        averageDir = Vector3.Normalize(averageDir);
        Vector3 directionToEscape = -averageDir;

        List<Tile> path = FindReachableTile(directionToEscape, animals);
        if(path != null)
        {
            Debug.Log("found path in boss escape its enterable");
            boss.MovementScript.CurrentPath = path;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// In the escape direction find a reachable tile
    /// </summary>
    /// <param name="direction">Escape direction</param>
    /// <returns>Path to the reachable tile</returns>
    private List<Tile> FindReachableTile(Vector3 direction, List<AnimalAI> animals) // use surroundtiles
    {
        List<Tile> path;

        Vector3 startTile = boss.transform.position + (direction * escapeDistance);

        for (int i = 0; i < searchRange; i++)
        {
            Vector2 position = startTile + direction * i;
            Tile tile = tileManager.GetTile(position);

            if (tile.Type != TileType.Grass && tile.Type != TileType.River && tile.Type != TileType.Concrete) continue;

            if (tile == null)
            {
                Debug.Log("no escape tile found for boss");
                continue;
            }
            else
            {

                Debug.Log("boss pos: " + boss.transform.position);
                Debug.Log("escape tile position for boss" + tile.Position);
                
            }
            path = boss.MovementScript.PathFinder.GetPathAStar(tileManager.GetTile(boss.transform.position), tile, boss.MovementScript.IsBossTraversable, boss.MovementScript.MaxHeightDifference, (escapeDistance + searchRange) * 2);
            if (path != null)
            {
                return path;
            }
        }
        //if no path found return null, this will make the state "unEnterable" and second best scoring state will be selected instead
        return null;
    }

    private List<Tile> GetSurroundingTiles() //maybe better solution, do this pal, makes smarter escape, choose th etile that is furthest and pathfindable
    {
        List<Tile> tiles = new List<Tile>();
        Tile currentTile = tileManager.GetTile(boss.transform.position);
        Vector2 currentPos = new Vector2(currentTile.Position.x, currentTile.Position.z);
        foreach(Chunk chunk in tileManager.chunks)
        {
            foreach(Vector2Int tilePos in chunk.GetTiles(TileType.Grass))
            {
                float distance = Vector2.Distance(tilePos, currentPos);
                if (distance < escapeDistance || distance > escapeDistance + searchRange) continue;
                tiles.Add(tileManager.GetTile(tilePos));
            }
            foreach(Vector2Int tilePos in chunk.GetTiles(TileType.Concrete))
            {
                float distance = Vector2.Distance(tilePos, currentPos);
                if (distance < escapeDistance || distance > escapeDistance + searchRange) continue;
                tiles.Add(tileManager.GetTile(tilePos));
            }
        }
        return tiles;
    }

    public override void Tick()
    {
        CanBeInterupted = false;
        (bool reachedTargetTile, bool reachedNextTile) = boss.MovementScript.BossMove();
        if (reachedNextTile)
        {
            tilesEscaped += 1;
            Debug.Log("escape 1 tile");
            if(tilesEscaped >= escapeDistance)
            {
                Debug.Log("escape distance reached");
                CanBeInterupted = true;
            }
            //else //for later to reroll tile, if new animals are closer as we progress, make it smart pal
            //{
            //    OnExitState();
            //    OnEnterState();
            //}
        }
        if (reachedTargetTile)
        {
            TaskCompleted = true;
            CanBeInterupted = true;
        }
    }

    public override void OnExitState()
    {
        
    }
}

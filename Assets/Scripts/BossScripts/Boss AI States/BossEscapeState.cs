using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Environment;
using System;


public class BossEscapeState : BossAIState
{
    TileManager tileManager = TileManager.Instance;
    static int escapeDistance = 5;
    static int searchRange = 10;

    public BossEscapeState(Boss boss) : base(boss)
    {
        container = new BossEscapeContainer();
    }

    public override bool OnEnterState()
    {
        TaskCompleted = false;

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
    List<Tile> FindReachableTile(Vector3 direction, List<AnimalAI> animals)
    {
        List<Tile> path = new List<Tile>();

        Vector3 startTile = boss.transform.position + (direction * escapeDistance);

        for (int i = 0; i < searchRange; i++)
        {
            Vector2 position = startTile + direction * i;
            Tile tile = tileManager.GetTile(position);
            if (tile == null)
            {
                continue;
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

    //List<Tile> GetSurroundingTiles() //maybe better solution
    //{

    //}

    public override void Tick()
    {
        CanBeInterupted = false;
        (bool reachedTargetTile, bool reachedNextTile) = boss.MovementScript.BossMove();
        if (reachedNextTile)
        {
            CanBeInterupted = true;
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

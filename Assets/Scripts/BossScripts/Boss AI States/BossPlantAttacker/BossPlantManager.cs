using Assets.Scripts.Environment;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// this class is part of boss ai, boss type that attacks plants
/// </summary>
public class BossPlantManager : BossTypeManager
{
    private TileManager tileManager;
    private Boss boss;

    public override void Initialize(Boss boss)
    {
        tileManager = TileManager.Instance;
        this.boss = boss;
    }

    List<Tile> FindAllTilesWithPlants()
    {
        List<Tile> tilesWithPlants = new List<Tile>();
        foreach (Chunk chunk in tileManager.chunks)
        {
            foreach (Tile tile in chunk.Tiles)
            {
                if (tile.HasPlant)
                {
                    tilesWithPlants.Add(tile);
                }
            }
        }
        //Debug.Log("tile with plants: " + tilesWithPlants.Count);
        return tilesWithPlants;
    }

    /// <summary>
    /// Finds the closest reachable plant by the boss.
    /// </summary>
    /// <returns>Returns the Tile the plant is on, or null if tile is unreachable</returns>
    public void FindClosestPlantTile()
    {
        List<Tile> tilesWithPlants = FindAllTilesWithPlants();
        if (tilesWithPlants.Count <= 0)
        {
            boss.ClosestAttackTilePath = null;
            return;
        }

        tilesWithPlants = tilesWithPlants.OrderBy(tile => Vector3.Distance(transform.position, tile.Position)).ToList();

        for (int i = 0; i < tilesWithPlants.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, tilesWithPlants[i].Position);
            if (distance > boss.SightDistanceToTiles) continue;

            boss.ClosestAttackTilePath = boss.MovementScript.PathFinder.GetPathAStar(tileManager.GetTile(transform.position), tilesWithPlants[i], boss.MovementScript.IsBossTraversable, 2f, (int)(boss.SightDistanceToTiles * 2));
            if (boss.ClosestAttackTilePath != null)
            {
                return;
            }
        }
    }

    public override void OnTick()
    {
        FindClosestPlantTile();
    }

    void Update()
    {
        
    }
}
